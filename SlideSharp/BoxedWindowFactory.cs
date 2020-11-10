using System;
using System.Collections.Generic;
using System.Linq;
using Win32Api;
using WpfScreenHelper;

namespace SlideSharp
{
    public class BoxedWindowFactory
    {
        private static IEnumerable<Enum> GetFlags(Enum e)
        {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
        }
        public static Slide SlideFromRay(Ray ray)
        {
            Screen screen = Screen.FromPoint(ray.EndPoint().ToWindowsPoint());
            if (ray.Movement.LengthAsVector() < Configuration.SettingsInstance.Middle_Button_DeadZone) return new CenterSlide(screen);

            return (GetActualDirection(ray, screen)) switch
            {
                Direction.Up => new TopSlide(screen),
                Direction.Down => new BottomSlide(screen),
                Direction.Left => new LeftSlide(screen),
                Direction.Right => new RightSlide(screen),
                _ => new CenterSlide(screen),
            };
        }

        private static Direction GetActualDirection(Ray ray, Screen screen)
        {
            foreach (var flag in GetFlags(ray.Direction)) {
                POINT endPoint = flag switch
                {
                    Direction.Up => ray.ScaledEndPoint((screen.Bounds.Top - ray.Position.Y) / ray.Movement.Y),
                    Direction.Down => ray.ScaledEndPoint((screen.Bounds.Bottom - ray.Position.Y) / ray.Movement.Y),
                    Direction.Left => ray.ScaledEndPoint((screen.Bounds.Left - ray.Position.X) / ray.Movement.X),
                    Direction.Right => ray.ScaledEndPoint((screen.Bounds.Right - ray.Position.X) / ray.Movement.X),
                    _ => throw new InvalidOperationException(),
                };

                if (screen.Bounds.Contains(endPoint.ToWindowsPoint())) {
                    return (Direction)flag;
                }
            }
            return Direction.Center;
        }
    }
}
