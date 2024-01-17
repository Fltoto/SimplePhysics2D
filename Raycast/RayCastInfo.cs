using SimplePhysics2D.RigidBody;

namespace SimplePhysics2D.Raycast
{
    public readonly struct RayCastInfo
    {
        public readonly SPBody2D Hited;
        public readonly SPVector2 Point;
        public readonly float Distance;

        public RayCastInfo(SPBody2D body,SPVector2 point,float distance) {
            Hited = body;
            Point = point;
            Distance = distance;
        }
    }
}
