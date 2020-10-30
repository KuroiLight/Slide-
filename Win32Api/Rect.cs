using System.Runtime.InteropServices;

namespace Win32Api
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT : System.IEquatable<RECT>
    {
        public int Left, Top, Right, Bottom;

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

        public int Width => Right - Left;

        public int Height => Bottom - Top;

        public POINT Center => new POINT(Left + (Width / 2), Top + (Height / 2));

        public bool Contains(POINT pt) => Left <= pt.X && Right >= pt.X && Top <= pt.Y && Bottom >= pt.Y;

        public bool Equals(RECT r) => r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;

        public System.Windows.Rect ToWindowsRectangle => new System.Windows.Rect(Left, Top, Width, Height);

        public System.Drawing.Rectangle ToDrawingRectangle => new System.Drawing.Rectangle(Left, Top, Width, Height);

        public POINT ToPoint => new POINT(Left, Top);

        public static RECT FromDrawingRectangle(System.Drawing.Rectangle R) => new RECT(R.Left, R.Top, R.Right, R.Bottom);

        public static RECT FromWindowsRectangle(System.Windows.Rect R) => new RECT(R.Left, R.Top, R.Right, R.Bottom);

        public static bool operator ==(RECT r1, RECT r2) => r1.Equals(r2);

        public static bool operator !=(RECT r1, RECT r2) => !r1.Equals(r2);

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
    }
}