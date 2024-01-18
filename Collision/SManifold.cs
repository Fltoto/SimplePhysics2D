using SimplePhysics2D.RigidBody;

namespace SimplePhysics2D.Collision
{
    public readonly struct SManifold
    {
        public readonly SPBody2D BodyA;
        public readonly SPBody2D BodyB;
        public readonly SPVector2 Normal;
        public readonly float Depth;
        public readonly SPVector2 Contact1;
        public readonly SPVector2 Contact2;
        public readonly int ContactCount;

        public SManifold(SPBody2D bodya, SPBody2D bodyb, SPVector2 normal, float depth, SPVector2 contact1, SPVector2 contact2, int contactCount)
        {
            BodyA = bodya;
            BodyB = bodyb;
            Normal = normal;
            Depth = depth;
            Contact1 = contact1;
            Contact2 = contact2;
            ContactCount = contactCount;
        }
    }
}
