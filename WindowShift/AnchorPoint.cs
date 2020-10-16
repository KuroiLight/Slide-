using System;
using System.Collections.Generic;
using System.Windows.Forms;
using HWND = System.IntPtr;

namespace WindowShift
{
    public enum AnchorStatus
    {
        Offscreen = 1,
        OnScreen = 2,
        CenterScreen = 3,
        Empty = 4
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
                } else if (State != AnchorStatus.CenterScreen) {
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
                }
            }
        }

        public POINT AnchorPt { get; private set; }
        public DragDirection Direction { get; private set; }

        private RECT MonitorArea;
        private POINT NextPosition;
        private int MonitorMaxStepX = 10, MonitorMaxStepY = 10;
        private Settings Config = Settings.SettingsInstance;

        public AnchorPoint(DragDirection direction, Screen monitor)
        {
            Direction = direction;
            MonitorArea = RECT.FromRectangle(monitor.WorkingArea);

            AnchorPt = direction switch
            {
                DragDirection.Left => new POINT(MonitorArea.Left, MonitorArea.Height / 2),
                DragDirection.Right => new POINT(MonitorArea.Right, MonitorArea.Height / 2),
                DragDirection.Up => new POINT(MonitorArea.Width / 2, MonitorArea.Top),
                DragDirection.Down => new POINT(MonitorArea.Width / 2, MonitorArea.Bottom),
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
            if (State == AnchorStatus.Empty) {
                return;
            }

            RECT winRect = Api.Wrapd_GetWindowRect(WindowHandle);
            winRect.Left += Math.Clamp(NextPosition.X - winRect.Left, -1 * MonitorMaxStepX, MonitorMaxStepX);
            winRect.Top += Math.Clamp(NextPosition.Y - winRect.Top, -1 * MonitorMaxStepY, MonitorMaxStepY);

            Api.Wrapd_SetWindowPos(WindowHandle, winRect.ToPoint());
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
                    _nextPosition.X = AnchorPt.X;
                    break;
                case (AnchorStatus.OnScreen, DragDirection.Right):
                    _nextPosition.X = AnchorPt.X - curWinRct.Width;
                    break;
                case (AnchorStatus.OnScreen, DragDirection.Up):
                    _nextPosition.Y = AnchorPt.Y;
                    break;
                case (AnchorStatus.OnScreen, DragDirection.Down):
                    _nextPosition.Y = AnchorPt.Y - curWinRct.Height;
                    break;
                case (_, _): //leave at center screen
                    break;
            }

            NextPosition = _nextPosition;
        }

        public bool SameScreen(POINT pt)
        {
            return MonitorArea.Contains(pt);
        }

        public bool ContainedIn(RECT area)
        {
            return area.Contains(AnchorPt);
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

