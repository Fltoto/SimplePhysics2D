using SimplePhysics2D.BoudingBox;
using SimplePhysics2D.RigidBody;
using System.Collections.Generic;

namespace SimplePhysics2D.QuadTree
{
    /// <summary>
    /// 动态四叉树，通过划分2D空间来优化物理碰撞
    /// </summary>
    public class SpaceTree
    {
        /// <summary>
        /// 最小为32，再小会内存溢出
        /// </summary>
        public static int MaxContain = 32;
        public SAABB Area { get; }
        public int Depth { get; }
        public int MaxDepth { get; }

        public SpaceTree Parent { get; }
        private List<SpaceTree> Children = new List<SpaceTree>();
        private List<SPBody2D> Items = new List<SPBody2D>();

        public SpaceTree(SAABB Area, int Depth, int MaxDepth, SpaceTree Parent = null)
        {
            this.Area = Area;
            this.Depth = Depth;
            this.MaxDepth = MaxDepth;
            this.Parent = Parent;
        }
        private void AddChild(SpaceTree t)
        {
            lock (Children)
            {
                Children.Add(t);
            }
        }
        private void RemoveChild(SpaceTree t)
        {
            lock (Children)
            {
                Children.Remove(t);
            }
        }
        private void ClearChild()
        {
            lock (Children)
            {
                Children.Clear();
            }
        }
        private bool Splite()
        {
            if (Children.Count > 0)
            {
                return false;
            }
            if (Depth >= MaxDepth)
            {
                return false;
            }
            var width = Area.Width / 2f;
            var height = Area.Height / 2f;
            var nextDepth = Depth + 1;
            SAABB[] AABBS = new SAABB[4];

            AABBS[0] = new SAABB(Area.Min, Area.Min + new Vector2(width, height));
            AABBS[1] = new SAABB(Area.Min + new Vector2(width, 0), Area.Min + new Vector2(width * 2, height));
            AABBS[2] = new SAABB(Area.Min + new Vector2(width, height), Area.Min + new Vector2(width * 2, height * 2));
            AABBS[3] = new SAABB(Area.Min + new Vector2(0, height), Area.Min + new Vector2(width, height * 2));
            for (int i = 0; i < AABBS.Length; i++)
            {
                AddChild(new SpaceTree(AABBS[i], nextDepth, MaxDepth, this));
            }
            return true;
        }
        public void Add(SPBody2D body)
        {
            if (!Area.Insect(body.GetAABB()))
            {
                return;
            }
            if (Children.Count == 0)
            {
                lock (Items)
                {
                    if (Items.Contains(body))
                    {
                        return;
                    }
                    Items.Add(body);
                    if (Items.Count >= MaxContain)
                    {
                        Splite();
                        if (Children.Count > 0)
                        {
                            for (int i = 0; i < Items.Count; i++)
                            {
                                AddToChildren(Items[i]);
                            }
                            Items.Clear();
                        }
                    }
                }
            }
            else
            {
                AddToChildren(body);
            }
        }
        private void AddToChildren(SPBody2D body)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Add(body);
            }
        }
        public void Remove(SPBody2D body)
        {
            if (Area.Insect(body.LastAABB))
            {
                lock (Items)
                {
                    bool t = false;
                    if (Items.Count > 0)
                    {
                        t = Items.Remove(body);
                    }
                    if (t)
                    {
                        if (GetItemsCount() == 0)
                        {
                            ClearChild();
                        }
                        return;
                    }
                    for (int i = 0; i < Children.Count; i++)
                    {
                        Children[i].Remove(body);
                    }
                }
            }
        }
        public int GetItemsCount()
        {
            int c = 0;
            if (Children.Count > 0)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    c += Children[i].GetItemsCount();
                }
            }
            else if (Items != null)
            {
                c += Items.Count;
            }
            return c;
        }
        public void Update(SPBody2D body)
        {
            Remove(body);
            Add(body);
        }
        public void Query(SAABB AABB, List<SPBody2D> outs)
        {
            if (Area.Insect(AABB))
            {
                if (Children.Count > 0)
                {
                    for (int i = 0; i < Children.Count; i++)
                    {
                        Children[i].Query(AABB, outs);
                    }
                }
                else
                {
                    outs.AddRange(GetItems());
                }
            }
            else if (Parent != null)
            {
                if (Parent.Area.Insect(AABB))
                {
                    outs.AddRange(Parent.GetItems());
                }
            }
        }
        public void GetLastestTress(List<SpaceTree> outs)
        {
            if (Children.Count > 0)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].GetLastestTress(outs);
                }
            }
            else
            {
                if (Items.Count > 0)
                {
                    outs.Add(this);
                }
            }
        }
        public SPBody2D[] GetItems()
        {
            if (Items == null)
            {
                return new SPBody2D[0];
            }
            lock (Items)
            {
                return Items.ToArray();
            }
        }
    }
}
