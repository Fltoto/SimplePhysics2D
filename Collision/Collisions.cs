using SimplePhysics2D.BoudingBox;
using SimplePhysics2D.RigidBody;
using SimplePhysics2D.Shapes;
using System;
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
namespace SimplePhysics2D.Collision
{
    public static class Collisions
    {
        public static bool IntersectAABBs(SAABB a, SAABB b)
        {
            if (a.Max.X <= b.Min.X || b.Max.X <= a.Min.X ||
                a.Max.Y <= b.Min.Y || b.Max.Y <= a.Min.Y)
            {
                return false;
            }
            return true;
        }

        public static void PointSegmentDistance(SPVector2 p, SPVector2 a, SPVector2 b, out float distanceSquared, out SPVector2 cp)
        {
            SPVector2 ab = b - a;
            SPVector2 ap = p - a;

            float proj = SPMath2D.Dot(ap, ab);
            float abLenSq = SPMath2D.LengthSquared(ab);
            float d = proj / abLenSq;

            if (d <= 0f)
            {
                cp = a;
            }
            else if (d >= 1f)
            {
                cp = b;
            }
            else
            {
                cp = a + ab * d;
            }

            distanceSquared = SPMath2D.DistanceSquared(p, cp);
        }
        public static void FindContacts(SPBody2D bodyA,SPBody2D bodyB,out SPVector2 contact1,out SPVector2 contact2,out int contactCount) {
            contact1 = SPVector2.Zero;
            contact2 = SPVector2.Zero;
            contactCount = 0;

            ShapeType2D shapeTypeA = bodyA.ShapeType;
            ShapeType2D shapeTypeB = bodyB.ShapeType;

            if (shapeTypeA is ShapeType2D.Box)
            {
                if (shapeTypeB is ShapeType2D.Box)
                {
                    FindPolygonsContactPoints(bodyA.GetTransformedVertices(), bodyB.GetTransformedVertices(),
                        out contact1, out contact2, out contactCount);
                }
                else if (shapeTypeB is ShapeType2D.Circle)
                {
                    FindCirclePolygonContactPoint(bodyB.Position, bodyB.Radius, bodyA.Position, bodyA.GetTransformedVertices(), out contact1);
                    contactCount = 1;
                }
            }
            else if (shapeTypeA is ShapeType2D.Circle)
            {
                if (shapeTypeB is ShapeType2D.Box)
                {
                    FindCirclePolygonContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.GetTransformedVertices(), out contact1);
                    contactCount = 1;
                }
                else if (shapeTypeB is ShapeType2D.Circle)
                {
                    FindCirclesContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position, out contact1);
                    contactCount = 1;
                }
            }
        }
        private static void FindPolygonsContactPoints(
            SPVector2[] verticesA, SPVector2[] verticesB,
            out SPVector2 contact1, out SPVector2 contact2, out int contactCount)
        {
            contact1 = SPVector2.Zero;
            contact2 = SPVector2.Zero;
            contactCount = 0;

            float minDistSq = float.MaxValue;

            for (int i = 0; i < verticesA.Length; i++)
            {
                SPVector2 p = verticesA[i];

                for (int j = 0; j < verticesB.Length; j++)
                {
                    SPVector2 va = verticesB[j];
                    SPVector2 vb = verticesB[(j + 1) % verticesB.Length];

                    Collisions.PointSegmentDistance(p, va, vb, out float distSq, out SPVector2 cp);

                    if (SPMath2D.NearlyEqual(distSq, minDistSq))
                    {
                        if (!SPMath2D.NearlyEqual(cp, contact1))
                        {
                            contact2 = cp;
                            contactCount = 2;
                        }
                    }
                    else if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        contactCount = 1;
                        contact1 = cp;
                    }
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                SPVector2 p = verticesB[i];

                for (int j = 0; j < verticesA.Length; j++)
                {
                    SPVector2 va = verticesA[j];
                    SPVector2 vb = verticesA[(j + 1) % verticesA.Length];

                    Collisions.PointSegmentDistance(p, va, vb, out float distSq, out SPVector2 cp);

                    if (SPMath2D.NearlyEqual(distSq, minDistSq))
                    {
                        if (!SPMath2D.NearlyEqual(cp, contact1))
                        {
                            contact2 = cp;
                            contactCount = 2;
                        }
                    }
                    else if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        contactCount = 1;
                        contact1 = cp;
                    }
                }
            }
        }
        private static void FindCirclePolygonContactPoint(
            SPVector2 circleCenter, float circleRadius,
            SPVector2 polygonCenter, SPVector2[] polygonVertices,
            out SPVector2 cp)
        {
            cp = SPVector2.Zero;

            float minDistSq = float.MaxValue;

            for (int i = 0; i < polygonVertices.Length; i++)
            {
                SPVector2 va = polygonVertices[i];
                SPVector2 vb = polygonVertices[(i + 1) % polygonVertices.Length];

                Collisions.PointSegmentDistance(circleCenter, va, vb, out float distSq, out SPVector2 contact);

                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    cp = contact;
                }
            }
        }
        private static void FindCirclesContactPoint(SPVector2 centerA,float radiusA,SPVector2 centerB,out SPVector2 cp) {
            SPVector2 ab = centerB - centerA;
            SPVector2 dir = SPMath2D.Normalize(ab);
            cp = centerA + dir * radiusA;
        }
        public static bool Collide(SPBody2D bodyA, SPBody2D bodyB, out SPVector2 normal, out float depth)
        {
            normal = SPVector2.Zero;
            depth = 0;
            ShapeType2D ShapeTypeA = bodyA.ShapeType;
            ShapeType2D ShapeTypeB = bodyB.ShapeType;
            if (ShapeTypeA is ShapeType2D.Box)
            {
                if (ShapeTypeB is ShapeType2D.Circle)
                {
                    return Collisions.IntersectCirclePolygons(bodyB.Position, bodyB.Radius, bodyA.Position, bodyA.GetTransformedVertices(), out normal, out depth);
                }
                else if (ShapeTypeB is ShapeType2D.Box)
                {
                    return Collisions.IntersectPolygons(bodyA.GetTransformedVertices(), bodyA.Position, bodyB.GetTransformedVertices(), bodyB.Position, out normal, out depth);

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
                    return Collisions.IntersectCirclePolygons(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.GetTransformedVertices(), out normal, out depth);
                }
            }
            return false;
        }
        public static bool IntersectCirclePolygons(SPVector2 center, float circleRadius,SPVector2 polygonCenter, SPVector2[] vertices, out SPVector2 normal, out float depth)
        {
            normal = SPVector2.Zero;
            depth = float.MaxValue;
            SPVector2 axis;
            float axisDepth;
            float minA, maxA, minB, maxB;
            for (int i = 0; i < vertices.Length; i++)
            {
                SPVector2 va = vertices[i];
                SPVector2 vb = vertices[(i + 1) % vertices.Length];
                SPVector2 edge = vb - va;
                axis = new SPVector2(-edge.Y, edge.X);
                axis = SPMath2D.Normalize(axis);
                ProjectVertices(vertices, axis, out minA, out maxA);
                ProjectCircle(center, circleRadius, axis, out minB, out maxB);
                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }
                axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }
            int cpIndex = FindClosetPointOnPolygon(center, vertices);
            SPVector2 cp = vertices[cpIndex];
            axis = cp - center;
            axis = SPMath2D.Normalize(axis);
            ProjectVertices(vertices, axis, out minA, out maxA);
            ProjectCircle(center, circleRadius, axis, out minB, out maxB);
            if (minA >= maxB || minB >= maxA)
            {
                return false;
            }
            axisDepth = MathF.Min(maxB - minA, maxA - minB);
            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = axis;
            }

            depth /= SPMath2D.Length(normal);
            normal = SPMath2D.Normalize(normal);

            SPVector2 direction = polygonCenter - center;
            if (SPMath2D.Dot(direction, normal) < 0)
            {
                normal = -normal;
            }

            return true;
        }
        private static int FindClosetPointOnPolygon(SPVector2 circlecenter, SPVector2[] vertices)
        {
            int result = 0;
            float minDistance = float.MaxValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                SPVector2 v = vertices[i];
                float distance = SPMath2D.Distance(v, circlecenter);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    result = i;
                }
            }
            return result;
        }
        private static void ProjectCircle(SPVector2 center, float radius, SPVector2 axis, out float min, out float max)
        {
            SPVector2 direction = SPMath2D.Normalize(axis);
            SPVector2 directionAndRadius = direction * radius;
            SPVector2 p1 = center + directionAndRadius;
            SPVector2 p2 = center - directionAndRadius;
            min = SPMath2D.Dot(p1, axis);
            max = SPMath2D.Dot(p2, axis);
            if (min > max)
            {
                float t = min;
                min = max;
                max = t;
            }
        }
        public static bool IntersectPolygons(SPVector2[] verticesA,SPVector2 centerA, SPVector2[] verticesB,SPVector2 centerB, out SPVector2 normal, out float depth)
        {
            normal = SPVector2.Zero;
            depth = float.MaxValue;
            for (int i = 0; i < verticesA.Length; i++)
            {
                SPVector2 va = verticesA[i];
                SPVector2 vb = verticesA[(i + 1) % verticesA.Length];
                SPVector2 edge = vb - va;
                SPVector2 axis = new SPVector2(-edge.Y, edge.X);
                axis = SPMath2D.Normalize(axis);
                ProjectVertices(verticesA, axis, out float minA, out float maxA);
                ProjectVertices(verticesB, axis, out float minB, out float maxB);
                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }
                float axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }
            for (int i = 0; i < verticesB.Length; i++)
            {
                SPVector2 va = verticesB[i];
                SPVector2 vb = verticesB[(i + 1) % verticesB.Length];
                SPVector2 edge = vb - va;
                SPVector2 axis = new SPVector2(-edge.Y, edge.X);
                axis = SPMath2D.Normalize(axis);
                ProjectVertices(verticesA, axis, out float minA, out float maxA);
                ProjectVertices(verticesB, axis, out float minB, out float maxB);
                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }
                float axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }
            depth /= SPMath2D.Length(normal);
            normal = SPMath2D.Normalize(normal);
            SPVector2 direction = centerB - centerA;
            if (SPMath2D.Dot(direction, normal) < 0)
            {
                normal = -normal;
            }
            return true;
        }
        public static SPVector2 FindArithmeticMean(SPVector2[] vertices)
        {
            float sumX = 0;
            float sumY = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                SPVector2 v = vertices[i];
                sumX += v.X;
                sumY += v.Y;
            }
            return new SPVector2(sumX / (float)vertices.Length, sumY / (float)vertices.Length);
        }
        private static void ProjectVertices(SPVector2[] vertices, SPVector2 axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                SPVector2 v = vertices[i];
                float proj = SPMath2D.Dot(v, axis);

                if (proj < min) { min = proj; }
                if (proj > max) { max = proj; }
            }
        }
        public static bool IntersectCircles(SPVector2 centerA, float radiusA, SPVector2 centerB, float radiusB, out SPVector2 normal, out float depth)
        {
            normal = SPVector2.Zero;
            depth = 0;
            float distance = SPMath2D.Distance(centerA, centerB);
            float radii = radiusA + radiusB;
            if (distance >= radii)
            {
                return false;
            }
            normal = SPMath2D.Normalize(centerB - centerA);
            depth = radii - distance;
            return true;
        }
    }
}
