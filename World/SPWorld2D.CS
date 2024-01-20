using SimpleECS;
using SimplePhysics2D.BoudingBox;
using SimplePhysics2D.Collision;
using SimplePhysics2D.QuadTree;
using SimplePhysics2D.Raycast;
using SimplePhysics2D.RigidBody;
using SimplePhysics2D.World;
using System;
using System.Collections.Generic;
using System.Threading;

/*
# THIS FILE IS PART OF SimplePhysics2D
# 
# THIS PROGRAM IS FREE SOFTWARE, WE USED APACHE2.0 LICENSE.
# YOU SHOULD HAVE RECEIVED A COPY OF APACHE2.0 LICENSE.
#
# THIS STATEMENT APPLIES TO THE ENTIRE PROJECT 
#
# Copyright (c) 2024 Fltoto
*/
namespace SimplePhysics2D
{
    public class SPWorld2D : SComponentContainer<SPBody2D>
    {
        public static readonly float MinBodySize = 0.01f * 0.01f;
        public static readonly float MaxBodySize = 640 * 640;
        public static readonly float MinDensity = 0.5f;
        public static readonly float MaxDensity = 21.4f;

        public static readonly int MinIterations = 1;
        public static readonly int MaxIterations = 128;

        public float Duration = 0.01f;
        public int MaxTreeDepth = 16;
        public int FPS { get; private set; }
        private int fps;
        public CollideSolveType SolveType = CollideSolveType.Friction;
        public int Iterations = 2;
        public bool Running { get; private set; }

        public readonly SAABB WorldWide = new SAABB(new Vector2(-100000, -100000), new Vector2(100000, 100000));

        private DateTime LastTime;
        private DateTime lastfps;
        private Vector2[] contactList;
        private Vector2[] impulseList;
        private Vector2[] raList;
        private Vector2[] rbList;
        private Vector2[] frictionImpulseList;
        private float[] jList;
        private SpaceTree SpaceTree;

