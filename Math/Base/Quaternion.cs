using System;
using System.Numerics;
using System.Runtime.Serialization;

namespace SimplePhysics2D
{
    public struct Quaternion : IEquatable<Quaternion>
    {
        private static readonly Quaternion _identity = new Quaternion(0f, 0f, 0f, 1f);

        [DataMember]
        public float X;

        [DataMember]
        public float Y;

        [DataMember]
        public float Z;

        [DataMember]
        public float W;

        public static Quaternion Identity => _identity;

        internal string DebugDisplayString
        {
            get
            {
                if (this == _identity)
                {
                    return "Identity";
                }

                return X + " " + Y + " " + Z + " " + W;
            }
        }

        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Quaternion(Vector3 value, float w)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }

        public Quaternion(Vector4 value)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = value.W;
        }

        public static Quaternion Add(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion result = default(Quaternion);
            result.X = quaternion1.X + quaternion2.X;
            result.Y = quaternion1.Y + quaternion2.Y;
            result.Z = quaternion1.Z + quaternion2.Z;
            result.W = quaternion1.W + quaternion2.W;
            return result;
        }

        public static void Add(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            result.X = quaternion1.X + quaternion2.X;
            result.Y = quaternion1.Y + quaternion2.Y;
            result.Z = quaternion1.Z + quaternion2.Z;
            result.W = quaternion1.W + quaternion2.W;
        }

        public static Quaternion Concatenate(Quaternion value1, Quaternion value2)
        {
            float x = value1.X;
            float y = value1.Y;
            float z = value1.Z;
            float w = value1.W;
            float x2 = value2.X;
            float y2 = value2.Y;
            float z2 = value2.Z;
            float w2 = value2.W;
            Quaternion result = default(Quaternion);
            result.X = x2 * w + x * w2 + (y2 * z - z2 * y);
            result.Y = y2 * w + y * w2 + (z2 * x - x2 * z);
            result.Z = z2 * w + z * w2 + (x2 * y - y2 * x);
            result.W = w2 * w - (x2 * x + y2 * y + z2 * z);
            return result;
        }

        public static void Concatenate(ref Quaternion value1, ref Quaternion value2, out Quaternion result)
        {
            float x = value1.X;
            float y = value1.Y;
            float z = value1.Z;
            float w = value1.W;
            float x2 = value2.X;
            float y2 = value2.Y;
            float z2 = value2.Z;
            float w2 = value2.W;
            result.X = x2 * w + x * w2 + (y2 * z - z2 * y);
            result.Y = y2 * w + y * w2 + (z2 * x - x2 * z);
            result.Z = z2 * w + z * w2 + (x2 * y - y2 * x);
            result.W = w2 * w - (x2 * x + y2 * y + z2 * z);
        }

        public void Conjugate()
        {
            X = 0f - X;
            Y = 0f - Y;
            Z = 0f - Z;
        }

        public static Quaternion Conjugate(Quaternion value)
        {
            return new Quaternion(0f - value.X, 0f - value.Y, 0f - value.Z, value.W);
        }

        public static void Conjugate(ref Quaternion value, out Quaternion result)
        {
            result.X = 0f - value.X;
            result.Y = 0f - value.Y;
            result.Z = 0f - value.Z;
            result.W = value.W;
        }

        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            float x = angle * 0.5f;
            float num = MathF.Sin(x);
            float w = MathF.Cos(x);
            return new Quaternion(axis.X * num, axis.Y * num, axis.Z * num, w);
        }

        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Quaternion result)
        {
            float x = angle * 0.5f;
            float num = MathF.Sin(x);
            float w = MathF.Cos(x);
            result.X = axis.X * num;
            result.Y = axis.Y * num;
            result.Z = axis.Z * num;
            result.W = w;
        }

        public static Quaternion CreateFromRotationMatrix(Matrix matrix)
        {
            float num = matrix.M11 + matrix.M22 + matrix.M33;
            Quaternion result = default(Quaternion);
            float num2;
            if (num > 0f)
            {
                num2 = MathF.Sqrt(num + 1f);
                result.W = num2 * 0.5f;
                num2 = 0.5f / num2;
                result.X = (matrix.M23 - matrix.M32) * num2;
                result.Y = (matrix.M31 - matrix.M13) * num2;
                result.Z = (matrix.M12 - matrix.M21) * num2;
                return result;
            }

            float num3;
            if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
            {
                num2 = MathF.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33);
                num3 = 0.5f / num2;
                result.X = 0.5f * num2;
                result.Y = (matrix.M12 + matrix.M21) * num3;
                result.Z = (matrix.M13 + matrix.M31) * num3;
                result.W = (matrix.M23 - matrix.M32) * num3;
                return result;
            }

            if (matrix.M22 > matrix.M33)
            {
                num2 = MathF.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33);
                num3 = 0.5f / num2;
                result.X = (matrix.M21 + matrix.M12) * num3;
                result.Y = 0.5f * num2;
                result.Z = (matrix.M32 + matrix.M23) * num3;
                result.W = (matrix.M31 - matrix.M13) * num3;
                return result;
            }

            num2 = MathF.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22);
            num3 = 0.5f / num2;
            result.X = (matrix.M31 + matrix.M13) * num3;
            result.Y = (matrix.M32 + matrix.M23) * num3;
            result.Z = 0.5f * num2;
            result.W = (matrix.M12 - matrix.M21) * num3;
            return result;
        }

        public static void CreateFromRotationMatrix(ref Matrix matrix, out Quaternion result)
        {
            float num = matrix.M11 + matrix.M22 + matrix.M33;
            if (num > 0f)
            {
                float num2 = MathF.Sqrt(num + 1f);
                result.W = num2 * 0.5f;
                num2 = 0.5f / num2;
                result.X = (matrix.M23 - matrix.M32) * num2;
                result.Y = (matrix.M31 - matrix.M13) * num2;
                result.Z = (matrix.M12 - matrix.M21) * num2;
            }
            else if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
            {
                float num2 = MathF.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33);
                float num3 = 0.5f / num2;
                result.X = 0.5f * num2;
                result.Y = (matrix.M12 + matrix.M21) * num3;
                result.Z = (matrix.M13 + matrix.M31) * num3;
                result.W = (matrix.M23 - matrix.M32) * num3;
            }
            else if (matrix.M22 > matrix.M33)
            {
                float num2 = MathF.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33);
                float num3 = 0.5f / num2;
                result.X = (matrix.M21 + matrix.M12) * num3;
                result.Y = 0.5f * num2;
                result.Z = (matrix.M32 + matrix.M23) * num3;
                result.W = (matrix.M31 - matrix.M13) * num3;
            }
            else
            {
                float num2 = MathF.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22);
                float num3 = 0.5f / num2;
                result.X = (matrix.M31 + matrix.M13) * num3;
                result.Y = (matrix.M32 + matrix.M23) * num3;
                result.Z = 0.5f * num2;
                result.W = (matrix.M12 - matrix.M21) * num3;
            }
        }

        public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            float x = roll * 0.5f;
            float x2 = pitch * 0.5f;
            float x3 = yaw * 0.5f;
            float num = MathF.Sin(x);
            float num2 = MathF.Cos(x);
            float num3 = MathF.Sin(x2);
            float num4 = MathF.Cos(x2);
            float num5 = MathF.Sin(x3);
            float num6 = MathF.Cos(x3);
            return new Quaternion(num6 * num3 * num2 + num5 * num4 * num, num5 * num4 * num2 - num6 * num3 * num, num6 * num4 * num - num5 * num3 * num2, num6 * num4 * num2 + num5 * num3 * num);
        }

        public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result)
        {
            float x = roll * 0.5f;
            float x2 = pitch * 0.5f;
            float x3 = yaw * 0.5f;
            float num = MathF.Sin(x);
            float num2 = MathF.Cos(x);
            float num3 = MathF.Sin(x2);
            float num4 = MathF.Cos(x2);
            float num5 = MathF.Sin(x3);
            float num6 = MathF.Cos(x3);
            result.X = num6 * num3 * num2 + num5 * num4 * num;
            result.Y = num5 * num4 * num2 - num6 * num3 * num;
            result.Z = num6 * num4 * num - num5 * num3 * num2;
            result.W = num6 * num4 * num2 + num5 * num3 * num;
        }

        public static Quaternion Divide(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x = quaternion1.X;
            float y = quaternion1.Y;
            float z = quaternion1.Z;
            float w = quaternion1.W;
            float num = quaternion2.X * quaternion2.X + quaternion2.Y * quaternion2.Y + quaternion2.Z * quaternion2.Z + quaternion2.W * quaternion2.W;
            float num2 = 1f / num;
            float num3 = (0f - quaternion2.X) * num2;
            float num4 = (0f - quaternion2.Y) * num2;
            float num5 = (0f - quaternion2.Z) * num2;
            float num6 = quaternion2.W * num2;
            float num7 = y * num5 - z * num4;
            float num8 = z * num3 - x * num5;
            float num9 = x * num4 - y * num3;
            float num10 = x * num3 + y * num4 + z * num5;
            Quaternion result = default(Quaternion);
            result.X = x * num6 + num3 * w + num7;
            result.Y = y * num6 + num4 * w + num8;
            result.Z = z * num6 + num5 * w + num9;
            result.W = w * num6 - num10;
            return result;
        }

        public static void Divide(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            float x = quaternion1.X;
            float y = quaternion1.Y;
            float z = quaternion1.Z;
            float w = quaternion1.W;
            float num = quaternion2.X * quaternion2.X + quaternion2.Y * quaternion2.Y + quaternion2.Z * quaternion2.Z + quaternion2.W * quaternion2.W;
            float num2 = 1f / num;
            float num3 = (0f - quaternion2.X) * num2;
            float num4 = (0f - quaternion2.Y) * num2;
            float num5 = (0f - quaternion2.Z) * num2;
            float num6 = quaternion2.W * num2;
            float num7 = y * num5 - z * num4;
            float num8 = z * num3 - x * num5;
            float num9 = x * num4 - y * num3;
            float num10 = x * num3 + y * num4 + z * num5;
            result.X = x * num6 + num3 * w + num7;
            result.Y = y * num6 + num4 * w + num8;
            result.Z = z * num6 + num5 * w + num9;
            result.W = w * num6 - num10;
        }

        public static float Dot(Quaternion quaternion1, Quaternion quaternion2)
        {
            return quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
        }

        public static void Dot(ref Quaternion quaternion1, ref Quaternion quaternion2, out float result)
        {
            result = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
        }

        public override bool Equals(object obj)
        {
            if (obj is Quaternion)
            {
                return Equals((Quaternion)obj);
            }

            return false;
        }

        public bool Equals(Quaternion other)
        {
            if (X == other.X && Y == other.Y && Z == other.Z)
            {
                return W == other.W;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
        }

        public static Quaternion Inverse(Quaternion quaternion)
        {
            float num = quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W;
            float num2 = 1f / num;
            Quaternion result = default(Quaternion);
            result.X = (0f - quaternion.X) * num2;
            result.Y = (0f - quaternion.Y) * num2;
            result.Z = (0f - quaternion.Z) * num2;
            result.W = quaternion.W * num2;
            return result;
        }

        public static void Inverse(ref Quaternion quaternion, out Quaternion result)
        {
            float num = quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W;
            float num2 = 1f / num;
            result.X = (0f - quaternion.X) * num2;
            result.Y = (0f - quaternion.Y) * num2;
            result.Z = (0f - quaternion.Z) * num2;
            result.W = quaternion.W * num2;
        }

        public float Length()
        {
            return MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);
        }

        public float LengthSquared()
        {
            return X * X + Y * Y + Z * Z + W * W;
        }

        public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
        {
            float num = 1f - amount;
            Quaternion result = default(Quaternion);
            if (quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W >= 0f)
            {
                result.X = num * quaternion1.X + amount * quaternion2.X;
                result.Y = num * quaternion1.Y + amount * quaternion2.Y;
                result.Z = num * quaternion1.Z + amount * quaternion2.Z;
                result.W = num * quaternion1.W + amount * quaternion2.W;
            }
            else
            {
                result.X = num * quaternion1.X - amount * quaternion2.X;
                result.Y = num * quaternion1.Y - amount * quaternion2.Y;
                result.Z = num * quaternion1.Z - amount * quaternion2.Z;
                result.W = num * quaternion1.W - amount * quaternion2.W;
            }

            float x = result.X * result.X + result.Y * result.Y + result.Z * result.Z + result.W * result.W;
            float num2 = 1f / MathF.Sqrt(x);
            result.X *= num2;
            result.Y *= num2;
            result.Z *= num2;
            result.W *= num2;
            return result;
        }

        public static void Lerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
        {
            float num = 1f - amount;
            if (quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W >= 0f)
            {
                result.X = num * quaternion1.X + amount * quaternion2.X;
                result.Y = num * quaternion1.Y + amount * quaternion2.Y;
                result.Z = num * quaternion1.Z + amount * quaternion2.Z;
                result.W = num * quaternion1.W + amount * quaternion2.W;
            }
            else
            {
                result.X = num * quaternion1.X - amount * quaternion2.X;
                result.Y = num * quaternion1.Y - amount * quaternion2.Y;
                result.Z = num * quaternion1.Z - amount * quaternion2.Z;
                result.W = num * quaternion1.W - amount * quaternion2.W;
            }

            float x = result.X * result.X + result.Y * result.Y + result.Z * result.Z + result.W * result.W;
            float num2 = 1f / MathF.Sqrt(x);
            result.X *= num2;
            result.Y *= num2;
            result.Z *= num2;
            result.W *= num2;
        }

        public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
        {
            float num = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
            bool flag = false;
            if (num < 0f)
            {
                flag = true;
                num = 0f - num;
            }

            float num2;
            float num3;
            if (num > 0.999999f)
            {
                num2 = 1f - amount;
                num3 = (flag ? (0f - amount) : amount);
            }
            else
            {
                float num4 = MathF.Acos(num);
                float num5 = (float)(1.0 / Math.Sin(num4));
                num2 = MathF.Sin((1f - amount) * num4) * num5;
                num3 = (flag ? ((0f - MathF.Sin(amount * num4)) * num5) : (MathF.Sin(amount * num4) * num5));
            }

            Quaternion result = default(Quaternion);
            result.X = num2 * quaternion1.X + num3 * quaternion2.X;
            result.Y = num2 * quaternion1.Y + num3 * quaternion2.Y;
            result.Z = num2 * quaternion1.Z + num3 * quaternion2.Z;
            result.W = num2 * quaternion1.W + num3 * quaternion2.W;
            return result;
        }

        public static void Slerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
        {
            float num = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
            bool flag = false;
            if (num < 0f)
            {
                flag = true;
                num = 0f - num;
            }

            float num2;
            float num3;
            if (num > 0.999999f)
            {
                num2 = 1f - amount;
                num3 = (flag ? (0f - amount) : amount);
            }
            else
            {
                float num4 = MathF.Acos(num);
                float num5 = (float)(1.0 / Math.Sin(num4));
                num2 = MathF.Sin((1f - amount) * num4) * num5;
                num3 = (flag ? ((0f - MathF.Sin(amount * num4)) * num5) : (MathF.Sin(amount * num4) * num5));
            }

            result.X = num2 * quaternion1.X + num3 * quaternion2.X;
            result.Y = num2 * quaternion1.Y + num3 * quaternion2.Y;
            result.Z = num2 * quaternion1.Z + num3 * quaternion2.Z;
            result.W = num2 * quaternion1.W + num3 * quaternion2.W;
        }

        public static Quaternion Subtract(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion result = default(Quaternion);
            result.X = quaternion1.X - quaternion2.X;
            result.Y = quaternion1.Y - quaternion2.Y;
            result.Z = quaternion1.Z - quaternion2.Z;
            result.W = quaternion1.W - quaternion2.W;
            return result;
        }

        public static void Subtract(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            result.X = quaternion1.X - quaternion2.X;
            result.Y = quaternion1.Y - quaternion2.Y;
            result.Z = quaternion1.Z - quaternion2.Z;
            result.W = quaternion1.W - quaternion2.W;
        }

        public static Quaternion Multiply(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x = quaternion1.X;
            float y = quaternion1.Y;
            float z = quaternion1.Z;
            float w = quaternion1.W;
            float x2 = quaternion2.X;
            float y2 = quaternion2.Y;
            float z2 = quaternion2.Z;
            float w2 = quaternion2.W;
            float num = y * z2 - z * y2;
            float num2 = z * x2 - x * z2;
            float num3 = x * y2 - y * x2;
            float num4 = x * x2 + y * y2 + z * z2;
            Quaternion result = default(Quaternion);
            result.X = x * w2 + x2 * w + num;
            result.Y = y * w2 + y2 * w + num2;
            result.Z = z * w2 + z2 * w + num3;
            result.W = w * w2 - num4;
            return result;
        }

        public static Quaternion Multiply(Quaternion quaternion1, float scaleFactor)
        {
            Quaternion result = default(Quaternion);
            result.X = quaternion1.X * scaleFactor;
            result.Y = quaternion1.Y * scaleFactor;
            result.Z = quaternion1.Z * scaleFactor;
            result.W = quaternion1.W * scaleFactor;
            return result;
        }

        public static void Multiply(ref Quaternion quaternion1, float scaleFactor, out Quaternion result)
        {
            result.X = quaternion1.X * scaleFactor;
            result.Y = quaternion1.Y * scaleFactor;
            result.Z = quaternion1.Z * scaleFactor;
            result.W = quaternion1.W * scaleFactor;
        }

        public static void Multiply(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            float x = quaternion1.X;
            float y = quaternion1.Y;
            float z = quaternion1.Z;
            float w = quaternion1.W;
            float x2 = quaternion2.X;
            float y2 = quaternion2.Y;
            float z2 = quaternion2.Z;
            float w2 = quaternion2.W;
            float num = y * z2 - z * y2;
            float num2 = z * x2 - x * z2;
            float num3 = x * y2 - y * x2;
            float num4 = x * x2 + y * y2 + z * z2;
            result.X = x * w2 + x2 * w + num;
            result.Y = y * w2 + y2 * w + num2;
            result.Z = z * w2 + z2 * w + num3;
            result.W = w * w2 - num4;
        }

        public static Quaternion Negate(Quaternion quaternion)
        {
            return new Quaternion(0f - quaternion.X, 0f - quaternion.Y, 0f - quaternion.Z, 0f - quaternion.W);
        }

        public static void Negate(ref Quaternion quaternion, out Quaternion result)
        {
            result.X = 0f - quaternion.X;
            result.Y = 0f - quaternion.Y;
            result.Z = 0f - quaternion.Z;
            result.W = 0f - quaternion.W;
        }

        public void Normalize()
        {
            float num = 1f / MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);
            X *= num;
            Y *= num;
            Z *= num;
            W *= num;
        }

        public static Quaternion Normalize(Quaternion quaternion)
        {
            float num = 1f / MathF.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W);
            Quaternion result = default(Quaternion);
            result.X = quaternion.X * num;
            result.Y = quaternion.Y * num;
            result.Z = quaternion.Z * num;
            result.W = quaternion.W * num;
            return result;
        }

        public static void Normalize(ref Quaternion quaternion, out Quaternion result)
        {
            float num = 1f / MathF.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W);
            result.X = quaternion.X * num;
            result.Y = quaternion.Y * num;
            result.Z = quaternion.Z * num;
            result.W = quaternion.W * num;
        }

        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + " Z:" + Z + " W:" + W + "}";
        }

        public Vector4 ToVector4()
        {
            return new Vector4(X, Y, Z, W);
        }

        public void Deconstruct(out float x, out float y, out float z, out float w)
        {
            x = X;
            y = Y;
            z = Z;
            w = W;
        }

        public System.Numerics.Quaternion ToNumerics()
        {
            return new System.Numerics.Quaternion(X, Y, Z, W);
        }

        public static implicit operator Quaternion(System.Numerics.Quaternion value)
        {
            return new Quaternion(value.X, value.Y, value.Z, value.W);
        }

        public static Quaternion operator +(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion result = default(Quaternion);
            result.X = quaternion1.X + quaternion2.X;
            result.Y = quaternion1.Y + quaternion2.Y;
            result.Z = quaternion1.Z + quaternion2.Z;
            result.W = quaternion1.W + quaternion2.W;
            return result;
        }

        public static Quaternion operator /(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x = quaternion1.X;
            float y = quaternion1.Y;
            float z = quaternion1.Z;
            float w = quaternion1.W;
            float num = quaternion2.X * quaternion2.X + quaternion2.Y * quaternion2.Y + quaternion2.Z * quaternion2.Z + quaternion2.W * quaternion2.W;
            float num2 = 1f / num;
            float num3 = (0f - quaternion2.X) * num2;
            float num4 = (0f - quaternion2.Y) * num2;
            float num5 = (0f - quaternion2.Z) * num2;
            float num6 = quaternion2.W * num2;
            float num7 = y * num5 - z * num4;
            float num8 = z * num3 - x * num5;
            float num9 = x * num4 - y * num3;
            float num10 = x * num3 + y * num4 + z * num5;
            Quaternion result = default(Quaternion);
            result.X = x * num6 + num3 * w + num7;
            result.Y = y * num6 + num4 * w + num8;
            result.Z = z * num6 + num5 * w + num9;
            result.W = w * num6 - num10;
            return result;
        }

        public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2)
        {
            if (quaternion1.X == quaternion2.X && quaternion1.Y == quaternion2.Y && quaternion1.Z == quaternion2.Z)
            {
                return quaternion1.W == quaternion2.W;
            }

            return false;
        }

        public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2)
        {
            if (quaternion1.X == quaternion2.X && quaternion1.Y == quaternion2.Y && quaternion1.Z == quaternion2.Z)
            {
                return quaternion1.W != quaternion2.W;
            }

            return true;
        }

        public static Quaternion operator *(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x = quaternion1.X;
            float y = quaternion1.Y;
            float z = quaternion1.Z;
            float w = quaternion1.W;
            float x2 = quaternion2.X;
            float y2 = quaternion2.Y;
            float z2 = quaternion2.Z;
            float w2 = quaternion2.W;
            float num = y * z2 - z * y2;
            float num2 = z * x2 - x * z2;
            float num3 = x * y2 - y * x2;
            float num4 = x * x2 + y * y2 + z * z2;
            Quaternion result = default(Quaternion);
            result.X = x * w2 + x2 * w + num;
            result.Y = y * w2 + y2 * w + num2;
            result.Z = z * w2 + z2 * w + num3;
            result.W = w * w2 - num4;
            return result;
        }

        public static Quaternion operator *(Quaternion quaternion1, float scaleFactor)
        {
            Quaternion result = default(Quaternion);
            result.X = quaternion1.X * scaleFactor;
            result.Y = quaternion1.Y * scaleFactor;
            result.Z = quaternion1.Z * scaleFactor;
            result.W = quaternion1.W * scaleFactor;
            return result;
        }

        public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion result = default(Quaternion);
            result.X = quaternion1.X - quaternion2.X;
            result.Y = quaternion1.Y - quaternion2.Y;
            result.Z = quaternion1.Z - quaternion2.Z;
            result.W = quaternion1.W - quaternion2.W;
            return result;
        }

        public static Quaternion operator -(Quaternion quaternion)
        {
            Quaternion result = default(Quaternion);
            result.X = 0f - quaternion.X;
            result.Y = 0f - quaternion.Y;
            result.Z = 0f - quaternion.Z;
            result.W = 0f - quaternion.W;
            return result;
        }
    }
}
