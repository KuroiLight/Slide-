using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using HWND = System.IntPtr;

namespace WindowShift
{
    public enum AnchorStatus
    {
        Empty = 0,
        Offscreen = 1,
        OnScreen = 2
    }

    public class AnchorPoint
    {
        private HWND _WindowHandle;
        public HWND WindowHandle
        {
            get => _WindowHandle != HWND.Zero && !Api.IsWindow(_WindowHandle) ? HWND.Zero : _WindowHandle;
            set {
                _WindowHandle = value;
                if (_WindowHandle == HWND.Zero) {
                    State = AnchorStatus.Empty;
                } else {
                    State = AnchorStatus.Offscreen;
                }
            }
        }

        private AnchorStatus _State;
        public AnchorStatus State
        {
            get => WindowHandle == HWND.Zero ? AnchorStatus.Empty : _State;
            set {
                if (_State != value) {
                    _State = value;
                    UpdatePosition();
                    TickEventEnabled = true;
                }
            }
        }

        private bool tickEventEnabled;
        private bool TickEventEnabled {
            get => tickEventEnabled;
            set {
                if(tickEventEnabled != value) {
                    if(value == false) {
                        Main.SingletonInstance.TaskScheduler.Tick -= UpdateTick;
                    } else {
                        Main.SingletonInstance.TaskScheduler.Tick += UpdateTick;
                    }
                    tickEventEnabled = value;
                }
                
            }
        }

        public POINT AnchorPt { get; private set; }
        public DragDirection Direction { get; private set; }

        public RECT MonitorArea { get; private set; }
        private POINT NextPosition;
        private int MonitorMaxStepX = 10, MonitorMaxStepY = 10;
        private Settings Config = Settings.SettingsInstance;

        public AnchorPoint(DragDirection direction, Screen monitor)
        {
            Direction = direction;
            MonitorArea = RECT.FromRectangle(monitor.Bounds);

            AnchorPt = direction switch
            {
                DragDirection.Left => new POINT(MonitorArea.Left + 1, MonitorArea.Height / 2),
                DragDirection.Right => new POINT(MonitorArea.Right - 1, MonitorArea.Height / 2),
                DragDirection.Up => new POINT(MonitorArea.Width / 2, MonitorArea.Top + 1),
                DragDirection.Down => new POINT(MonitorArea.Width / 2, MonitorArea.Bottom - 1),
                DragDirection.None => new POINT(MonitorArea.Width / 2, MonitorArea.Height / 2),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        ~AnchorPoint()
        {
            WindowHandle = HWND.Zero;
        }

        public void UpdateTick(object sender, EventArgs e)
        {
            Debug.WriteLine("Tick for " + ToString());
            if (State == AnchorStatus.Empty) {
                TickEventEnabled = false;
                return;
            }

            RECT winRect = Api.Wrapd_GetWindowRect(WindowHandle);
            var nextPoint = new POINT(
                winRect.Left + Math.Clamp(NextPosition.X - winRect.Left, -1 * MonitorMaxStepX, MonitorMaxStepX),
                winRect.Top + Math.Clamp(NextPosition.Y - winRect.Top, -1 * MonitorMaxStepY, MonitorMaxStepY)
                );

            if(winRect.ToPoint() == nextPoint) {
                TickEventEnabled = false;
                if(Direction == DragDirection.None) {
                    WindowHandle = HWND.Zero;
                }
                return;
            }

            Api.Wrapd_SetWindowPos(WindowHandle, nextPoint);
        }

        private void UpdatePosition()
        {
            if (State == AnchorStatus.Empty) {
                return;
            }

            MonitorMaxStepX = (MonitorArea.Right / 100) * Config.Window_Movement_Rate;
            MonitorMaxStepY = (MonitorArea.Bottom / 100) * Config.Window_Movement_Rate;

            var OffSet = Config.Window_Offscreen_Offset; // => user-defined setting;
            RECT curWinRct = Api.Wrapd_GetWindowRect(WindowHandle);
            var _nextPosition = new POINT(MonitorArea.Center.X - curWinRct.Center.X, MonitorArea.Center.Y - curWinRct.Center.Y);

            switch (State, Direction) {
                case (AnchorStatus.Offscreen, DragDirection.Left):
                    _nextPosition.X = (AnchorPt.X - curWinRct.Width) + OffSet;
                    break;
                case (AnchorStatus.Offscreen, DragDirection.Right):
                    _nextPosition.X = AnchorPt.X - OffSet;
                    break;
                case (AnchorStatus.Offscreen, DragDirection.Up):
                    _nextPosition.Y = (AnchorPt.Y - curWinRct.Height) + OffSet;
                    break;
                case (AnchorStatus.Offscreen, DragDirection.Down):
                    _nextPosition.Y = AnchorPt.Y - OffSet;
                    break;
                case (AnchorStatus.OnScreen, DragDirection.Left):
                    _nextPosition.X = AnchorPt.X - 1;
                    break;
                case (AnchorStatus.OnScreen, DragDirection.Right):
                    _nextPosition.X = 1 + AnchorPt.X - curWinRct.Width;
                    break;
                case (AnchorStatus.OnScreen, DragDirection.Up):
                    _nextPosition.Y = AnchorPt.Y - 1;
                    break;
                case (AnchorStatus.OnScreen, DragDirection.Down):
                    _nextPosition.Y = 1 + AnchorPt.Y - curWinRct.Height;
                    break;
                case (_, _): //leave at center screen
                    break;
            }

            NextPosition = _nextPosition;
        }

        public override bool Equals(object obj)
        {
            return obj is AnchorPoint point &&
                   EqualityComparer<POINT>.Default.Equals(AnchorPt, point.AnchorPt) &&
                   Direction == point.Direction &&
                   EqualityComparer<RECT>.Default.Equals(MonitorArea, point.MonitorArea);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AnchorPt, Direction, MonitorArea);
        }

        public static bool operator ==(AnchorPoint left, AnchorPoint right)
        {
            return EqualityComparer<AnchorPoint>.Default.Equals(left, right);
        }

        public static bool operator !=(AnchorPoint left, AnchorPoint right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"AnchorPoint [ Direction: {Direction}, AnchorPt: {AnchorPt}, State: {_State} ]";
        }
    }

    public enum DragDirection : uint
    {
        None = 0,
        Left = 1,
        Right,
        Up,
        Down
    }
}

