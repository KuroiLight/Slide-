using System.Runtime.InteropServices;

namespace Win32Api
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct RECT : System.IEquatable<RECT>
    {
        public readonly int Left, Top, Right, Bottom;

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

        public POINT XY => new POINT(Left, Top);

        public override int GetHashCode()
        {
            return Left + (15 * Top) + (17 * Right) + (21 * Bottom);
        }

        public override string ToString()
        {
            return string.Concat("[{0}:{1}, {2}:{3}]", Left, Top, Right, Bottom);
        }

        public bool Equals(RECT other) => other.Left == Left && other.Top == Top && other.Right == Right && other.Bottom == Bottom;

        public override bool Equals(object? obj)
        {
            if (obj is RECT rECT)
            {
                return Equals(rECT);
            }

            return false;
        }

        public static bool operator ==(RECT left, RECT right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RECT left, RECT right)
        {
            return !(left == right);
        }
    }
}