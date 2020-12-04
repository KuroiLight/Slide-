using ExtensionMethods;
using Screen_Drop_In;
using System;
using System.Collections.Generic;
using System.Drawing;
using Win32Api;

namespace SlideSharp
{
    public static class SlideFactory
    {
        public static Slide? SlideFromMouseMovement(POINT start, POINT end)
        {
            var screen = Screen.FromPoint(end.ToDrawingPoint());
            if (screen is null) return null;

            var diff = end - start;
            if (diff.LengthAsVector() < Config.GetInstance.MouseDragDeadzone) return new CenterSlide(screen);

            static System.Drawing.Point scaleVector(POINT _start, POINT _vector, double _factor) 
            {
                var temp = (_vector * _factor) + _start;
                return new Point(temp.X, temp.Y);
            }

            if (diff.X > 0 && IsValidSlideDirection(Direction.Left, screen) && screen.WorkingArea.HasPoint(scaleVector(start, diff, (double)(screen.Bounds.Right - start.X) / diff.X))) // right
            {
                return new RightSlide(screen);
            }
            else if (diff.X < 0 && IsValidSlideDirection(Direction.Right, screen) && screen.WorkingArea.HasPoint(scaleVector(start, diff, (double)(screen.Bounds.Left - start.X) / diff.X))) // left
            {
                return new LeftSlide(screen);
            }

            if (diff.Y < 0 && IsValidSlideDirection(Direction.Up, screen) && screen.WorkingArea.HasPoint(scaleVector(start, diff, (double)(screen.Bounds.Top - start.Y) / diff.Y))) // top
            {
                return new TopSlide(screen);
            }
            else if (diff.Y > 0 && IsValidSlideDirection(Direction.Down, screen) && screen.WorkingArea.HasPoint(scaleVector(start, diff, (double)(screen.Bounds.Bottom - start.Y) / diff.Y))) // bottom
            {
                return new BottomSlide(screen);
            }

            return new CenterSlide(screen);
        }

        private static bool IsValidSlideDirection(Direction dir, Screen screen)
        {
            static System.Drawing.Point PointOffset(System.Drawing.Point pt, int x, int y) => new(pt.X + x, pt.Y + y);

            System.Drawing.Point? outsidePt = (dir) switch
            {
                Direction.Up => PointOffset(screen.WorkingArea.Location, screen.Bounds.Width / 2, -100),
                Direction.Down => PointOffset(screen.WorkingArea.BottomLeft(), screen.Bounds.Width / 2, 100),
                Direction.Left => PointOffset(screen.WorkingArea.Location, -100, screen.Bounds.Height / 2),
                Direction.Right => PointOffset(screen.WorkingArea.TopRight(), 100, screen.Bounds.Height / 2),
                _ => null,
            };

            if (outsidePt is null) return false;
            return Screen.FromPoint(outsidePt.Value) == null;
        }
    }
}