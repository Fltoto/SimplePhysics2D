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
    public readonly struct SPVector2
    {
        public readonly float X;
        public readonly float Y;

        public static SPVector2 Zero = new SPVector2(0, 0);


        public SPVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static SPVector2 operator +(SPVector2 a, SPVector2 b)
        {
            return new SPVector2(a.X + b.X, a.Y + b.Y);
        }
        public static SPVector2 operator -(SPVector2 a, SPVector2 b)
        {
            return new SPVector2(a.X - b.X, a.Y - b.Y);
        }
        public static SPVector2 operator -(SPVector2 a)
        {
            return new SPVector2(-a.X, -a.Y);
        }
        public static SPVector2 operator *(SPVector2 a, SPVector2 b)
        {
            return new SPVector2(a.X * b.X, a.Y * b.Y);
        }
        public static SPVector2 operator *(SPVector2 a, float aim)
        {
            return new SPVector2(a.X * aim, a.Y * aim);
        }
        public static SPVector2 operator *(float aim, SPVector2 a)
        {
            return new SPVector2(a.X * aim, a.Y * aim);
        }
        public static SPVector2 operator /(SPVector2 a, float aim)
        {
            return new SPVector2(a.X / aim, a.Y / aim);
        }
        public static SPVector2 operator /(float aim, SPVector2 a)
        {
            return new SPVector2(a.X / aim, a.Y / aim);
        }
        public static SPVector2 operator /(SPVector2 a, SPVector2 b)
        {
            return new SPVector2(a.X / b.X, a.Y / b.Y);
        }
        internal static SPVector2 Transform(SPVector2 v, Translation transform)
        {
            return new SPVector2(
                transform.Cos * v.X - transform.Sin * v.Y + transform.PositionX,
                transform.Sin * v.X + transform.Cos * v.Y + transform.PositionY);
        }
        public bool Equals(SPVector2 Other)
        {
            return X == Other.X && Y == Other.Y;
        }
        public override bool Equals(object obj)
        {
            if (obj is SPVector2 Other)
            {
                return Equals(Other);
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return new { this.X, this.Y }.GetHashCode();
        }
        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
}
