using System.Drawing;

namespace ExtensionMethods
{
    public static class RectangleMethods
    {
        public static Point TopRight(this Rectangle rc)
        {
            return new Point(rc.X + rc.Width, rc.Y);
        }

        public static Point BottomLeft(this Rectangle rc)
        {
            return new Point(rc.X, rc.Y + rc.Height);
        }

        /// <summary>
        /// Similar to Contains, but in addition will return true if point is on rectangles edge
        /// </summary>
        /// <param name="rc">rectangle being tested</param>
        /// <param name="pt">point to look for</param>
        /// <returns>true if point is on or inside of the rectangle, otherwise false</returns>
        public static bool HasPoint(this Rectangle rc, Point pt)
        {
            return rc.X <= pt.X && rc.Right >= pt.X && rc.Y <= pt.Y && rc.Bottom >= pt.Y;
        }
    }
}