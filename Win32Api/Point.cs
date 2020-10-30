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

        public POINT VectorTo(POINT targetPoint)
        {
            return this - targetPoint;
        }

        public double DistanceTo(POINT targetPoint)
        {
            return Math.Sqrt((X - targetPoint.X) ^ 2 + (Y - targetPoint.Y) ^ 2);
        }

        public double ClampedDistanceTo(POINT targetPoint, double maxDistance)
        {
            return Math.Clamp(DistanceTo(targetPoint), -1 * maxDistance, maxDistance);
        }

        public POINT ClampedVectorTo(POINT targetPoint, POINT maxVector)
        {
            return new POINT(Math.Clamp(X - targetPoint.X, -1 * maxVector.X, maxVector.X), Math.Clamp(Y - targetPoint.Y, -1 * maxVector.Y, maxVector.Y));
        }

        public POINT ClampedVectorTo(POINT targetPoint, int maxMove)
        {
            return new POINT(Math.Clamp(X - targetPoint.X, -1 * maxMove, maxMove), Math.Clamp(Y - targetPoint.Y, -1 * maxMove, maxMove));
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

        public static POINT operator /(POINT p1, int divisor)
        {
            return new POINT(p1.X / divisor, p1.Y / divisor);
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