using System;

namespace SimplePhysics2D
{
    public static class SPMath2D
    {
        public static readonly float VerySmallAmount = 0.0005f;

        public static float Length(Vector2 other)
        {
            return MathF.Sqrt(other.X * other.X + other.Y * other.Y);
        }
        public static float LengthSquared(Vector2 other)
        {
            return other.X * other.X + other.Y * other.Y;
        }
        public static float Distance(Vector2 a, Vector2 b)
        {
            return Length(a - b);
        }
        public static float DistanceSquared(Vector2 a, Vector2 b)
        {
            return LengthSquared(a - b);
        }
        public static Vector2 Normalize(Vector2 a)
        {
            var len = Length(a);
            return new Vector2(a.X / len, a.Y / len);
        }
        public static float Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
        public static float Cross(Vector2 a, Vector2 b)
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
        public static float Angle(Vector2 form, Vector2 to)
        {
            float x = to.X - form.X;
            float y = to.Y - form.Y;

            float hy = MathF.Sqrt(MathF.Pow(x, 2) + MathF.Pow(y, 2f));

            float cos = x / hy;
            float radian = MathF.Acos(cos);

            float angle = 180 / (MathF.PI / radian);

            //if (x <= 0 && y > 0) angle = 180 - angle;
            //if (x <= 0 && y < 0) angle = 180 + angle;
            //if (x > 0 && y <=0) angle = 360 - angle;

            if (y < 0) angle = 360 - angle;   // if (y < 0) angle = - angle;   //-180-180
            else if ((y == 0) && (x < 0)) angle = 180;

            return angle;
        }
        public static bool NearlyEqual(float a, float b)
        {
            return MathF.Abs(a - b) < VerySmallAmount;
        }
        public static bool NearlyEqual(Vector2 a, Vector2 b)
        {
            return DistanceSquared(a, b) < VerySmallAmount * VerySmallAmount;
        }
    }
}
