using SimplePhysics2D.RigidBody;

namespace SimplePhysics2D.Raycast
{
    public readonly struct RayCastInfo
    {
        public readonly SPBody2D Hited;
        public readonly Vector2 Point;
        public readonly float Distance;

        public RayCastInfo(SPBody2D body, Vector2 point, float distance)
        {
            Hited = body;
            Point = point;
            Distance = distance;
        }
    }
}
