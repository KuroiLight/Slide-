using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public WindowSlider Assign(IntPtr windowHandle)
        {
            return Assign(windowHandle, Status.Hiding);
        }

        public WindowSlider Assign(Status status)
        {
            return Assign(Window.GetHandle(), status);
        }

        public WindowSlider Assign(IntPtr windowHandle, Status status)
        {
            if(windowHandle == IntPtr.Zero && Window != null) {
                Window.SetTopMost(Window.TopMost);
            }

            if(Window?.GetHandle() != windowHandle) {
                Window = windowHandle != IntPtr.Zero ? new WindowObj(windowHandle) : null;
            }

            if(status != Status && Window != null) {
                Status = status;
                GenerateTargetPosition();
            }

            return this;
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
                if(TargetPosition != Window.Rect.ToPoint) {
                    Window.SetPosition(Window.Rect.ToPoint + Window.Rect.ToPoint.ClampedVectorTo(TargetPosition, new POINT(MainWindow.config.Window_Movement_Rate, MainWindow.config.Window_Movement_Rate)));
                } else {
                    if (Direction == Direction.Center) {
                        Window.SetTopMost(Window.TopMost);
                        Window = null;
                    }
                }
            }
            return this;
        }

        public bool WillIntersect(POINT start, POINT end)
        {
            Vector vec = new Vector(start.X - end.X, start.Y - end.Y);
            if(vec.Length < MainWindow.config.Middle_Button_DeadZone) {
                return false;
            }
            
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
            return Screen.Contains(endPoint);
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

            if (direction == Direction.Up && screen.WorkingArea.Top != screen.Bounds.Top) {
                return ScreenAtPoint(new Point(0, screen.Bounds.Top - 1)) ? null : NewInstance;
            } else if (direction == Direction.Down && screen.WorkingArea.Bottom != screen.Bounds.Bottom) {
                return ScreenAtPoint(new Point(0, screen.Bounds.Bottom + 1)) ? null : NewInstance;
            } else if (direction == Direction.Left && screen.WorkingArea.Left != screen.Bounds.Left) {
                return ScreenAtPoint(new Point(screen.Bounds.Left - 1, 0)) ? null : NewInstance;
            } else if (direction == Direction.Right && screen.WorkingArea.Right != screen.Bounds.Right) {
                return ScreenAtPoint(new Point(screen.Bounds.Right + 1, 0)) ? null : NewInstance;
            }

            return WpfScreenHelper.Screen.AllScreens.Contains(screen) ? null : NewInstance;
        }
    }
}