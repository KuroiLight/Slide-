using Win32Api;

namespace SlideSharp
{
    public struct Ray
    {
        public POINT Position { get; }
        public Vector Movement { get; }
        public Direction Direction { get; }


        public Ray(POINT start, POINT end)
        {
            Position = start;
            Movement = new Vector(start - end);
            Direction = 0;
            Direction |= Movement.X > 0 ? Direction.Left : Direction.Right;
            Direction |= Movement.Y > 0 ? Direction.Up : Direction.Down;
        }

        public Ray(POINT position, Vector movement)
        {
            Position = position;
            Movement = movement;
            Direction = 0;
            Direction |= Movement.X > 0 ? Direction.Left : Direction.Right;
            Direction |= Movement.Y > 0 ? Direction.Up : Direction.Down;
        }

        public POINT EndPoint()
        {
            return new POINT(Position.X + Movement.X, Position.Y + Movement.Y);
        }

        public Ray Scale(double factor)
        {
            return new Ray(Position, Movement.Multiply(factor));
        }
    }
}