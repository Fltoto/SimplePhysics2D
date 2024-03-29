﻿/*
# THIS FILE IS PART OF SimplePhysics2D
# 
# THIS PROGRAM IS FREE SOFTWARE, WE USED APACHE2.0 LICENSE.
# YOU SHOULD HAVE RECEIVED A COPY OF APACHE2.0 LICENSE.
#
# THIS STATEMENT APPLIES TO THE ENTIRE PROJECT 
#
# Copyright (c) 2024 Fltoto
*/
using SimplePhysics2D.Collision;

namespace SimplePhysics2D.BoudingBox
{
    public readonly struct SAABB
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;
        public float Width
        {
            get
            {
                return (Max.X - Min.X);
            }
        }
        public float Height
        {
            get
            {
                return (Max.Y - Min.Y);
            }
        }
        public float Area
        {
            get
            {
                return Width * Height;
            }
        }
        public SAABB(Vector2 Min, Vector2 Max)
        {
            this.Min = Min;
            this.Max = Max;
        }

        public SAABB(float minX, float minY, float maxX, float maxY)
        {
            this.Min = new Vector2(minX, minY);
            this.Max = new Vector2(maxX, maxY);
        }
        public bool Insect(SAABB Other)
        {
            return Collisions.IntersectAABBs(this, Other);
        }

        public override bool Equals(object obj)
        {
            var b = (SAABB)obj;
            return ((Min.Equals(b.Min) && (Max.Equals(b.Max))));
        }
    }
}
