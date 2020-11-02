using System;
using Win32Api;

namespace SlideSharp
{
    public struct Vector
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public Vector(POINT pt)
        {
            X = pt.X;
            Y = pt.Y;
        }

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector((double x, double y) tuple)
        {
            (X, Y) = tuple;
        }

        public Vector(Vector v)
        {
            X = v.X;
            Y = v.Y;
        }

        public Vector(POINT p1, POINT p2)
        {
            POINT diff = p2 - p1;
            X = diff.X;
            Y = diff.Y;
        }

        public Vector Clamp(int absoluteMinMax)
        {
            int absoluteMinMin = absoluteMinMax * -1;
            return new Vector(this)
            {
                X = Math.Clamp(X, absoluteMinMin, absoluteMinMax),
                Y = Math.Clamp(Y, absoluteMinMin, absoluteMinMax)
            };
        }

        public Vector Multiply(double factor)
        {
            return new Vector(X * factor, Y * factor);
        }

        public double Length()
        {
            return Math.Abs(Math.Sqrt((X * X) + (Y * Y)));
        }

        public POINT ToPoint()
        {
            return new POINT(X, Y);
        }

        public override string ToString()
        {
            return $"/{X}:{Y}/";
        }
    }
}