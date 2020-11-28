using Win32Api;

namespace SlideSharp
{
    public class Ray
    {
        public POINT Position { get; }
        public POINT Movement { get; }
        public Direction Direction { get; }

        public Ray(POINT position, POINT movement)
        {
            Position = position;
            Movement = movement;
            Direction = 0;

            Direction |= Movement.X < 0 ? Direction.Left : (Movement.X > 0 ? Direction.Right : 0);
            Direction |= Movement.X < 0 ? Direction.Up : (Movement.X > 0 ? Direction.Down : 0);
        }

        public POINT EndPoint()
        {
            return new POINT(Position.X + Movement.X, Position.Y + Movement.Y);
        }

        public POINT ScaledEndPoint(double factor)
        {
            return (Movement * factor) + Position;
        }
    }
}