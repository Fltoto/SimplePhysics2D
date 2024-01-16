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
        public static bool IntersectCirclePolygons(SPVector2 center, float circleRadius, SPVector2[] vertices, out SPVector2 normal, out float depth)
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
            SPVector2 polygonCenter = FindArithmeticMean(vertices);
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
        public static bool IntersectPolygons(SPVector2[] verticesA, SPVector2[] verticesB, out SPVector2 normal, out float depth)
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
            SPVector2 centerA = FindArithmeticMean(verticesA);
            SPVector2 centerB = FindArithmeticMean(verticesB);
            SPVector2 direction = centerB - centerA;
            if (SPMath2D.Dot(direction, normal) < 0)
            {
                normal = -normal;
            }
            return true;
        }
        private static SPVector2 FindArithmeticMean(SPVector2[] vertices)
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
