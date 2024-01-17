using SkiaSharp;
using System.Collections.Generic;

namespace SimplePhysics2D
{
    /// <summary>
    /// 16像素为单位一，不建议使用此方法，误差极大
    /// </summary>
    public static class PicModelCreater
    {        
        public static SPVector2[] CalculateCollision(SKBitmap pic,int ScanStep=2) {
            List<SPVector2> vers = new List<SPVector2>();
            int width = pic.Width;
            int height = pic.Height;
            if (ScanStep<=0) {
                ScanStep = 1;
            }
            for (int x=0;x<=width;x+=ScanStep) {
                for (int y=0; y<=height;y+=ScanStep) {
                    if (x>width || y>height) {
                        continue;
                    }
                    if (pic.GetPixel(x,y).Alpha>0) {
                        vers.Add(new SPVector2(x,y));
                    }
                }
            }
            var a = vers.ToArray();
            var center = new SPVector2(width/2f,height/2f);
            for (int i=0;i<a.Length;i++) {
                a[i] = (a[i] - center)/16f;
            }
            return a;
        }
    }
}
