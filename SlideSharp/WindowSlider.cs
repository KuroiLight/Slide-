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
        Center = 16,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }


    public enum Status
    {
        Hiding = 1,
        Showing = -1
    }

    public class SlidingWindow
    {
        private readonly Direction _hideDirection;
        private readonly RECT _screen;
        private Status _status;

        private IntPtr _windowHandle;
        private RECT _windowRect;
        private POINT _windowShown, _windowHidden;

        private SlidingWindow(Screen screen, Direction hideDirection)
        {
            MarkedForDeletion = false;
            _hideDirection = hideDirection;
            _status = Status.Hiding;
            _screen = new RECT(screen.WorkingArea.Left, screen.WorkingArea.Top, screen.WorkingArea.Right,
                screen.WorkingArea.Bottom);
            _windowHandle = IntPtr.Zero;
            _windowRect = new RECT();
            _windowHidden = new POINT();
            _windowShown = new POINT();
        }

        public bool MarkedForDeletion { get; private set; }

        public static SlidingWindow CreateInstance(Screen screen, Direction hideDirection)
        {
            return new SlidingWindow(screen, hideDirection);
        }

        public static SlidingWindow CreateFromRay(Ray ray)
        {
            static IEnumerable<Enum> GetFlags(Enum e)
            {
                return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
            }

            var screen = Screen.FromPoint(new Point(ray.EndPoint().X, ray.EndPoint().Y));
            if (ray.Movement.Length() <= MainWindow.config.Middle_Button_DeadZone)
                return CreateInstance(screen, Direction.Center);

            foreach (var flag in GetFlags(ray.Direction))
            {
                var scaledRay = flag switch
                {
                    Direction.Up => ray.Scale((screen.Bounds.Top - ray.Position.Y) / ray.Movement.Y),
                    Direction.Down => ray.Scale((screen.Bounds.Bottom - ray.Position.Y) / ray.Movement.Y),
                    Direction.Left => ray.Scale((screen.Bounds.Left - ray.Position.X) / ray.Movement.X),
                    Direction.Right => ray.Scale((screen.Bounds.Right - ray.Position.X) / ray.Movement.X),
                    _ => throw new ArgumentOutOfRangeException(nameof(flag))
                };

                if (screen.Bounds.Contains(new Point(scaledRay.EndPoint().X, scaledRay.EndPoint().Y)))
                    return CreateInstance(screen, (Direction) flag);
            }

            return CreateInstance(screen, Direction.Center);
        }

        public void ManageWindow(IntPtr handle)
        {
            _windowHandle = handle;
            if (handle != IntPtr.Zero)
            {
                User32.SetWindowPos(_windowHandle, Imports.HWND_INSERTAFTER.HWND_TOPMOST);
                PrecalculateWindowPlacements();
            }
        }

        private void PrecalculateWindowPlacements()
        {
            var WindowOffSet = MainWindow.config.Window_Offscreen_Offset;
            _windowRect = User32.GetWindowRect(_windowHandle);
            var tmpThis = this;

            int GetCenterY()
            {
                return tmpThis._screen.Center.Y - tmpThis._windowRect.Height / 2;
            }

            int GetCenterX()
            {
                return tmpThis._screen.Center.X - tmpThis._windowRect.Width / 2;
            }

            switch (_hideDirection)
            {
                case Direction.Center:
                    _windowHidden = _windowShown = new POINT(GetCenterX(), GetCenterY());
                    break;
                case Direction.Up:
                    _windowHidden = new POINT(GetCenterX(), _screen.Top - _windowRect.Height + WindowOffSet);
                    _windowShown = new POINT(GetCenterX(), _screen.Top);
                    break;
                case Direction.Down:
                    _windowHidden = new POINT(GetCenterX(), _screen.Bottom - WindowOffSet);
                    _windowShown = new POINT(GetCenterX(), _screen.Bottom - _windowRect.Height);
                    break;
                case Direction.Left:
                    _windowHidden = new POINT(_screen.Left - _windowRect.Width + WindowOffSet, GetCenterY());
                    _windowShown = new POINT(_screen.Left, GetCenterY());
                    break;
                case Direction.Right:
                    _windowHidden = new POINT(_screen.Right - WindowOffSet, GetCenterY());
                    _windowShown = new POINT(_screen.Right - _windowRect.Width, GetCenterY());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void MoveNextStep()
        {
            if (_windowHandle != IntPtr.Zero && User32.IsWindow(_windowHandle))
            {
                if (WindowSizeChanged()) PrecalculateWindowPlacements();

                _windowRect = User32.GetWindowRect(_windowHandle);
                var nextPosition = _status == Status.Hiding ? _windowHidden : _windowShown;
                if (_windowRect.ToPoint != nextPosition)
                {
                    var v = new Vector(_windowRect.ToPoint, nextPosition);
                    User32.SetWindowPos(_windowHandle,
                        _windowRect.ToPoint + v.Clamp(MainWindow.config.Window_Movement_Rate).ToPoint());
                }
                else if (_hideDirection == Direction.Center)
                {
                    MarkedForDeletion = true;
                }
            }
            else
            {
                MarkedForDeletion = true;
            }
        }

        public bool RayIntersect(Ray ray)
        {
            if (ray.Movement.Length() <= MainWindow.config.Middle_Button_DeadZone) return false;
            if (ray.Direction != _hideDirection || _hideDirection == Direction.Center) return false;

            var scaledRay = _hideDirection switch
            {
                Direction.Up => ray.Scale((_screen.Top - ray.Position.Y) / ray.Movement.Y),
                Direction.Down => ray.Scale((_screen.Bottom - ray.Position.Y) / ray.Movement.Y),
                Direction.Left => ray.Scale((_screen.Left - ray.Position.X) / ray.Movement.X),
                Direction.Right => ray.Scale((_screen.Right - ray.Position.X) / ray.Movement.X),
                _ => throw new ArgumentOutOfRangeException(nameof(Direction))
            };

            return _screen.Contains(scaledRay.EndPoint());
        }

        private bool WindowSizeChanged()
        {
            var rect = User32.GetWindowRect(_windowHandle);
            return rect.Width != _windowRect.Width || rect.Height != _windowRect.Height;
        }

        public void SetWindowState(Status status)
        {
            _status = status;
        }

        public bool HasWindow()
        {
            return _windowHandle != IntPtr.Zero;
        }

        public bool HasWindow(IntPtr handle)
        {
            return _windowHandle == handle;
        }

        public void MarkForDeletion()
        {
            MarkedForDeletion = true;
        }
    }
}