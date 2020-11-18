using System;
using System.Runtime.InteropServices;

namespace Win32Api
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }

        public POINT(double x, double y)
        {
            X = (int)x;
            Y = (int)y;
        }

        public double LengthAsVector()
        {
            return Math.Abs(Math.Sqrt(X * X + Y * Y));
        }

        public static POINT operator +(POINT p1, POINT p2)
        {
            return new POINT(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static POINT operator -(POINT p1, POINT p2)
        {
            return new POINT(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static POINT operator *(POINT p1, double factor)
        {
            return new POINT(p1.X * factor, p1.Y * factor);
        }

        public override int GetHashCode()
        {
            return X + (19 * Y);
        }

        public System.Windows.Point ToWindowsPoint()
        {
            return new System.Windows.Point(X, Y);
        }
        public override string ToString()
        {
            return X.ToString() + ":" + Y.ToString();
        }

        public bool Equals(POINT p)
        {
            return p.X == X && p.Y == Y;
        }
        public override bool Equals(object obj)
        {
            if (obj is POINT pOINT) {
                return Equals(pOINT);
            } else {
                return false;
            }
        }
    }
}