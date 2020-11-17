using System;
using System.Collections.Generic;
using System.Linq;
using Win32Api;
using WpfScreenHelper;

namespace SlideSharp
{
    public static class SlideFactory
    {
        public static IEnumerable<Enum> GetUniqueFlags(this Enum flags)
        {
            ulong flag = 1;
            foreach (var value in Enum.GetValues(flags.GetType()).Cast<Enum>()) {
                ulong bits = Convert.ToUInt64(value);
                while (flag < bits) {
                    flag <<= 1;
                }

                if (flag == bits && flags.HasFlag(value)) {
                    yield return value;
                }
            }
        }
        public static Slide SlideFromRay(Ray ray)
        {
            Screen screen = Screen.FromPoint(ray.EndPoint().ToWindowsPoint());
            if (ray.Movement.LengthAsVector() < Configuration.Config.MMDRAG_DEADZONE) return new CenterSlide(screen);

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
            static System.Windows.Point PointOffset(System.Windows.Point pt, int x, int y) => new System.Windows.Point(pt.X + x, pt.Y + y);

            return (dir) switch
            {
                Direction.Center => false,
                Direction.Up => Screen.FromPoint(PointOffset(screen.WorkingArea.TopLeft, 0, -1)) == null,
                Direction.Down => Screen.FromPoint(PointOffset(screen.WorkingArea.BottomLeft, 0, 1)) == null,
                Direction.Left => Screen.FromPoint(PointOffset(screen.WorkingArea.TopLeft, -1, 0)) == null,
                Direction.Right => Screen.FromPoint(PointOffset(screen.WorkingArea.BottomLeft, 1, 0)) == null,
                _ => false
            };
        }

        private static Direction GetActualDirection(Ray ray, Screen screen)
        {
            foreach (Direction flag in GetUniqueFlags(ray.Direction)) {
                POINT endPoint = flag switch
                {
                    Direction.Up => ray.ScaledEndPoint((screen.Bounds.Top - ray.Position.Y) / ray.Movement.Y),
                    Direction.Down => ray.ScaledEndPoint((screen.Bounds.Bottom - ray.Position.Y) / ray.Movement.Y),
                    Direction.Left => ray.ScaledEndPoint((screen.Bounds.Left - ray.Position.X) / ray.Movement.X),
                    Direction.Right => ray.ScaledEndPoint((screen.Bounds.Right - ray.Position.X) / ray.Movement.X),
                    _ => throw new InvalidOperationException(),
                };

                if (screen.Bounds.Contains(endPoint.ToWindowsPoint())) {
                    return flag;
                }
            }
            return Direction.Center;
        }
    }
}
