using SimplePhysics2D.RigidBody;
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
    public struct CollideInfo
    {
        public SPBody2D hited { get; }
        public SPVector2 normal { get; }
        public float depth { get; }
        public CollideInfo(SPBody2D body, SPVector2 normal, float depth)
        {
            hited = body;
            this.normal = normal;
            this.depth = depth;
        }
    }
}
