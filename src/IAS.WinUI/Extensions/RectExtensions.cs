using System;
using Windows.Foundation;

namespace IAS.WinUI.Extensions
{
    public static class RectExtensions
    {
        public static bool Intersects(this Rect r1, Rect r2)
        {
            return !(r1.Right <= r2.Left ||  // r1 is entirely to the left of r2
                 r1.Left >= r2.Right ||  // r1 is entirely to the right of r2
                 r1.Bottom <= r2.Top ||  // r1 is entirely above r2
                 r1.Top >= r2.Bottom);   // r1 is entirely below r2
        }
        public static Rect CombineWith(this Rect rect1, Rect rect2)
        {
            double x = Math.Min(rect1.X, rect2.X);
            double y = Math.Min(rect1.Y, rect2.Y);
            double right = Math.Max(rect1.X + rect1.Width, rect2.X + rect2.Width);
            double bottom = Math.Max(rect1.Y + rect1.Height, rect2.Y + rect2.Height);

            return new Rect(x, y, right - x, bottom - y);
        }
    }
}
