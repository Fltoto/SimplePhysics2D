using SimpleECS;
using SimplePhysics2D.BoudingBox;
using SimplePhysics2D.Collision;
using SimplePhysics2D.Math;
using SimplePhysics2D.Shapes;
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
namespace SimplePhysics2D.RigidBody
{
    public class SPBody2D : SComponent
    {
        public Action<CollideInfo> OnCollide;

        private SPVector2 position;
        private SPVector2 linearVelocity;
        private float rotation;
        private float rotationalVelocity;

        private SPVector2 force;

        public readonly float Density;
        public readonly float Mass;
        public readonly float InvMass;
        public readonly float Restiution;
        public readonly float Area;

        public bool isStatic;

        public readonly float Radius;
        public readonly float Width;
        public readonly float Height;

        public ShapeType2D ShapeType;
        private readonly SPVector2[] Vertices;
        private bool transformUpdateRequired = true;
        private SPVector2[] transformVertices;
        private SAABB aabb;
        private bool aabbUpdateRequire=true;

        public float Rotation => rotation;
        public SPVector2 Position => position;
        public SPVector2 LinearVelocity { get => linearVelocity; set { linearVelocity = value; } }

        private SPBody2D(SPVector2 position, float density, float mass, float restiution, float area,
            bool isStatic, float radius, float width, float height, ShapeType2D shapetype)
        {
            this.position = position;
            this.linearVelocity = SPVector2.Zero;
            this.rotation = 0;
            this.rotationalVelocity = 0;

            this.Density = density;
            this.Mass = mass;
            this.Restiution = restiution;
            this.Area = area;
            this.isStatic = isStatic;
            this.Radius = radius;
            this.Width = width;
            this.Height = height;
            this.ShapeType = shapetype;

            if (!this.isStatic)
            {
                this.InvMass = 1f / Mass;
            }
            else {
                this.InvMass = 0;
            }

            if (ShapeType == ShapeType2D.Box)
            {
                Vertices = CreateBoxVertices(Width * 2, Height * 2);
                transformVertices = new SPVector2[Vertices.Length];
            }
            else
            {
                Vertices = new SPVector2[0];
                transformVertices = new SPVector2[0];
            }
        }
        private static SPVector2[] CreateBoxVertices(float width, float height)
        {
            float left = -width / 2f;
            float right = left + width;
            float bottom = -height / 2f;
            float top = bottom + height;

            SPVector2[] ret = new SPVector2[4];
            ret[0] = new SPVector2(left, top);
            ret[1] = new SPVector2(right, top);
            ret[2] = new SPVector2(right, bottom);
            ret[3] = new SPVector2(left, bottom);
            return ret;
        }
        public static bool CreateCircleBody(float radius, SPVector2 position, float density, bool isStatic, float restiution,
            out SPBody2D body, out string errormsg)
        {
            body = null;
            errormsg = string.Empty;
            float area = radius * radius * MathF.PI;
            if (area < SPWorld2D.MinBodySize)
            {
                errormsg = $"Area is too small, the min cirle area is {SPWorld2D.MinBodySize}";
                return false;
            }
            if (area > SPWorld2D.MaxBodySize)
            {
                errormsg = $"Area is too larg, the max cirle area is {SPWorld2D.MaxBodySize}";
                return false;
            }
            if (density < SPWorld2D.MinDensity)
            {
                errormsg = $"Density is too small, the min density is {SPWorld2D.MinDensity}";
                return false;
            }
            if (density > SPWorld2D.MaxDensity)
            {
                errormsg = $"Density is too large, the max density is {SPWorld2D.MaxDensity}";
                return false;
            }
            restiution = SPMath2D.Clamp(restiution, 0, 1);
            var mass = area * density;
            body = new SPBody2D(position, density, mass, restiution, area, isStatic, radius, 0, 0, ShapeType2D.Circle);
            return true;
        }
        public static bool CreateBoxBody(float width, float height, SPVector2 position, float density, bool isStatic, float restiution,
    out SPBody2D body, out string errormsg)
        {
            body = null;
            errormsg = string.Empty;
            float area = width * height;
            if (area < SPWorld2D.MinBodySize)
            {
                errormsg = $"Area is too small, the min cirle area is {SPWorld2D.MinBodySize}";
                return false;
            }
            if (area > SPWorld2D.MaxBodySize)
            {
                errormsg = $"Area is too larg, the max cirle area is {SPWorld2D.MaxBodySize}";
                return false;
            }
            if (density < SPWorld2D.MinDensity)
            {
                errormsg = $"Density is too small, the min density is {SPWorld2D.MinDensity}";
                return false;
            }
            if (density > SPWorld2D.MaxDensity)
            {
                errormsg = $"Density is too large, the max density is {SPWorld2D.MaxDensity}";
                return false;
            }
            restiution = SPMath2D.Clamp(restiution, 0, 1);
            var mass = area * density;
            body = new SPBody2D(position, density, mass, restiution, area, isStatic, 0, width, height, ShapeType2D.Box);
            return true;
        }

        public SPVector2[] GetTransformVertices()
        {
            if (this.transformUpdateRequired)
            {
                Translation trans = new Translation(this.position, this.rotation);
                for (int i = 0; i < this.Vertices.Length; i++)
                {
                    SPVector2 v = this.Vertices[i];
                    this.transformVertices[i] = SPVector2.Transform(v, trans);
                }
                this.transformUpdateRequired = false;
            }
            return transformVertices;
        }

        public SAABB GetAABB() {
            if (!aabbUpdateRequire) {
                return aabb;
            }
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            if (ShapeType is ShapeType2D.Circle)
            {
                minX = Position.X - Radius;
                minY = position.Y - Radius;
                maxX = position.X + Radius;
                maxY = position.Y + Radius;
            }
            else if (ShapeType is ShapeType2D.Box)
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    SPVector2 v = Vertices[i]+Position;
                    if (v.X < minX) { minX = v.X; }
                    if (v.Y < minY) { minY = v.Y; }
                    if (v.X > maxX) { maxX = v.X; }
                    if (v.Y > maxY) { maxY = v.Y; }
                }
            }
            else throw new Exception("UnKnown type");
            aabb=new SAABB(minX,minY,maxX,maxY);
            aabbUpdateRequire = false;
            return aabb;
        }

        public void Step(float time,int Iterations)
        {
            for (int i=0;i<Iterations;i++) {
                SPVector2 acceleration = this.force / this.Mass;
                this.linearVelocity += acceleration * time;
                Move(linearVelocity * time);
                Rotate(rotationalVelocity * time);
                this.force = SPVector2.Zero;
            }
        }
        public void Move(SPVector2 vec)
        {
            this.position += vec;
            this.transformUpdateRequired = true;
            aabbUpdateRequire = true;
        }
        public void MoveTo(SPVector2 pos)
        {
            this.position = pos;
            this.transformUpdateRequired = true;
            aabbUpdateRequire = true;
        }
        public void Rotate(float angle)
        {
            this.rotation += angle;
            this.transformUpdateRequired = true;
        }
        public void AddForce(SPVector2 force)
        {
            this.force += force;
        }
    }
}
