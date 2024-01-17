using SimpleECS;
using SimplePhysics2D.BoudingBox;
using SimplePhysics2D.Collision;
using SimplePhysics2D.RigidBody;
using SimplePhysics2D.World;
using System;
using System.Threading;
using System.Threading.Tasks;
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

        public CollideSolveType SolveType = CollideSolveType.Friction;
        public int Iterations = 4;
        public bool Running { get; private set; }

        private DateTime LastTime;
        private SPVector2[] contactList;
        private SPVector2[] impulseList;
        private SPVector2[] raList;
        private SPVector2[] rbList;
        private SPVector2[] frictionImpulseList;
        private float[] jList;

        public SPWorld2D() {
            this.contactList = new SPVector2[2];
            this.impulseList = new SPVector2[2];
            this.raList = new SPVector2[2];
            this.rbList = new SPVector2[2];
            this.frictionImpulseList = new SPVector2[2];
            this.jList = new float[2];
        }

        public void Run()
        {
            if (Running)
            {
                return;
            }
            Running = true;
            Thread t = new Thread(() => { Loop(); });
            LastTime = DateTime.Now;
            t.Start();
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
            for (int it = 0; it < Iterations; it++) {
                BroadPhase(l,time,Iterations);
            }
        }
        private async void Loop()
        {
            while (Running)
            {
                await Task.Delay(1);
                var time = (float)(DateTime.Now - LastTime).TotalSeconds;
                LastTime = DateTime.Now;
                Step(time, Iterations);
            }
        }
        private void BroadPhase(SPBody2D[] l,float time,int Iterations) {
            for (int i=0;i<l.Length;i++) {
                SPBody2D bodyA = l[i];
                bodyA.Step(time,Iterations);
                SAABB aabbA = bodyA.GetAABB();
                for (int j=0;j<l.Length;j++) {
                    SPBody2D bodyB = l[j];
                    SAABB aabbB = bodyB.GetAABB();
                    if (bodyA==bodyB || (bodyA.IsStatic && bodyB.IsStatic)) {
                        continue;
                    }
                    if (!Collisions.IntersectAABBs(aabbA,aabbB)) {
                        continue;
                    }
                    NarrowPhase(bodyA,bodyB);
                }
            }
        }
        private void NarrowPhase(SPBody2D bodyA,SPBody2D  bodyB) {
            if (SolveType == CollideSolveType.DoNothing) {
                return;
            }
            if (Collisions.Collide(bodyA,bodyB,out SPVector2 normal,out float depth)) {
                SeparateBodies(bodyA,bodyB,normal*depth);
                Collisions.FindContacts(bodyA, bodyB, out SPVector2 contact1, out SPVector2 contact2, out int contactCount);
                SManifold contact = new SManifold(bodyA, bodyB, normal, depth, contact1, contact2, contactCount);
                switch (SolveType) {
                    case CollideSolveType.Basic: { ResolveCollisionBasic(contact); }break;
                    case CollideSolveType.Rotation: { ResolveCollisionWithRotation(contact); }break;
                    case CollideSolveType.Friction: { ResolveCollisionWithRotationAndFriction(contact); }break;
                }
            }
        }
        private void SeparateBodies(SPBody2D bodyA, SPBody2D bodyB, SPVector2 mtv)
        {
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
        public void ResolveCollisionBasic(in SManifold contact)
        {
            var bodyA = contact.BodyA;
            var bodyB = contact.BodyB;
            var normal = contact.Normal;
            var depth = contact.Depth;

            bodyA.OnCollide?.Invoke(contact);
            bodyB.OnCollide?.Invoke(contact);

            SPVector2 relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            if (SPMath2D.Dot(relativeVelocity,normal)>0) {
                return;
            }

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);
            float j = -(1f + e) * SPMath2D.Dot(relativeVelocity, normal);
            j /= bodyA.InvMass+ bodyB.InvMass;
            SPVector2 impulse = j * normal;
            bodyA.LinearVelocity -= impulse * bodyA.InvMass;
            bodyB.LinearVelocity += impulse * bodyB.InvMass;
        }
        public void ResolveCollisionWithRotation(in SManifold contact)
        {
            SPBody2D bodyA = contact.BodyA;
            SPBody2D bodyB = contact.BodyB;
            bodyA.OnCollide?.Invoke(contact);
            bodyB.OnCollide?.Invoke(contact);
            SPVector2 normal = contact.Normal;
            SPVector2 contact1 = contact.Contact1;
            SPVector2 contact2 = contact.Contact2;
            int contactCount = contact.ContactCount;

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            this.contactList[0] = contact1;
            this.contactList[1] = contact2;

            for (int i = 0; i < contactCount; i++)
            {
                this.impulseList[i] = SPVector2.Zero;
                this.raList[i] = SPVector2.Zero;
                this.rbList[i] = SPVector2.Zero;
            }

            for (int i = 0; i < contactCount; i++)
            {
                SPVector2 ra = contactList[i] - bodyA.Position;
                SPVector2 rb = contactList[i] - bodyB.Position;

                raList[i] = ra;
                rbList[i] = rb;

                SPVector2 raPerp = new SPVector2(-ra.Y, ra.X);
                SPVector2 rbPerp = new SPVector2(-rb.Y, rb.X);

                SPVector2 angularLinearVelocityA = raPerp * bodyA.AngularVelocity;
                SPVector2 angularLinearVelocityB = rbPerp * bodyB.AngularVelocity;

                SPVector2 relativeVelocity =
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

                SPVector2 impulse = j * normal;
                impulseList[i] = impulse;
            }

            for (int i = 0; i < contactCount; i++)
            {
                SPVector2 impulse = impulseList[i];
                SPVector2 ra = raList[i];
                SPVector2 rb = rbList[i];

                bodyA.LinearVelocity += -impulse * bodyA.InvMass;
                bodyA.AngularVelocity += -SPMath2D.Cross(ra, impulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += impulse * bodyB.InvMass;
                bodyB.AngularVelocity += SPMath2D.Cross(rb, impulse) * bodyB.InvInertia;
            }
        }
        public void ResolveCollisionWithRotationAndFriction(in SManifold contact)
        {
            SPBody2D bodyA = contact.BodyA;
            SPBody2D bodyB = contact.BodyB;
            bodyA.OnCollide?.Invoke(contact);
            bodyB.OnCollide?.Invoke(contact);
            SPVector2 normal = contact.Normal;
            SPVector2 contact1 = contact.Contact1;
            SPVector2 contact2 = contact.Contact2;
            int contactCount = contact.ContactCount;

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            float sf = (bodyA.StaticFriction + bodyB.StaticFriction) * 0.5f;
            float df = (bodyA.DynamicFriction + bodyB.DynamicFriction) * 0.5f;

            this.contactList[0] = contact1;
            this.contactList[1] = contact2;

            for (int i = 0; i < contactCount; i++)
            {
                this.impulseList[i] = SPVector2.Zero;
                this.raList[i] = SPVector2.Zero;
                this.rbList[i] = SPVector2.Zero;
                this.frictionImpulseList[i] = SPVector2.Zero;
                this.jList[i] = 0f;
            }

            for (int i = 0; i < contactCount; i++)
            {
                SPVector2 ra = contactList[i] - bodyA.Position;
                SPVector2 rb = contactList[i] - bodyB.Position;

                raList[i] = ra;
                rbList[i] = rb;

                SPVector2 raPerp = new SPVector2(-ra.Y, ra.X);
                SPVector2 rbPerp = new SPVector2(-rb.Y, rb.X);

                SPVector2 angularLinearVelocityA = raPerp * bodyA.AngularVelocity;
                SPVector2 angularLinearVelocityB = rbPerp * bodyB.AngularVelocity;

                SPVector2 relativeVelocity =
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

                SPVector2 impulse = j * normal;
                impulseList[i] = impulse;
            }

            for (int i = 0; i < contactCount; i++)
            {
                SPVector2 impulse = impulseList[i];
                SPVector2 ra = raList[i];
                SPVector2 rb = rbList[i];

                bodyA.LinearVelocity += -impulse * bodyA.InvMass;
                bodyA.AngularVelocity += -SPMath2D.Cross(ra, impulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += impulse * bodyB.InvMass;
                bodyB.AngularVelocity += SPMath2D.Cross(rb, impulse) * bodyB.InvInertia;
            }

            for (int i = 0; i < contactCount; i++)
            {
                SPVector2 ra = contactList[i] - bodyA.Position;
                SPVector2 rb = contactList[i] - bodyB.Position;

                raList[i] = ra;
                rbList[i] = rb;

                SPVector2 raPerp = new SPVector2(-ra.Y, ra.X);
                SPVector2 rbPerp = new SPVector2(-rb.Y, rb.X);

                SPVector2 angularLinearVelocityA = raPerp * bodyA.AngularVelocity;
                SPVector2 angularLinearVelocityB = rbPerp * bodyB.AngularVelocity;

                SPVector2 relativeVelocity =
                    (bodyB.LinearVelocity + angularLinearVelocityB) -
                    (bodyA.LinearVelocity + angularLinearVelocityA);

                SPVector2 tangent = relativeVelocity - SPMath2D.Dot(relativeVelocity, normal) * normal;

                if (SPMath2D.NearlyEqual(tangent, SPVector2.Zero))
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

                SPVector2 frictionImpulse;
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
                SPVector2 frictionImpulse = this.frictionImpulseList[i];
                SPVector2 ra = raList[i];
                SPVector2 rb = rbList[i];

                bodyA.LinearVelocity += -frictionImpulse * bodyA.InvMass;
                bodyA.AngularVelocity += -SPMath2D.Cross(ra, frictionImpulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += frictionImpulse * bodyB.InvMass;
                bodyB.AngularVelocity += SPMath2D.Cross(rb, frictionImpulse) * bodyB.InvInertia;
            }
        }
    }
}
