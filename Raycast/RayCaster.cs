using SimplePhysics2D.Collision;
using SimplePhysics2D.QuadTree;
using SimplePhysics2D.RigidBody;
using System.Collections.Generic;

namespace SimplePhysics2D.Raycast
{
    public static class RayCaster
    {
        private static SpaceTree tree;

        public static void SetWorld(SpaceTree tree)
        {
            RayCaster.tree = tree;
        }
        public static bool Cast(SPVector2 Start, SPVector2 End, float Width, out RayCastInfo[] infos) {
            infos = new RayCastInfo[0];
            var ray = new BoxRay(Start, End, Width);
            List<RayCastInfo> ifs = new List<RayCastInfo>();
            if (BroadPhase(ray,out SPBody2D[] bodies)) {
                for (int i=0;i<bodies.Length;i++) {
                    if (NarrowPhase(ray, bodies[i],out RayCastInfo info)) {
                        ifs.Add(info);
                    }
                }
            }
            if (ifs.Count>0) {
                infos = ifs.ToArray();
                return true;
            }
            return false;
        }
        private static bool BroadPhase(BoxRay ray, out SPBody2D[] bodies)
        {
            bodies = new SPBody2D[0];
            List<SpaceTree> spaces = new List<SpaceTree>();
            tree.GetLastestTress(spaces);
            List<SPBody2D> bs = new List<SPBody2D>();
            ray.TempBody.UpdateAABB();
            for (int x = 0; x < spaces.Count; x++)
            {
                var b = spaces[x].GetItems();
                for (int i = 0; i < b.Length; i++)
                {
                    if (Collisions.IntersectAABBs(b[i].GetAABB(), ray.TempBody.GetAABB()))
                    {
                        bs.Add(b[i]);
                    }
                }
            }
            if (bs.Count > 0)
            {
                bodies = bs.ToArray();
                return true;
            }
            return false;
        }
        private static bool NarrowPhase(BoxRay ray, SPBody2D body, out RayCastInfo info)
        {
            info = new RayCastInfo();
            var point = new SPVector2();
            if (Collisions.Collide(ray.TempBody,body,out _,out _))
            {
                Collisions.FindContacts(ray.TempBody,body,out SPVector2 p1,out SPVector2 p2,out int count);
                if (count == 2)
                {
                    point = p2;
                }
                else {
                    point = p1;
                }
                info = new RayCastInfo(body,point,SPMath2D.Distance(ray.Start,point));
                return true;
            }
            return false;
        }
    }
}
