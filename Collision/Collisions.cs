using SimplePhysics2D.BoudingBox;
using SimplePhysics2D.RigidBody;
using SimplePhysics2D.Shapes;
using System;
using System.Collections.Generic;
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
        public static void PointSegmentDistance(Vector2 p, Vector2 a, Vector2 b, out float distanceSquared, out Vector2 cp)
        {
            Vector2 ab = b - a;
            Vector2 ap = p - a;

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
        public static void FindContacts(SPBody2D bodyA, SPBody2D bodyB, out Vector2 contact1, out Vector2 contact2, out int contactCount)
        {
            contact1 = Vector2.Zero;
            contact2 = Vector2.Zero;
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
            List<Vector2[]> vertsA, List<Vector2[]> vertsB,
            out Vector2 contact1, out Vector2 contact2, out int contactCount)
        {
            contact1 = Vector2.Zero;
            contact2 = Vector2.Zero;
            contactCount = 0;
            float minDistSq = float.MaxValue;

            for (int g = 0; g < vertsA.Count; g++)
            {
                var verticesA = vertsA[g];
                for (int h = 0; h < vertsB.Count; h++)
                {
                    var verticesB = vertsB[h];

                    for (int i = 0; i < verticesA.Length; i++)
                    {
                        Vector2 p = verticesA[i];

                        for (int j = 0; j < verticesB.Length; j++)
                        {
                            Vector2 va = verticesB[j];
                            Vector2 vb = verticesB[(j + 1) % verticesB.Length];

                            Collisions.PointSegmentDistance(p, va, vb, out float distSq, out Vector2 cp);

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
                        Vector2 p = verticesB[i];

                        for (int j = 0; j < verticesA.Length; j++)
                        {
                            Vector2 va = verticesA[j];
                            Vector2 vb = verticesA[(j + 1) % verticesA.Length];

                            Collisions.PointSegmentDistance(p, va, vb, out float distSq, out Vector2 cp);

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
            }

        }
        private static void FindCirclePolygonContactPoint(
            Vector2 circleCenter, float circleRadius,
            Vector2 polygonCenter, List<Vector2[]> pverts,
            out Vector2 cp)
        {
            cp = Vector2.Zero;
            float minDistSq = float.MaxValue;
            for (int g = 0; g < pverts.Count; g++)
            {
                var polygonVertices = pverts[g];

                for (int i = 0; i < polygonVertices.Length; i++)
                {
                    Vector2 va = polygonVertices[i];
                    Vector2 vb = polygonVertices[(i + 1) % polygonVertices.Length];

                    Collisions.PointSegmentDistance(circleCenter, va, vb, out float distSq, out Vector2 contact);

                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        cp = contact;
                    }
                }
            }
        }
        private static void FindCirclesContactPoint(Vector2 centerA, float radiusA, Vector2 centerB, out Vector2 cp)
        {
            Vector2 ab = centerB - centerA;
            Vector2 dir = SPMath2D.Normalize(ab);
            cp = centerA + dir * radiusA;
        }
        public static bool Collide(SPBody2D bodyA, SPBody2D bodyB, out Vector2 outer)
        {
            outer = Vector2.Zero;
            ShapeType2D ShapeTypeA = bodyA.ShapeType;
            ShapeType2D ShapeTypeB = bodyB.ShapeType;
            if (ShapeTypeA is ShapeType2D.Box)
            {
                if (ShapeTypeB is ShapeType2D.Circle)
                {
                    return Collisions.IntersectCirclePolygons(bodyB.Position, bodyB.Radius, bodyA.GetTransformedVertices(), out outer);
                }
                else if (ShapeTypeB is ShapeType2D.Box)
                {
                    return Collisions.IntersectPolygons(bodyA.GetTransformedVertices(), bodyB.GetTransformedVertices(), out outer);

                }
            }
            else if (ShapeTypeA is ShapeType2D.Circle)
            {
                if (ShapeTypeB is ShapeType2D.Circle)
                {
                    return Collisions.IntersectCircles(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.Radius, out outer);
                }
                else if (ShapeTypeB is ShapeType2D.Box)
                {
                    return Collisions.IntersectCirclePolygons(bodyA.Position, bodyA.Radius, bodyB.GetTransformedVertices(), out outer);
                }
            }
            return false;
        }
        public static bool IntersectCirclePolygons(Vector2 center, float circleRadius, List<Vector2[]> verts, out Vector2 outer)
        {
            outer = Vector2.Zero;

            for (int g = 0; g < verts.Count; g++)
            {
                var vertices = verts[g];
                var polygonCenter = FindArithmeticMean(vertices);
                var tnormal = Vector2.Zero;
                var tdepth = float.MaxValue;
                Vector2 axis;
                float axisDepth;
                float minA, maxA, minB, maxB;
                bool check = false;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector2 va = vertices[i];
                    Vector2 vb = vertices[(i + 1) % vertices.Length];
                    Vector2 edge = vb - va;
                    axis = new Vector2(-edge.Y, edge.X);
                    axis = SPMath2D.Normalize(axis);
                    ProjectVertices(vertices, axis, out minA, out maxA);
                    ProjectCircle(center, circleRadius, axis, out minB, out maxB);
                    if (minA >= maxB || minB >= maxA)
                    {
                        check = true;
                        break;
                    }
                    axisDepth = MathF.Min(maxB - minA, maxA - minB);
                    if (axisDepth < tdepth)
                    {
                        tdepth = axisDepth;
                        tnormal = axis;
                    }
                }
                if (check)
                {
                    continue;
                }
                int cpIndex = FindClosetPointOnPolygon(center, vertices);
                Vector2 cp = vertices[cpIndex];
                axis = cp - center;
                axis = SPMath2D.Normalize(axis);
                ProjectVertices(vertices, axis, out minA, out maxA);
                ProjectCircle(center, circleRadius, axis, out minB, out maxB);
                if (minA >= maxB || minB >= maxA)
                {
                    continue;
                }
                axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < tdepth)
                {
                    tdepth = axisDepth;
                    tnormal = axis;
                }

                tdepth /= SPMath2D.Length(tnormal);
                tnormal = SPMath2D.Normalize(tnormal);

                Vector2 direction = polygonCenter - center;
                if (SPMath2D.Dot(direction, tnormal) < 0)
                {
                    tnormal = -tnormal;
                }

                outer += tnormal * tdepth;
            }
            if (outer.Equals(Vector2.Zero))
            {
                return false;
            }
            return true;
        }
        private static int FindClosetPointOnPolygon(Vector2 circlecenter, Vector2[] vertices)
        {
            int result = 0;
            float minDistance = float.MaxValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 v = vertices[i];
                float distance = SPMath2D.Distance(v, circlecenter);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    result = i;
                }
            }
            return result;
        }
        private static void ProjectCircle(Vector2 center, float radius, Vector2 axis, out float min, out float max)
        {
            Vector2 direction = SPMath2D.Normalize(axis);
            Vector2 directionAndRadius = direction * radius;
            Vector2 p1 = center + directionAndRadius;
            Vector2 p2 = center - directionAndRadius;
            min = SPMath2D.Dot(p1, axis);
            max = SPMath2D.Dot(p2, axis);
            if (min > max)
            {
                float t = min;
                min = max;
                max = t;
            }
        }
        public static bool IntersectPolygons(List<Vector2[]> vertsA, List<Vector2[]> vertsB, out Vector2 outer)
        {
            outer = Vector2.Zero;

            for (int g = 0; g < vertsA.Count; g++)
            {
                var verticesA = vertsA[g];
                var centerA = FindArithmeticMean(verticesA);
                for (int h = 0; h < vertsB.Count; h++)
                {
                    var verticesB = vertsB[h];
                    var centerB = FindArithmeticMean(verticesB);
                    var tnormal = Vector2.Zero;
                    var tdepth = float.MaxValue;
                    bool check = false;
                    for (int i = 0; i < verticesA.Length; i++)
                    {
                        Vector2 va = verticesA[i];
                        Vector2 vb = verticesA[(i + 1) % verticesA.Length];
                        Vector2 edge = vb - va;
                        Vector2 axis = new Vector2(-edge.Y, edge.X);
                        axis = SPMath2D.Normalize(axis);
                        ProjectVertices(verticesA, axis, out float minA, out float maxA);
                        ProjectVertices(verticesB, axis, out float minB, out float maxB);
                        if (minA >= maxB || minB >= maxA)
                        {
                            check = true;
                            break;
                        }
                        float axisDepth = MathF.Min(maxB - minA, maxA - minB);
                        if (axisDepth < tdepth)
                        {
                            tdepth = axisDepth;
                            tnormal = axis;
                        }
                    }
                    if (check)
                    {
                        continue;
                    }
                    for (int i = 0; i < verticesB.Length; i++)
                    {
                        Vector2 va = verticesB[i];
                        Vector2 vb = verticesB[(i + 1) % verticesB.Length];
                        Vector2 edge = vb - va;
                        Vector2 axis = new Vector2(-edge.Y, edge.X);
                        axis = SPMath2D.Normalize(axis);
                        ProjectVertices(verticesA, axis, out float minA, out float maxA);
                        ProjectVertices(verticesB, axis, out float minB, out float maxB);
                        if (minA >= maxB || minB >= maxA)
                        {
                            check = true;
                            break;
                        }
                        float axisDepth = MathF.Min(maxB - minA, maxA - minB);
                        if (axisDepth < tdepth)
                        {
                            tdepth = axisDepth;
                            tnormal = axis;
                        }
                    }
                    if (check)
                    {
                        continue;
                    }
                    tdepth /= SPMath2D.Length(tnormal);
                    tnormal = SPMath2D.Normalize(tnormal);
                    Vector2 direction = centerB - centerA;
                    if (SPMath2D.Dot(direction, tnormal) < 0)
                    {
                        tnormal = -tnormal;
                    }

                    outer += tnormal * tdepth;
                }
            }
            if (outer.Equals(Vector2.Zero))
            {
                return false;
            }
            return true;
        }
        public static Vector2 FindArithmeticMean(Vector2[] vertices)
        {
            float sumX = 0;
            float sumY = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 v = vertices[i];
                sumX += v.X;
                sumY += v.Y;
            }
            return new Vector2(sumX / (float)vertices.Length, sumY / (float)vertices.Length);
        }
        private static void ProjectVertices(Vector2[] vertices, Vector2 axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 v = vertices[i];
                float proj = SPMath2D.Dot(v, axis);

                if (proj < min) { min = proj; }
                if (proj > max) { max = proj; }
            }
        }
        public static bool IntersectCircles(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB, out Vector2 outer)
        {
            outer = Vector2.Zero;
            float distance = SPMath2D.Distance(centerA, centerB);
            float radii = radiusA + radiusB;
            if (distance >= radii)
            {
                return false;
            }
            var depth = radii - distance;
            outer = SPMath2D.Normalize(centerB - centerA) * depth;
            return true;
        }
    }
}
