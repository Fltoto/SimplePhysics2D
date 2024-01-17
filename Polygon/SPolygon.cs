using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace SimplePhysics2D
{
    public static class SPolygon
    {
        public static float Area(SPVector2[] vertices)
        {
            float area = 0f;

            for (int i = 0; i < vertices.Length; i++)
            {
                SPVector2 a = vertices[i];
                SPVector2 b = vertices[(i + 1) % vertices.Length];

                float width = b.X - a.X;
                float height = (b.Y + b.X) * 0.5f;

                area += width * height;
            }

            return area;
        }

        public static bool PointInTriangle(SPVector2 p, SPVector2 a, SPVector2 b, SPVector2 c)
        {
            SPVector2 ab = b - a;
            SPVector2 bc = c - b;
            SPVector2 ca = a - c;

            SPVector2 ap = p - a;
            SPVector2 bp = p - b;
            SPVector2 cp = p - c;

            float c1 = SPMath2D.Cross(ap, ab);
            float c2 = SPMath2D.Cross(bp, bc);
            float c3 = SPMath2D.Cross(cp, ca);

            if (c1 <= 0f || c2 <= 0f || c3 <= 0f)
            {
                return false;
            }

            return true;
        }

        private static bool AnyVerticesInTriangle(SPVector2[] vertices, SPVector2 a, SPVector2 b, SPVector2 c)
        {
            for (int j = 0; j < vertices.Length; j++)
            {
                SPVector2 p = vertices[j];

                if (SPolygon.PointInTriangle(p, a, b, c))
                {
                    return true;
                }
            }

            return false;
        }

        private static T GetItem<T>(List<T> list, int index)
        {
            int count = list.Count;

            if(index >= count)
            {
                return list[index % count];
            }
            else if(index < 0)
            {
                return list[(index % count) + count];
            }

            return list[index];
        }

        public static bool Triangulate(SPVector2[] vertices, [NotNullWhen(true)] out int[]? triangleIndices, out string errorMessage)
        {
            triangleIndices = null;
            errorMessage = string.Empty;

            if (vertices is null)
            {
                errorMessage = "Vertices array is null.";
                return false;
            }

            if (vertices.Length < 3)
            {
                errorMessage = "Vertices array must contain at least 3 items.";
                return false;
            }

            int triangleCount = vertices.Length - 2;
            int triangleIndicesCount = triangleCount * 3;

            triangleIndices = new int[triangleIndicesCount];
            int indexCount = 0;

            List<int> indices = new List<int>(vertices.Length);
            for (int i = 0; i < vertices.Length; i++)
            {
                indices.Add(i);
            }

            while (indices.Count > 3)
            {
                for (int i = 0; i < indices.Count; i++)
                {
                    int a = SPolygon.GetItem(indices, i - 1);
                    int b = SPolygon.GetItem(indices, i);
                    int c = SPolygon.GetItem(indices, i + 1);

                    SPVector2 va = vertices[a];
                    SPVector2 vb = vertices[b];
                    SPVector2 vc = vertices[c];

                    // Test for convexity. If not convex move to next angle.
                    if (SPMath2D.Cross(va - vb, vc - vb) <= 0f)
                    {
                        continue;
                    }

                    // Test for any points "inside" this triangle.
                    if (SPolygon.AnyVerticesInTriangle(vertices, va, vb, vc))
                    {
                        continue;
                    }

                    triangleIndices[indexCount++] = a;
                    triangleIndices[indexCount++] = b;
                    triangleIndices[indexCount++] = c;

                    indices.RemoveAt(i);

                    break;
                }
            }

            triangleIndices[indexCount++] = indices[0];
            triangleIndices[indexCount++] = indices[1];
            triangleIndices[indexCount++] = indices[2];

            return true;
        }
    }
}