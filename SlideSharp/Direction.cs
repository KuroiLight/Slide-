using System;

namespace SlideSharp
{
    [Flags]
    public enum Direction
    {
        Center = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }
}