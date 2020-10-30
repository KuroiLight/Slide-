using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Win32Api;
using WpfScreenHelper;

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

    public enum Status : int
    {
        Hiding = 1,
        Showing = -1
    }

    public class WindowSlider
    {
        public WindowSlider(Screen screen, Direction direction)
        {
            Screen = new RECT(screen.WorkingArea.Left, screen.WorkingArea.Top, screen.WorkingArea.Right, screen.WorkingArea.Bottom);
            Direction = direction;
        }

        public WindowObj Window { get; protected set; }
        private POINT TargetPosition { get; set; }
        public RECT Screen { get; }
        public Status Status { get; protected set; }
        public Direction Direction { get; }

        public WindowSlider SetWindow(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) {
                Window = null;
                TargetPosition = default;
            } else {
                Window = new WindowObj(windowHandle);
            }
            return this;
        }

        public WindowSlider SetState(Status S)
        {
            if (S != Status) {
                Status = S;
                GenerateTargetPosition();
            }
            return this;
        }

        private void GenerateTargetPosition()
        {
            const int WindowOffSet = 30;
            int GetCenterY() => Screen.Center.Y - (Window.WindowArea.Height / 2);
            int GetCenterX() => Screen.Center.X - (Window.WindowArea.Width / 2);

            if (Window?.Exists() == true) {
                TargetPosition = (Direction, Status) switch
                {
                    (Direction.Center, _) => new POINT(GetCenterX(), GetCenterY()),
                    (Direction.Left, Status.Showing) => new POINT(Screen.Left, GetCenterY()),
                    (Direction.Left, Status.Hiding) => new POINT(Screen.Left - Window.WindowArea.Width + WindowOffSet, GetCenterY()),
                    (Direction.Right, Status.Showing) => new POINT(Screen.Right - Window.WindowArea.Width, GetCenterY()),
                    (Direction.Right, Status.Hiding) => new POINT(Screen.Right - WindowOffSet, GetCenterY()),
                    (Direction.Up, Status.Showing) => new POINT(GetCenterX(), Screen.Top),
                    (Direction.Up, Status.Hiding) => new POINT(GetCenterX(), Screen.Top - Window.WindowArea.Height + WindowOffSet),
                    (Direction.Down, Status.Showing) => new POINT(GetCenterX(), Screen.Bottom - Window.WindowArea.Height),
                    (Direction.Down, Status.Hiding) => new POINT(GetCenterX(), Screen.Bottom - WindowOffSet),
                    _ => throw new ArgumentOutOfRangeException("Direction, Status"),
                };
            }
        }

        public WindowSlider UpdatePosition()
        {
            if (Window?.Exists() == true) {
                if (TargetPosition != Window.WindowArea.Center) {
                    Window.SetPosition(Window.WindowArea.ToPoint + Window.WindowArea.ToPoint.ClampedVectorTo(TargetPosition, 60));
                } else {
                    if (Direction == Direction.Center) {
                        Window = null;
                    }
                }
            }
            return this;
        }

        public bool WillIntersect(POINT start, POINT end)
        {
            Vector vec = new Vector(start.X - end.X, start.Y - end.Y);

            Direction possibleDirections = new Direction();
            if (vec.X != 0) {
                possibleDirections |= (vec.X > 0 ? Direction.Left : Direction.Right);
            }
            if (vec.Y != 0) {
                possibleDirections |= (vec.Y > 0 ? Direction.Up : Direction.Down);
            }
            if ((possibleDirections & Direction) != Direction) {
                return false;
            }

            Vector endVector = (Direction) switch
            {
                Direction.Up => Vector.Multiply((Screen.Top - start.Y) / vec.Y, vec),
                Direction.Down => Vector.Multiply((Screen.Bottom - start.Y) / vec.Y, vec),
                Direction.Left => Vector.Multiply((Screen.Left - start.X) / vec.X, vec),
                Direction.Right => Vector.Multiply((Screen.Right - start.X) / vec.X, vec),
                _ => default,
            };

            if (endVector == default) {
                return false;
            }

            var endPoint = new POINT(start.X + endVector.X, start.Y + endVector.Y);
            //Debug.WriteLine($"{Direction}: {endPoint} = {Screen.Contains(endPoint)}");
            return Screen.Contains(endPoint);
        }

        public static List<WindowSlider> GetAllValidInstances()
        {
            List<WindowSlider> NewList = new List<WindowSlider>();
            WpfScreenHelper.Screen.AllScreens.ToList().ForEach((S) => {
                foreach (Direction d in Enum.GetValues(typeof(Direction))) {
                    WindowSlider edge = GetValidInstance(d, S);
                    if (edge != null) {
                        NewList.Add(edge);
                    }
                }
            });
            return NewList;
        }

        public static WindowSlider GetValidInstance(Direction direction, Screen screen)
        {
            var NewInstance = new WindowSlider(screen, direction);
            return WpfScreenHelper.Screen.AllScreens.ToList().Exists((S) => {
                if (S != screen) {
                    return false;
                } else {
                    return S.Bounds.Contains(NewInstance.Screen.ToWindowsRectangle);
                }
            }) ? null : NewInstance;
        }
    }
}