using SimpleECS;
using SimplePhysics2D.Collision;
using SimplePhysics2D.RigidBody;
using SimplePhysics2D.Shapes;
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

        public int Iterations = 4;
        public bool Running { get; private set; }

        private DateTime LastTime;

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
        private void Step(float time,int Iterations)
        {
            Iterations = (int)SPMath2D.Clamp(Iterations,MinIterations,MaxIterations);


            var l = GetAllComponents();
            if (l.Length > 2)
            {
                l[0].AddForce(new SPVector2(1, 0));
                l[1].isStatic = true;
                l[2].isStatic = true;
            }

            for (int it=0;it<Iterations;it++) {
                for (int i = 0; i < l.Length; i++)
                {
                    SPBody2D bodyA = l[i];
                    bodyA.Step(time, Iterations);
                    for (int j = 0; j < l.Length; j++)
                    {
                        SPBody2D bodyB = l[j];
                        if (bodyA == bodyB)
                        {
                            continue;
                        }
                        if (bodyA.isStatic && bodyB.isStatic)
                        {
                            continue;
                        }
                        if (Collide(bodyA, bodyB, out SPVector2 normal, out float depth))
                        {
                            ResolveCollision(bodyA, bodyB, normal, depth);
                        }
                    }
                }
            }
        }
        public void ResolveCollision(SPBody2D bodyA, SPBody2D bodyB, SPVector2 normal, float depth)
        {
            if (bodyB.isStatic)
            {
                bodyA.Move(-normal * depth / 2f);
            }
            else
            if (bodyA.isStatic)
            {
                bodyB.Move(normal * depth / 2f);
            }
            else {
                bodyA.Move(-normal * depth / 2f);
                bodyB.Move(normal * depth / 2f);
            }
            bodyA.OnCollide?.Invoke(new CollideInfo(bodyB, normal, depth));
            bodyB.OnCollide?.Invoke(new CollideInfo(bodyA, normal, depth));

            SPVector2 relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            if (SPMath2D.Dot(relativeVelocity,normal)>0) {
                return;
            }

            float e = MathF.Min(bodyA.Restiution, bodyB.Restiution);
            float j = -(1f + e) * SPMath2D.Dot(relativeVelocity, normal);
            j /= bodyA.InvMass+ bodyB.InvMass;
            SPVector2 impulse = j * normal;
            if (!bodyA.isStatic){
                bodyA.LinearVelocity -= impulse * bodyA.InvMass;
            }
            if (!bodyB.isStatic) {
                bodyB.LinearVelocity += impulse * bodyB.InvMass;
            }
        }
        public bool Collide(SPBody2D bodyA, SPBody2D bodyB, out SPVector2 normal, out float depth)
        {
            normal = SPVector2.Zero;
            depth = 0;
            ShapeType2D ShapeTypeA = bodyA.ShapeType;
            ShapeType2D ShapeTypeB = bodyB.ShapeType;
            if (ShapeTypeA is ShapeType2D.Box)
            {
                if (ShapeTypeB is ShapeType2D.Circle)
                {
                    return Collisions.IntersectCirclePolygons(bodyB.Position, bodyB.Radius, bodyA.Position,bodyA.GetTransformVertices(), out normal, out depth);
                }
                else if (ShapeTypeB is ShapeType2D.Box)
                {
                    return Collisions.IntersectPolygons(bodyA.GetTransformVertices(),bodyA.Position, bodyB.GetTransformVertices(),bodyB.Position, out normal, out depth);

                }
            }
            else if (ShapeTypeA is ShapeType2D.Circle)
            {
                if (ShapeTypeB is ShapeType2D.Circle)
                {
                    return Collisions.IntersectCircles(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.Radius, out normal, out depth);
                }
                else if (ShapeTypeB is ShapeType2D.Box)
                {
                    return Collisions.IntersectCirclePolygons(bodyA.Position, bodyA.Radius, bodyB.Position,bodyB.GetTransformVertices(), out normal, out depth);
                }
            }
            return false;
        }
        private async void Loop()
        {
            while (Running)
            {
                await Task.Delay(1);
                var time = (float)(DateTime.Now - LastTime).TotalSeconds;
                LastTime = DateTime.Now;
                Step(time,Iterations);
            }
        }
    }
}
