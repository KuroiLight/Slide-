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
            if(windowHandle != Window?.GetHandle() || status != Status) {
                Window = windowHandle != IntPtr.Zero ? new WindowObj(windowHandle) : null;
                if(Window != null) {
                    ChangeStatus(status);
                    GenerateTargetPosition();
                }
            }

            return this;
        }

        private void ChangeStatus(Status status)
        {
            if(status == Status.Showing) {
                Window.SetTopMost(true);
            } else {
                Window.ResetTopMost();
            }
            Status = status;
        }

        private void GenerateTargetPosition()
        {
            const int WindowOffSet = 30;
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
            Debug.WriteLine($"{TargetPosition} {Status} {Direction}, {GetCenterX()} {GetCenterY()}");
        }

        public WindowSlider UpdatePosition()
        {
            if (Window?.Exists() == true) {
                if(TargetPosition != Window.Rect.ToPoint) {
                    Debug.WriteLine($"{Window.Rect.ToPoint} => {TargetPosition}");
                    Window.SetPosition(Window.Rect.ToPoint + Window.Rect.ToPoint.ClampedVectorTo(TargetPosition, 90));
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
            if(vec.Length < 25) {
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