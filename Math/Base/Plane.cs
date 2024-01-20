using System;
using System.Runtime.Serialization;

namespace SimplePhysics2D
{
    public struct Plane : IEquatable<Plane>
    {
        [DataMember]
        public float D;

        [DataMember]
        public Vector3 Normal;

        internal string DebugDisplayString => Normal.DebugDisplayString + "  " + D;

        public Plane(Vector4 value)
            : this(new Vector3(value.X, value.Y, value.Z), value.W)
        {
        }

        public Plane(Vector3 normal, float d)
        {
            Normal = normal;
            D = d;
        }

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 vector = b - a;
            Vector3 vector2 = c - a;
            Vector3 value = Vector3.Cross(vector, vector2);
            Vector3.Normalize(ref value, out Normal);
            D = 0f - Vector3.Dot(Normal, a);
        }

        public Plane(float a, float b, float c, float d)
            : this(new Vector3(a, b, c), d)
        {
        }

        public Plane(Vector3 pointOnPlane, Vector3 normal)
        {
            Normal = normal;
            D = 0f - (pointOnPlane.X * normal.X + pointOnPlane.Y * normal.Y + pointOnPlane.Z * normal.Z);
        }

        public float Dot(Vector4 value)
        {
            return Normal.X * value.X + Normal.Y * value.Y + Normal.Z * value.Z + D * value.W;
        }

        public void Dot(ref Vector4 value, out float result)
        {
            result = Normal.X * value.X + Normal.Y * value.Y + Normal.Z * value.Z + D * value.W;
        }

        public float DotCoordinate(Vector3 value)
        {
            return Normal.X * value.X + Normal.Y * value.Y + Normal.Z * value.Z + D;
        }

        public void DotCoordinate(ref Vector3 value, out float result)
        {
            result = Normal.X * value.X + Normal.Y * value.Y + Normal.Z * value.Z + D;
        }

        public float DotNormal(Vector3 value)
        {
            return Normal.X * value.X + Normal.Y * value.Y + Normal.Z * value.Z;
        }

        public void DotNormal(ref Vector3 value, out float result)
        {
            result = Normal.X * value.X + Normal.Y * value.Y + Normal.Z * value.Z;
        }

        public static Plane Transform(Plane plane, Matrix matrix)
        {
            Transform(ref plane, ref matrix, out var result);
            return result;
        }

        public static void Transform(ref Plane plane, ref Matrix matrix, out Plane result)
        {
            Matrix.Invert(ref matrix, out var result2);
            Matrix.Transpose(ref result2, out result2);
            Vector4 value = new Vector4(plane.Normal, plane.D);
            Vector4.Transform(ref value, ref result2, out var result3);
            result = new Plane(result3);
        }

        public static Plane Transform(Plane plane, Quaternion rotation)
        {
            Transform(ref plane, ref rotation, out var result);
            return result;
        }

        public static void Transform(ref Plane plane, ref Quaternion rotation, out Plane result)
        {
            Vector3.Transform(ref plane.Normal, ref rotation, out result.Normal);
            result.D = plane.D;
        }

        public void Normalize()
        {
            float num = Normal.Length();
            float num2 = 1f / num;
            Vector3.Multiply(ref Normal, num2, out Normal);
            D *= num2;
        }

        public static Plane Normalize(Plane value)
        {
            Normalize(ref value, out var result);
            return result;
        }

        public static void Normalize(ref Plane value, out Plane result)
        {
            float num = value.Normal.Length();
            float num2 = 1f / num;
            Vector3.Multiply(ref value.Normal, num2, out result.Normal);
            result.D = value.D * num2;
        }

        public static bool operator !=(Plane plane1, Plane plane2)
        {
            return !plane1.Equals(plane2);
        }

        public static bool operator ==(Plane plane1, Plane plane2)
        {
            return plane1.Equals(plane2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Plane))
            {
                return false;
            }

            return Equals((Plane)other);
        }

        public bool Equals(Plane other)
        {
            if (Normal == other.Normal)
            {
                return D == other.D;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Normal.GetHashCode() ^ D.GetHashCode();
        }

        internal PlaneIntersectionType Intersects(ref Vector3 point)
        {
            DotCoordinate(ref point, out var result);
            if (result > 0f)
            {
                return PlaneIntersectionType.Front;
            }

            if (result < 0f)
            {
                return PlaneIntersectionType.Back;
            }

            return PlaneIntersectionType.Intersecting;
        }

        public override string ToString()
        {
            string[] obj = new string[5] { "{Normal:", null, null, null, null };
            Vector3 normal = Normal;
            obj[1] = normal.ToString();
            obj[2] = " D:";
            obj[3] = D.ToString();
            obj[4] = "}";
            return string.Concat(obj);
        }

        public void Deconstruct(out Vector3 normal, out float d)
        {
            normal = Normal;
            d = D;
        }

        public System.Numerics.Plane ToNumerics()
        {
            return new System.Numerics.Plane(Normal.X, Normal.Y, Normal.Z, D);
        }

        public static implicit operator Plane(System.Numerics.Plane value)
        {
            return new Plane(value.Normal, value.D);
        }
    }
}
