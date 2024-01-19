using SimpleECS;
using SimplePhysics2D.BoudingBox;
using SimplePhysics2D.Collision;
using SimplePhysics2D.Shapes;
using System;
using System.Collections.Generic;

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
        public Action<SManifold> OnCollide;

        private Vector2 position;
        private Vector2 linearVelocity;
        private float rotation;
        private float rotationalVelocity;

        private Vector2 force;

        public readonly float Density;
        public float Mass { get; private set; }
        public float InvMass { get; private set; }
        public readonly float Restitution;
        public readonly float Area;

        public readonly float Inertia;
        public readonly float InvInertia;
        public readonly float StaticFriction;
        public readonly float DynamicFriction;

        public bool IsStatic;
        public bool IsTrigger;
        public bool RotFreeze;
        public bool VelocityFreeze;

        public readonly float Radius;
        public readonly float Width;
        public readonly float Height;

        public ShapeType2D ShapeType;
        public SPWorld2D SPWorld;
        public SAABB LastAABB;

        private List<Vector2[]> Vertices;
        private bool transformUpdateRequired = true;
        private List<Vector2[]> transformVertices;
        private SAABB aabb;
        private bool aabbUpdateRequire = true;

        public float Rotation => rotation;
        public Vector2 Position => position;
        public Vector2 LinearVelocity { get => linearVelocity; set { linearVelocity = value; } }
        public float AngularVelocity { get => rotationalVelocity; set { rotationalVelocity = value; } }

        private SPBody2D(Vector2 position, float density, float mass, float inertia, float restiution, float area,
            bool isStatic, float radius, float width, float height, float staticFriction, float dynamticFriction, ShapeType2D shapetype)
        {
            this.position = position;
            this.linearVelocity = Vector2.Zero;
            this.rotation = 0;
            this.rotationalVelocity = 0;

            this.Density = density;
            this.Mass = mass;
            this.Restitution = restiution;
            this.Area = area;
            this.IsStatic = isStatic;
            this.Radius = radius;
            this.Width = width;
            this.Height = height;
            this.ShapeType = shapetype;
            this.StaticFriction = staticFriction;
            this.DynamicFriction = dynamticFriction;
            this.Inertia = inertia;
            this.InvInertia = inertia > 0f ? 1f / inertia : 0f;

            if (!this.IsStatic)
            {
                this.InvMass = 1f / Mass;
            }
            else
            {
                this.InvMass = 0;
            }

            if (ShapeType == ShapeType2D.Box)
            {
                Vertices = new List<Vector2[]>() { CreateBoxVertices(Width, Height,Vector2.Zero) };
                transformVertices = new List<Vector2[]>() { new Vector2[Vertices[0].Length]};
            }
            else
            {
                Vertices = new List<Vector2[]>();
                transformVertices = new List<Vector2[]>();
            }
        }
        public static Vector2[] CreateBoxVertices(float width, float height,Vector2 offset)
        {
            float left = -width / 2f;
            float right = left + width;
            float bottom = -height / 2f;
            float top = bottom + height;

            Vector2[] ret = new Vector2[4];
            ret[0] = new Vector2(left, top)+offset;
            ret[1] = new Vector2(right, top)+offset;
            ret[2] = new Vector2(right, bottom)+offset;
            ret[3] = new Vector2(left, bottom)+offset;
            return ret;
        }
        public static bool CreateCircleBody(float radius, Vector2 position, float density, bool isStatic, float restiution,
            float staticFriction, float dynamticFriction, out SPBody2D body, out string errormsg)
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
            float mass = area * density;
            float inertia = (1f / 2f) * mass * radius * radius;
            body = new SPBody2D(position, density, mass, inertia, restiution, area, isStatic, radius, 0, 0, staticFriction, dynamticFriction, ShapeType2D.Circle);
            return true;
        }
        public static bool CreateBoxBody(float width, float height, Vector2 position, float density, bool isStatic, float restiution, float staticFriction, float dynamticFriction,
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
            var inertia = (1f / 12) * mass * (width * width + height * height);
            body = new SPBody2D(position, density, mass, inertia, restiution, area, isStatic, 0, width, height, staticFriction, dynamticFriction, ShapeType2D.Box);
            return true;
        }
        public static bool CreatePolygonBody(List<Vector2[]> Vertices, float area, Vector2 position, float density, bool isStatic, float restiution, float staticFriction, float dynamticFriction,
out SPBody2D body, out string errormsg)
        {
            body = null;
            errormsg = string.Empty;
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
            var inertia = (1f / 12) * mass * area;
            body = new SPBody2D(position, density, mass, inertia, restiution, area, isStatic, 0, 1, 1, staticFriction, dynamticFriction, ShapeType2D.Box);
            body.Vertices = Vertices;
            body.transformVertices = new List<Vector2[]>();
            foreach (var i in body.Vertices) {
                body.transformVertices.Add(new Vector2[i.Length]);
            }
            return true;
        }
        public List<Vector2[]> GetTransformedVertices()
        {
            if (this.transformUpdateRequired)
            {
                for (int x=0;x<this.Vertices.Count;x++) {
                    var Vertices = this.Vertices[x];
                    for (int i = 0; i < Vertices.Length; i++)
                    {
                        Vector2 v = Vertices[i];
                        Matrix mat = Matrix.CreateTranslation(new Vector3(v.X, v.Y, 0)) * Matrix.CreateRotationZ(-rotation);
                        var t = mat.Translation;
                        v = new Vector2(t.X, t.Y) + position ;
                        this.transformVertices[x][i] = v;
                    }
                    this.transformUpdateRequired = false;
                }
            }
            return transformVertices;
        }

        public SAABB GetAABB()
        {
            return aabb;
        }
        public void UpdateAABB()
        {
            if (!aabbUpdateRequire)
            {
                return;
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
                var Vertices = GetTransformedVertices();
                for (int i = 0; i < Vertices.Count; i++)
                {
                    for (int j = 0; j < Vertices[i].Length;j++) {
                        Vector2 v = Vertices[i][j];
                        if (v.X < minX) { minX = v.X; }
                        if (v.Y < minY) { minY = v.Y; }
                        if (v.X > maxX) { maxX = v.X; }
                        if (v.Y > maxY) { maxY = v.Y; }
                    }
                }
            }
            else throw new Exception("UnKnown type");
            aabb = new SAABB(minX, minY, maxX, maxY);
            aabbUpdateRequire = false;
        }
        /// <summary>
        /// 此方法仅对ShapeType为Box的对象有效,将会返回一个储存索引,如果删除不按照添加顺序删除，则索引会乱序
        /// </summary>
        public int AddColVertices(Vector2[] Vertices) {
            if (ShapeType!=ShapeType2D.Box) {
                return -1;
            }
            this.Vertices.Add(Vertices);
            this.transformVertices.Add(new Vector2[Vertices.Length]);
            this.transformUpdateRequired = true;
            this.aabbUpdateRequire = true;
            return this.Vertices.Count - 1;
        }
        /// <summary>
        /// 此方法仅对ShapeType为Box的对象有效
        /// </summary>
        public void RemoveCalVertices(int index) {
            if (ShapeType != ShapeType2D.Box)
            {
                return;
            }
            this.Vertices.RemoveAt(index);
            this.transformVertices.RemoveAt(index);
            this.transformUpdateRequired = true;
            this.aabbUpdateRequire = true;
        }
        public void SetMass(float mass) {
            this.Mass = mass;
            this.InvMass = 1f / mass;
        }
        public void Step(float time, int Iterations)
        {
            LastAABB = aabb;
            UpdateAABB();
            if (!LastAABB.Equals(aabb))
            {
                SPWorld.UpdateBodyTree(this);
            }
            if (IsStatic || IsTrigger)
            {
                linearVelocity = Vector2.Zero;
                rotationalVelocity = 0;
                force = Vector2.Zero;
                return;
            }
            if (RotFreeze)
            {
                rotationalVelocity = 0;
            }
            time /= Iterations;
            Vector2 acceleration = this.force / this.Mass;
            this.linearVelocity += acceleration * time;
            if (VelocityFreeze)
            {
                linearVelocity = Vector2.Zero;
            }
            Move(linearVelocity * time);
            Rotate(rotationalVelocity * time);
            this.force = Vector2.Zero;
        }
        public void Move(Vector2 vec)
        {
            this.position += vec;
            this.transformUpdateRequired = true;
            aabbUpdateRequire = true;
        }
        public void MoveTo(Vector2 pos)
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
        public void AddForce(Vector2 force)
        {
            this.force += force;
        }
    }
}
