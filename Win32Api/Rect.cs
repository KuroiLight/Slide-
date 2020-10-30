using System.Runtime.InteropServices;

namespace Win32Api
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
        public POINT Center => new POINT(Width / 2, Height / 2);

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public RECT(double left, double top, double right, double bottom)
        {
            (Left, Top, Right, Bottom) = ((int)left, (int)top, (int)right, (int)bottom);
        }

        public static RECT FromRectangle(System.Drawing.Rectangle R)
        {
            return new RECT(R.Left, R.Top, R.Right, R.Bottom);
        }

        public POINT ToPoint()
        {
            return new POINT(Left, Top);
        }

        public bool Contains(POINT pt)
        {
            return Left <= pt.X && Right >= pt.X && Top <= pt.Y && Bottom >= pt.Y;
        }

        public static bool operator ==(RECT r1, RECT r2)
        {
            return r1.Equals(r2);
        }

        public static bool operator !=(RECT r1, RECT r2)
        {
            return !r1.Equals(r2);
        }

        public bool Equals(RECT r)
        {
            return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
        }

        public override int GetHashCode()
        {
            return Left + (15 * Top) + (17 * Right) + (21 * Bottom);
        }

        public override string ToString()
        {
            return $"[*{Left}:{Top}, *{Right}:{Bottom}]";
        }

        public override bool Equals(object obj)
        {
            if (obj is RECT rECT) {
                return Equals(rECT);
            } else {
                return false;
            }
        }

        public System.Windows.Rect ToWindowsRect()
        {
            return new System.Windows.Rect(Left, Top, Width, Height);
        }
    }
}