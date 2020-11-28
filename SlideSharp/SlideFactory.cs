using ExtensionMethods;
using Screen_Drop_In;
using System;
using System.Collections.Generic;
using Win32Api;

namespace SlideSharp
{
    public static class SlideFactory
    {
        private static Direction[] GetDirections(Direction flag)
        {
            var AllValues = Enum.GetNames(typeof(Direction));
            List<Direction> ds = new();

            foreach (var item in AllValues) {
                var parsed = Enum.Parse<Direction>(item);
                if ((flag & parsed) != 0) ds.Add(parsed);
            }

            return ds.ToArray();
        }

        public static Slide SlideFromRay(Ray ray)
        {
            Screen? screen = Screen.FromPoint(ray.EndPoint().ToDrawingPoint());
            if (screen is null) return new CenterSlide(Screen.PrimaryScreen);
            if (ray.Movement.LengthAsVector() < Config.GetInstance.MouseDragDeadzone) return new CenterSlide(screen);

            return (GetActualDirection(ray, screen)) switch
            {
                Direction.Up when IsValidSlideDirection(Direction.Up, screen) => new TopSlide(screen),
                Direction.Down when IsValidSlideDirection(Direction.Down, screen) => new BottomSlide(screen),
                Direction.Left when IsValidSlideDirection(Direction.Left, screen) => new LeftSlide(screen),
                Direction.Right when IsValidSlideDirection(Direction.Right, screen) => new RightSlide(screen),
                _ => new CenterSlide(screen)
            };
        }

        private static bool IsValidSlideDirection(Direction dir, Screen screen)
        {
            static System.Drawing.Point PointOffset(System.Drawing.Point pt, int x, int y) => new(pt.X + x, pt.Y + y);

            System.Drawing.Point outsidePt = (dir) switch
            {
                Direction.Up => PointOffset(screen.WorkingArea.TopLeft(), screen.Bounds.Width / 2, -100),
                Direction.Down => PointOffset(screen.WorkingArea.BottomLeft(), screen.Bounds.Width / 2, 100),
                Direction.Left => PointOffset(screen.WorkingArea.TopLeft(), -100, screen.Bounds.Height / 2),
                Direction.Right => PointOffset(screen.WorkingArea.TopRight(), 100, screen.Bounds.Height / 2),
                _ => default,
            };

            foreach (var scr in Screen.AllScreens) {
                if (scr.WorkingArea.Contains(outsidePt)) return false;
            }
            return true;
        }

        private static Direction GetActualDirection(Ray ray, Screen screen)
        {
            foreach (Direction flag in GetDirections(ray.Direction)) {
                POINT? endPoint = flag switch
                {
                    Direction.Up => ray.ScaledEndPoint((screen.Bounds.Top - ray.Position.Y) / ray.Movement.Y),
                    Direction.Down => ray.ScaledEndPoint((screen.Bounds.Bottom - ray.Position.Y) / ray.Movement.Y),
                    Direction.Left => ray.ScaledEndPoint((screen.Bounds.Left - ray.Position.X) / ray.Movement.X),
                    Direction.Right => ray.ScaledEndPoint((screen.Bounds.Right - ray.Position.X) / ray.Movement.X),
                    _ => null
                };

                if (endPoint.HasValue && screen.Bounds.Contains(endPoint.Value.ToDrawingPoint())) {
                    return flag;
                }
            }
            return Direction.Center;
        }
    }
}
