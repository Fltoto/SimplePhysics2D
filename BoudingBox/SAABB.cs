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
using SimplePhysics2D.Collision;

namespace SimplePhysics2D.BoudingBox
{
    public readonly struct SAABB
    {
        public readonly SPVector2 Min;
        public readonly SPVector2 Max;
        public float Width {
            get {
                return (Max.X - Min.X);
            }
        }
        public float Height
        {
            get {
                return (Max.Y - Min.Y);
            }
        }
        public SAABB(SPVector2 Min,SPVector2 Max) {
            this.Min = Min;
            this.Max = Max;
        }

        public SAABB(float minX,float minY,float maxX,float maxY) {
            this.Min = new SPVector2(minX,minY);
            this.Max = new SPVector2(maxX,maxY);
        }
        public bool Insect(SAABB Other) {
            return Collisions.IntersectAABBs(this,Other);
        }
    }
}