        public SPWorld2D()
        {
            this.contactList = new Vector2[2];
            this.impulseList = new Vector2[2];
            this.raList = new Vector2[2];
            this.rbList = new Vector2[2];
            this.frictionImpulseList = new Vector2[2];
            this.jList = new float[2];
        }
        public SPWorld2D(SAABB WorldWide)
        {
            this.contactList = new Vector2[2];
            this.impulseList = new Vector2[2];
            this.raList = new Vector2[2];
            this.rbList = new Vector2[2];
            this.frictionImpulseList = new Vector2[2];
            this.jList = new float[2];
            this.WorldWide = WorldWide;
        }
        private List<SPBody2D> treetoremove = new List<SPBody2D>();
        private List<SPBody2D> treetoadd = new List<SPBody2D>();
        public void Run()
        {
            if (Running)
            {
                return;
            }
            Running = true;
            Thread t = new Thread(() => { Loop(); });
            LastTime = DateTime.Now;
            SpaceTree = new SpaceTree(WorldWide, 0, MaxTreeDepth);
            OnAddCom += OnAddBody;
            OnRemoveCom += OnRemoveBody;
            t.Start();
        }
        private void OnAddBody(SPBody2D body)
        {
            lock (treetoadd)
            {
                treetoadd.Add(body);
            }
            body.SPWorld = this;
        }
        private void OnRemoveBody(SPBody2D body)
        {
            lock (treetoremove)
            {
                treetoremove.Add(body);
            }
        }
        public void UpdateBodyTree(SPBody2D body)
        {
            SpaceTree.Update(body);
        }
        public void Shutdown()
        {
            if (!Running)
            {
                return;
            }
            Running = false;
        }
        private void Step(float time, int Iterations)
        {
            Iterations = (int)SPMath2D.Clamp(Iterations, MinIterations, MaxIterations);
            var l = GetAllComponents();
            RayCaster.SetWorld(SpaceTree);
            lock (treetoadd)
            {
                for (int i = 0; i < treetoadd.Count; i++)
                {
                    SpaceTree.Add(treetoadd[i]);
                }
                treetoadd.Clear();
            }
            lock (treetoremove)
            {
                for (int i = 0; i < treetoremove.Count; i++)
                {
                    SpaceTree.Remove(treetoremove[i]);
                }
                treetoremove.Clear();
            }
            for (int it = 0; it < Iterations; it++)
            {
                for (int i = 0; i < l.Length; i++)
                {
                    l[i].Step(time, Iterations);
                }
                BroadPhase();
            }
        }
        private void Loop()
        {
            while (Running)
            {
                Update();
            }
        }
        private void Update()
        {
            var time = (float)(DateTime.Now - LastTime).TotalSeconds;
            fps++;
            if ((DateTime.Now - lastfps).TotalSeconds > 1)
            {
                lastfps = DateTime.Now;
                FPS = fps;
                fps = 0;
            }
            if (time >= Duration)
            {
                LastTime = DateTime.Now;
                Step(time, Iterations);
            }
        }
        private void BroadPhase()
        {
            List<SpaceTree> spaces = new List<SpaceTree>();
            SpaceTree.GetLastestTress(spaces);
            for (int x = 0; x < spaces.Count; x++)
            {
                if (!CheckSpace(spaces[x]))
                {
                    continue;
                }
                var l = spaces[x].GetItems();
                for (int i = 0; i < l.Length; i++)
                {
                    SPBody2D bodyA = l[i];
                    SAABB aabbA = bodyA.GetAABB();
                    for (int j = 0; j < l.Length; j++)
                    {
                        SPBody2D bodyB = l[j];
                        SAABB aabbB = bodyB.GetAABB();
                        if (bodyA == bodyB || (bodyA.IsStatic && bodyB.IsStatic))
                        {
                            continue;
                        }
                        if (!Collisions.IntersectAABBs(aabbA, aabbB))
                        {
                            continue;
                        }
                        NarrowPhase(bodyA, bodyB);
                    }
                }
            }
        }
        /// <summary>
        /// 用于检查空间是否启用，默认返回true,返回false则是不启用空间，则该空间内所有碰撞体都会停止碰撞，但是碰撞体的自我更新会持续进行。
        /// </summary>
        protected virtual bool CheckSpace(SpaceTree space)
        {
            return true;
        }
        private void NarrowPhase(SPBody2D bodyA, SPBody2D bodyB)
        {
            if (SolveType == CollideSolveType.DoNothing)
            {
                return;
            }
            if (Collisions.Collide(bodyA, bodyB, out Vector2 outer))
            {
                SeparateBodies(bodyA, bodyB, outer);
                Collisions.FindContacts(bodyA, bodyB, out Vector2 contact1, out Vector2 contact2, out int contactCount);
                SManifold contact = new SManifold(bodyA, bodyB, SPMath2D.Normalize(outer), SPMath2D.Length(outer), contact1, contact2, contactCount);
                switch (SolveType)
                {
                    case CollideSolveType.Basic: { ResolveCollisionBasic(contact); } break;
                    case CollideSolveType.Rotation: { ResolveCollisionWithRotation(contact); } break;
                    case CollideSolveType.Friction: { ResolveCollisionWithRotationAndFriction(contact); } break;
                }
            }
        }
        private void SeparateBodies(SPBody2D bodyA, SPBody2D bodyB, Vector2 mtv)
        {
            if (bodyA.IsTrigger || bodyB.IsTrigger)
            {
                return;
            }
            if (bodyA.IsStatic)
            {
                bodyB.Move(mtv);
            }
            else if (bodyB.IsStatic)
            {
                bodyA.Move(-mtv);
            }
            else
            {
                bodyA.Move(-mtv / 2f);
                bodyB.Move(mtv / 2f);
            }
        }
        private void ResolveCollisionBasic(in SManifold contact)
        {
            var bodyA = contact.BodyA;
            var bodyB = contact.BodyB;
            var normal = contact.Normal;

            bodyA.OnCollide?.Invoke(contact);
            bodyB.OnCollide?.Invoke(contact);

            if (bodyA.IsTrigger || bodyB.IsTrigger)
            {
                return;
            }

            Vector2 relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            if (SPMath2D.Dot(relativeVelocity, normal) > 0)
            {
                return;
            }

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);
            float j = -(1f + e) * SPMath2D.Dot(relativeVelocity, normal);
            j /= bodyA.InvMass + bodyB.InvMass;
            Vector2 impulse = j * normal;
            bodyA.LinearVelocity -= impulse * bodyA.InvMass;
            bodyB.LinearVelocity += impulse * bodyB.InvMass;
        }
        private void ResolveCollisionWithRotation(in SManifold contact)
        {
            SPBody2D bodyA = contact.BodyA;
            SPBody2D bodyB = contact.BodyB;
            bodyA.OnCollide?.Invoke(contact);
            bodyB.OnCollide?.Invoke(contact);
            if (bodyA.IsTrigger || bodyB.IsTrigger)
            {
                return;
            }
            Vector2 normal = contact.Normal;
            Vector2 contact1 = contact.Contact1;
            Vector2 contact2 = contact.Contact2;
            int contactCount = contact.ContactCount;

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            this.contactList[0] = contact1;
            this.contactList[1] = contact2;

            for (int i = 0; i < contactCount; i++)
            {
                this.impulseList[i] = Vector2.Zero;
                this.raList[i] = Vector2.Zero;
                this.rbList[i] = Vector2.Zero;
            }

            for (int i = 0; i < contactCount; i++)
            {
                Vector2 ra = contactList[i] - bodyA.Position;
                Vector2 rb = contactList[i] - bodyB.Position;

                raList[i] = ra;
                rbList[i] = rb;

                Vector2 raPerp = new Vector2(-ra.Y, ra.X);
                Vector2 rbPerp = new Vector2(-rb.Y, rb.X);

                Vector2 angularLinearVelocityA = raPerp * bodyA.AngularVelocity;
                Vector2 angularLinearVelocityB = rbPerp * bodyB.AngularVelocity;

                Vector2 relativeVelocity =
                    (bodyB.LinearVelocity + angularLinearVelocityB) -
                    (bodyA.LinearVelocity + angularLinearVelocityA);

                float contactVelocityMag = SPMath2D.Dot(relativeVelocity, normal);

                if (contactVelocityMag > 0f)
                {
                    continue;
                }

                float raPerpDotN = SPMath2D.Dot(raPerp, normal);
                float rbPerpDotN = SPMath2D.Dot(rbPerp, normal);

                float denom = bodyA.InvMass + bodyB.InvMass +
                    (raPerpDotN * raPerpDotN) * bodyA.InvInertia +
                    (rbPerpDotN * rbPerpDotN) * bodyB.InvInertia;

                float j = -(1f + e) * contactVelocityMag;
                j /= denom;
                j /= (float)contactCount;

                Vector2 impulse = j * normal;
                impulseList[i] = impulse;
            }

            for (int i = 0; i < contactCount; i++)
            {
                Vector2 impulse = impulseList[i];
                Vector2 ra = raList[i];
                Vector2 rb = rbList[i];

                bodyA.LinearVelocity += -impulse * bodyA.InvMass;
                bodyA.AngularVelocity += -SPMath2D.Cross(ra, impulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += impulse * bodyB.InvMass;
                bodyB.AngularVelocity += SPMath2D.Cross(rb, impulse) * bodyB.InvInertia;
            }
        }
        private void ResolveCollisionWithRotationAndFriction(in SManifold contact)
        {
            SPBody2D bodyA = contact.BodyA;
            SPBody2D bodyB = contact.BodyB;
            bodyA.OnCollide?.Invoke(contact);
            bodyB.OnCollide?.Invoke(contact);
            if (bodyA.IsTrigger || bodyB.IsTrigger)
            {
                return;
            }
            Vector2 normal = contact.Normal;
            Vector2 contact1 = contact.Contact1;
            Vector2 contact2 = contact.Contact2;
            int contactCount = contact.ContactCount;

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            float sf = (bodyA.StaticFriction + bodyB.StaticFriction) * 0.5f;
            float df = (bodyA.DynamicFriction + bodyB.DynamicFriction) * 0.5f;

            this.contactList[0] = contact1;
            this.contactList[1] = contact2;

            for (int i = 0; i < contactCount; i++)
            {
                this.impulseList[i] = Vector2.Zero;
                this.raList[i] = Vector2.Zero;
                this.rbList[i] = Vector2.Zero;
                this.frictionImpulseList[i] = Vector2.Zero;
                this.jList[i] = 0f;
            }

            for (int i = 0; i < contactCount; i++)
            {
                Vector2 ra = contactList[i] - bodyA.Position;
                Vector2 rb = contactList[i] - bodyB.Position;

                raList[i] = ra;
                rbList[i] = rb;

                Vector2 raPerp = new Vector2(-ra.Y, ra.X);
                Vector2 rbPerp = new Vector2(-rb.Y, rb.X);

                Vector2 angularLinearVelocityA = raPerp * bodyA.AngularVelocity;
                Vector2 angularLinearVelocityB = rbPerp * bodyB.AngularVelocity;

                Vector2 relativeVelocity =
                    (bodyB.LinearVelocity + angularLinearVelocityB) -
                    (bodyA.LinearVelocity + angularLinearVelocityA);

                float contactVelocityMag = SPMath2D.Dot(relativeVelocity, normal);

                if (contactVelocityMag > 0f)
                {
                    continue;
                }

                float raPerpDotN = SPMath2D.Dot(raPerp, normal);
                float rbPerpDotN = SPMath2D.Dot(rbPerp, normal);

                float denom = bodyA.InvMass + bodyB.InvMass +
                    (raPerpDotN * raPerpDotN) * bodyA.InvInertia +
                    (rbPerpDotN * rbPerpDotN) * bodyB.InvInertia;

                float j = -(1f + e) * contactVelocityMag;
                j /= denom;
                j /= (float)contactCount;

                jList[i] = j;

                Vector2 impulse = j * normal;
                impulseList[i] = impulse;
            }

            for (int i = 0; i < contactCount; i++)
            {
                Vector2 impulse = impulseList[i];
                Vector2 ra = raList[i];
                Vector2 rb = rbList[i];

                bodyA.LinearVelocity += -impulse * bodyA.InvMass;
                bodyA.AngularVelocity += -SPMath2D.Cross(ra, impulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += impulse * bodyB.InvMass;
                bodyB.AngularVelocity += SPMath2D.Cross(rb, impulse) * bodyB.InvInertia;
            }

            for (int i = 0; i < contactCount; i++)
            {
                Vector2 ra = contactList[i] - bodyA.Position;
                Vector2 rb = contactList[i] - bodyB.Position;

                raList[i] = ra;
                rbList[i] = rb;

                Vector2 raPerp = new Vector2(-ra.Y, ra.X);
                Vector2 rbPerp = new Vector2(-rb.Y, rb.X);

                Vector2 angularLinearVelocityA = raPerp * bodyA.AngularVelocity;
                Vector2 angularLinearVelocityB = rbPerp * bodyB.AngularVelocity;

                Vector2 relativeVelocity =
                    (bodyB.LinearVelocity + angularLinearVelocityB) -
                    (bodyA.LinearVelocity + angularLinearVelocityA);

                Vector2 tangent = relativeVelocity - SPMath2D.Dot(relativeVelocity, normal) * normal;

                if (SPMath2D.NearlyEqual(tangent, Vector2.Zero))
                {
                    continue;
                }
                else
                {
                    tangent = SPMath2D.Normalize(tangent);
                }

                float raPerpDotT = SPMath2D.Dot(raPerp, tangent);
                float rbPerpDotT = SPMath2D.Dot(rbPerp, tangent);

                float denom = bodyA.InvMass + bodyB.InvMass +
                    (raPerpDotT * raPerpDotT) * bodyA.InvInertia +
                    (rbPerpDotT * rbPerpDotT) * bodyB.InvInertia;

                float jt = -SPMath2D.Dot(relativeVelocity, tangent);
                jt /= denom;
                jt /= (float)contactCount;

                Vector2 frictionImpulse;
                float j = jList[i];

                if (MathF.Abs(jt) <= j * sf)
                {
                    frictionImpulse = jt * tangent;
                }
                else
                {
                    frictionImpulse = -j * tangent * df;
                }

                this.frictionImpulseList[i] = frictionImpulse;
            }

            for (int i = 0; i < contactCount; i++)
            {
                Vector2 frictionImpulse = this.frictionImpulseList[i];
                Vector2 ra = raList[i];
                Vector2 rb = rbList[i];

                bodyA.LinearVelocity += -frictionImpulse * bodyA.InvMass;
                bodyA.AngularVelocity += -SPMath2D.Cross(ra, frictionImpulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += frictionImpulse * bodyB.InvMass;
                bodyB.AngularVelocity += SPMath2D.Cross(rb, frictionImpulse) * bodyB.InvInertia;
            }
        }
    }
}
