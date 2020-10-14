using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                _State = value;
                UpdatePosition();
            }
        }

        public POINT AnchorPt { get; private set; }
        private RECT MonitorArea;
        private POINT targetPosition;

        public DragDirection Direction { get; private set; }

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

            var StepSizeX = (int)Math.CopySign((MonitorArea.Right / 100) * 5, -1*(winRect.Left - targetPosition.X)); //sign is wrong
            var StepSizeY = (int)Math.CopySign((MonitorArea.Bottom / 100) * 5, -1*(winRect.Top - targetPosition.Y));
            
            bool inRange(int x, int min, int max) => ((x - max)*(x - min) <= 0);

            if(inRange(targetPosition.X, winRect.Left, winRect.Left + StepSizeX)) {
                StepSizeX = 0;
            }

            if (inRange(targetPosition.Y, winRect.Top, winRect.Top + StepSizeY)) {
                StepSizeY = 0;
            }

            //if we're already there, bump us to the right spot and return
            if (StepSizeX == 0 && StepSizeY == 0) {
                Api.Wrapd_SetWindowPos(WindowHandle, targetPosition);
                return;
            }

            //Debug.Write(winRect.Left + ":" + winRect.Top + " => ");

            winRect.Left += (int)StepSizeX;
            winRect.Top += (int)StepSizeY;

            //Debug.Write(winRect.Left + ":" + winRect.Top + "\n");

            Api.Wrapd_SetWindowPos(WindowHandle, winRect.ToPoint());
        }

        private void UpdatePosition()
        {
            if(State == AnchorStatus.Empty) {
                return;
            }

            var OffSet = 30; // => user-defined setting;
            RECT curWinRct = Api.Wrapd_GetWindowRect(WindowHandle);

            targetPosition = new POINT(MonitorArea.Width / 2 - (curWinRct.Width / 2), MonitorArea.Height / 2 - (curWinRct.Height / 2)); //default center screen

            if (State == AnchorStatus.Offscreen) {
                switch (Direction) {
                    case DragDirection.Left:
                        targetPosition.X = (AnchorPt.X - curWinRct.Width) + OffSet;
                        break;
                    case DragDirection.Right:
                        targetPosition.X = AnchorPt.X - OffSet;
                        break;
                    case DragDirection.Up:
                        targetPosition.Y = (AnchorPt.Y - curWinRct.Height) + OffSet;
                        break;
                    case DragDirection.Down:
                        targetPosition.Y = AnchorPt.Y - OffSet;
                        break;
                    default:
                        break;
                }
            } else if (State == AnchorStatus.OnScreen) {
                switch (Direction) {
                    case DragDirection.Left:
                        targetPosition.X = AnchorPt.X;
                        break;
                    case DragDirection.Right:
                        targetPosition.X = (AnchorPt.X - curWinRct.Width);
                        break;
                    case DragDirection.Up:
                        targetPosition.Y = AnchorPt.Y;
                        break;
                    case DragDirection.Down:
                        targetPosition.Y = (AnchorPt.Y - curWinRct.Height);
                        break;
                    default:
                        break;
                }
            }
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

