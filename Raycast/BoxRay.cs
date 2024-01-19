using SimplePhysics2D.RigidBody;

namespace SimplePhysics2D.Raycast
{
    internal readonly struct BoxRay
    {
        public readonly Vector2 Start;
        public readonly Vector2 End;
        public readonly float Width;
        public readonly SPBody2D TempBody;

        public BoxRay(Vector2 Start, Vector2 End, float Width)
        {
            this.Start = Start;
            this.End = End;
            this.Width = Width;
            SPBody2D.CreateBoxBody(Width, SPMath2D.Distance(Start, End), (Start + End) / 2, 0.5f, true, 0.5f, 0.5f, 0.5f, out SPBody2D body, out string erro);
            body.Rotate(-SPMath2D.Angle(new Vector2(0, 1), (End - Start)));
            this.TempBody = body;
        }
    }
}
