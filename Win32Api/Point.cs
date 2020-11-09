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

        public POINT Clamp(int clampAmount)
        {
            return new POINT(Math.Clamp(X, clampAmount * -1, clampAmount), Math.Clamp(Y, clampAmount * -1, clampAmount));
        }

        public double DistanceTo(POINT targetPoint)
        {
            return Math.Sqrt((X - targetPoint.X) ^ 2 + (Y - targetPoint.Y) ^ 2);
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

        public static bool operator ==(POINT p1, POINT p2)
        {
            return p1.Equals(p2);
        }

        public static POINT operator /(POINT p1, double divisor)
        {
            return new POINT(p1.X / divisor, p1.Y / divisor);
        }

        public static POINT operator *(POINT p1, double factor)
        {
            return new POINT(p1.X * factor, p1.Y * factor);
        }

        public static bool operator !=(POINT p1, POINT p2)
        {
            return !p1.Equals(p2);
        }

        public bool Equals(POINT p)
        {
            return p.X == X && p.Y == Y;
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
            return $"*{X}:{Y}";
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