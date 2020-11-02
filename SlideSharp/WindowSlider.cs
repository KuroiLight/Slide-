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

        private POINT TargetPosition { get; set; }
        public Direction Direction { get; }
        public RECT Screen { get; }
        public Status Status { get; private set; }
        public WindowObj Window { get; private set; }

        public WindowObj UnAssignWindow()
        {
            Window?.SetTopMost(Window.TopMost);
            var window = Window;
            Window = null;
            return window;
        }

        public void AssignWindow(WindowObj window)
        {
            Window = window;
            Window?.SetTopMost(true);
            AssignStatus(Status.Showing);
        }

        public void AssignWindow(IntPtr windowHandle)
        {
            AssignWindow(windowHandle != IntPtr.Zero ? new WindowObj(windowHandle) : null);
        }

        public void AssignStatus(Status status)
        {
            if (status != Status && Window != null) {
                Status = status;
                GenerateTargetPosition();
            }
        }

        private void GenerateTargetPosition()
        {
            int WindowOffSet = MainWindow.config.Window_Offscreen_Offset;
            int GetCenterY() => Screen.Center.Y - (Window.Rect.Height / 2);
            int GetCenterX() => Screen.Center.X - (Window.Rect.Width / 2);

            if (Window?.Exists() == true) {
                TargetPosition = (Direction, Status) switch
                {
                    (Direction.Center, _) => new POINT(GetCenterX(), GetCenterY()),
                    (Direction.Left, Status.Showing) => new POINT(Screen.Left, GetCenterY()),
                    (Direction.Left, Status.Hiding) => new POINT(Screen.Left - Window.Rect.Width + WindowOffSet, GetCenterY()),
                    (Direction.Right, Status.Showing) => new POINT(Screen.Right - Window.Rect.Width, GetCenterY()),
                    (Direction.Right, Status.Hiding) => new POINT(Screen.Right - WindowOffSet, GetCenterY()),
                    (Direction.Up, Status.Showing) => new POINT(GetCenterX(), Screen.Top),
                    (Direction.Up, Status.Hiding) => new POINT(GetCenterX(), Screen.Top - Window.Rect.Height + WindowOffSet),
                    (Direction.Down, Status.Showing) => new POINT(GetCenterX(), Screen.Bottom - Window.Rect.Height),
                    (Direction.Down, Status.Hiding) => new POINT(GetCenterX(), Screen.Bottom - WindowOffSet),
                    _ => throw new ArgumentOutOfRangeException("Direction, Status"),
                };
            }
        }

        public WindowSlider UpdatePosition()
        {
            if (Window?.Exists() == true) {
                if (TargetPosition != Window.Rect.ToPoint) {
                    Vector nextMove = new Vector(Window.Rect.ToPoint, TargetPosition);
                    Window.SetPosition(nextMove.Clamp(MainWindow.config.Window_Movement_Rate).ToPoint() + Window.Rect.ToPoint);
                    Window.UpdateRect();
                } else {
                    if (Direction == Direction.Center) {
                        Window.SetTopMost(Window.TopMost);
                        Window = null;
                    }
                }
            }
            return this;
        }

        public bool WillIntersect(Ray ray)
        {
            if (ray.Direction.Length() <= MainWindow.config.Middle_Button_DeadZone) {
                return false;
            }
            if (Direction != (ray.Direction.X > 0 ? Direction.Left : Direction.Right) && Direction != (ray.Direction.Y > 0 ? Direction.Up : Direction.Down)) {
                return false;
            }

            if (Direction == Direction.Center) {
                return false;
            }

            Ray scaledRay = (Direction) switch
            {
                Direction.Up => ray.Scale((Screen.Top - ray.Position.Y) / ray.Direction.Y),
                Direction.Down => ray.Scale((Screen.Bottom - ray.Position.Y) / ray.Direction.Y),
                Direction.Left => ray.Scale((Screen.Left - ray.Position.X) / ray.Direction.X),
                Direction.Right => ray.Scale((Screen.Right - ray.Position.X) / ray.Direction.X),
                _ => throw new ArgumentOutOfRangeException(nameof(Direction)),
            };

            return Screen.Contains(scaledRay.EndPoint());
        }

        public bool WillIntersect(POINT start, POINT end)
        {
            return WillIntersect(new Ray(start, end));
        }

        public static List<WindowSlider> GetAllValidInstances()
        {
            List<WindowSlider> NewList = new List<WindowSlider>();
            WpfScreenHelper.Screen.AllScreens.ToList().ForEach((S) => {
                foreach (Direction d in Enum.GetValues(typeof(Direction))) {
                    WindowSlider slider = GetValidInstance(d, S);
                    if (slider != null) {
                        NewList.Add(slider);
                    }
                }
            });
            return NewList;
        }

        public static WindowSlider GetValidInstance(Direction direction, Screen screen)
        {
            bool ScreenAtPoint(Point pt) => WpfScreenHelper.Screen.AllScreens.All((S) => S.Bounds.Contains(pt));
            var NewInstance = new WindowSlider(screen, direction);

            return (direction) switch
            {
                Direction.Center => NewInstance,
                Direction.Up when screen.WorkingArea.Top == screen.Bounds.Top =>
                    ScreenAtPoint(new Point(0, screen.Bounds.Top - 1)) ? null : NewInstance,
                Direction.Down when screen.WorkingArea.Bottom == screen.Bounds.Bottom =>
                    ScreenAtPoint(new Point(0, screen.Bounds.Bottom + 1)) ? null : NewInstance,
                Direction.Left when screen.WorkingArea.Left == screen.Bounds.Left =>
                    ScreenAtPoint(new Point(screen.Bounds.Left - 1, 0)) ? null : NewInstance,
                Direction.Right when screen.WorkingArea.Right == screen.Bounds.Right =>
                    ScreenAtPoint(new Point(screen.Bounds.Right + 1, 0)) ? null : NewInstance,
                _ => null,
            };
        }
    }
}