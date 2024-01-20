using SkiaSharp;
using System.Collections.Generic;

namespace SimplePhysics2D
{
    /// <summary>
    /// 16像素为单位一，不建议使用此方法，误差极大
    /// </summary>
    public static class PicModelCreater
    {
        public static Vector2[] CalculateCollision(SKBitmap pic, int Scale = 1)
        {
            List<Vector2> vers = new List<Vector2>();
            int width = pic.Width;
            int height = pic.Height;
            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    if (x > width || y > height)
                    {
                        continue;
                    }
                    if (pic.GetPixel(x, y).Alpha > 0)
                    {
                        vers.Add(new Vector2(x, y));
                    }
                }
            }
            var a = vers.ToArray();
            var center = new Vector2(width / 2f, height / 2f);
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = (a[i] - center) / 16f * Scale;
            }
            return a;
        }
    }
}
