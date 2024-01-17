using System;

namespace SimplePhysics2D
{
    public static class SPMath2D
    {
        public static readonly float VerySmallAmount = 0.0005f;

        public static float Length(SPVector2 other)
        {
            return MathF.Sqrt(other.X * other.X + other.Y * other.Y);
        }
        public static float LengthSquared(SPVector2 other)
        {
            return other.X * other.X + other.Y * other.Y;
        }
        public static float Distance(SPVector2 a, SPVector2 b)
        {
            return Length(a - b);
        }
        public static float DistanceSquared(SPVector2 a, SPVector2 b)
        {
            return LengthSquared(a - b);
        }
        public static SPVector2 Normalize(SPVector2 a)
        {
            var len = Length(a);
            return new SPVector2(a.X / len, a.Y / len);
        }
        public static float Dot(SPVector2 a, SPVector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
        public static float Cross(SPVector2 a, SPVector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            return value;
        }
        public static bool NearlyEqual(float a, float b)
        {
            return MathF.Abs(a - b) < VerySmallAmount;
        }
        public static bool NearlyEqual(SPVector2 a, SPVector2 b)
        {
            return DistanceSquared(a, b) < VerySmallAmount * VerySmallAmount;
        }
    }
}
