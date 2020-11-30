using System.Drawing;

namespace ExtensionMethods
{
    public static class RectangleMethods
    {
        public static Point TopLeft(this Rectangle rc)
        {
            return new Point(rc.X, rc.Y);
        }

        public static Point TopRight(this Rectangle rc)
        {
            return new Point(rc.X + rc.Width, rc.Y);
        }

        public static Point BottomLeft(this Rectangle rc)
        {
            return new Point(rc.X, rc.Y + rc.Height);
        }

        public static Point BottomRight(this Rectangle rc)
        {
            return new Point(rc.X + rc.Width, rc.Y + rc.Height);
        }
    }
}