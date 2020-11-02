using Win32Api;

namespace SlideSharp
{
    public struct Ray
    {
        public POINT Position { get; }
        public Vector Direction { get; }

        public Ray(POINT start, POINT end)
        {
            Position = start;
            Direction = new Vector(start - end);
        }

        public Ray(POINT position, Vector direction)
        {
            Position = position;
            Direction = direction;
        }

        public POINT EndPoint()
        {
            return new POINT(Position.X + Direction.X, Position.Y + Direction.Y);
        }

        public Ray Scale(double factor)
        {
            return new Ray(Position, Direction.Multiply(factor));
        }
    }
}