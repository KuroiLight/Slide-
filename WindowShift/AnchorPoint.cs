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
        public HWND WindowHandle { get; private set; }
        public POINT AnchorPt { get; private set; }
        private AnchorStatus State;

        public DragDirection Direction;
        public RECT MonitorArea;

        private Timer MovementTimer;
        private POINT targetPosition;

        public AnchorPoint(DragDirection direction, Screen monitor)
        {
            Direction = direction;
            MonitorArea = RECT.FromRectangle(monitor.WorkingArea);

            AnchorPt = direction switch
            {
                DragDirection.Left => new POINT(MonitorArea.Left, (MonitorArea.Bottom - MonitorArea.Top) / 2),
                DragDirection.Right => new POINT(MonitorArea.Right, (MonitorArea.Bottom - MonitorArea.Top) / 2),
                DragDirection.Up => new POINT((MonitorArea.Right - MonitorArea.Left) / 2, MonitorArea.Top),
                DragDirection.Down => new POINT((MonitorArea.Right - MonitorArea.Left) / 2, MonitorArea.Bottom),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        ~AnchorPoint()
        {
            WindowHandle = HWND.Zero;
            Direction = 0;
        }

        public void AttachWindow(HWND newWindow)
        {
            RemoveWindow();
            WindowHandle = newWindow;
            ChangeState(AnchorStatus.Offscreen);
        }

        public void RemoveWindow()
        {
            ChangeState(AnchorStatus.CenterScreen);
            WindowHandle = HWND.Zero;
            State = AnchorStatus.Empty;
        }


        public void ChangeState(AnchorStatus toState)
        {
            if (WindowHandle != HWND.Zero && State != toState && Api.IsWindow(WindowHandle)) {
                targetPosition = GetNewPosition(toState);
                State = toState;
                SendToNewPosition();
            }
        }

        private void SendToNewPosition()
        {
            if(MovementTimer != null && MovementTimer.Enabled) {
                return;
            }

            var Speed = 5; // => user-defined setting
            MovementTimer = new Timer();
            MovementTimer.Interval = Speed;
            MovementTimer.Tick += MovementTimer_Tick;
            MovementTimer.Start();
        }

        private void MovementTimer_Tick(object sender, EventArgs e)
        {
            if(!Api.IsWindow(WindowHandle)) {
                RemoveWindow();
                MovementTimer.Stop();
                return;
            }

            RECT R = Api.Wrapd_GetWindowRect(WindowHandle);

            var StepSizeX = (MonitorArea.Right / 100) * 5;
            var StepSizeY = (MonitorArea.Bottom / 100) * 5;
            
            if(R.Left + StepSizeX >= targetPosition.X && R.Left - StepSizeX <= targetPosition.X && R.Top + StepSizeY >= targetPosition.Y && R.Top - StepSizeY <= targetPosition.Y ) {
                MovementTimer.Stop();
                Api.Wrapd_SetWindowPos(WindowHandle, targetPosition);
            }

            var nextPoint = new POINT();

            if (Math.Abs(R.Left - targetPosition.X) < StepSizeX+1) {
                StepSizeX = 1;
            }
            if (Math.Abs(R.Top - targetPosition.Y) < StepSizeY+1) {
                StepSizeY = 1;
            }

            if (R.Left > targetPosition.X) {
                nextPoint.X = R.Left - StepSizeX;
            } else if(R.Left < targetPosition.X) {
                nextPoint.X = R.Left + StepSizeX;
            } else {
                nextPoint.X = R.Left;
            }
            if (R.Top > targetPosition.Y) {
                nextPoint.Y = R.Top - StepSizeY;
            } else if (R.Top < targetPosition.Y) {
                nextPoint.Y = R.Top + StepSizeY;
            } else {
                nextPoint.Y = R.Top;
            }

            Debug.WriteLine(R.Left + " " + R.Top + " => " + nextPoint.X + " " + nextPoint.Y);

            Api.Wrapd_SetWindowPos(WindowHandle, nextPoint);
        }

        private POINT GetNewPosition(AnchorStatus toState)
        {
            var OffSet = 30; // => user-defined setting
            RECT R = Api.Wrapd_GetWindowRect(WindowHandle);

            var newPosition = new POINT((MonitorArea.Right - MonitorArea.Left) / 2 - ((R.Right - R.Left) / 2), (MonitorArea.Bottom - MonitorArea.Top) / 2 - ((R.Bottom - R.Top) / 2)); //default center screen

            if (toState == AnchorStatus.Offscreen) {
                switch (Direction) {
                    case DragDirection.None:
                        break;
                    case DragDirection.Left:
                        newPosition.X = (AnchorPt.X - (R.Right - R.Left)) + OffSet;
                        break;
                    case DragDirection.Right:
                        newPosition.X = AnchorPt.X - OffSet;
                        break;
                    case DragDirection.Up:
                        newPosition.Y = (AnchorPt.Y - (R.Bottom - R.Top)) + OffSet;
                        break;
                    case DragDirection.Down:
                        newPosition.Y = AnchorPt.Y - OffSet;
                        break;
                    default:
                        break;
                }
            } else if (toState == AnchorStatus.OnScreen) {
                switch (Direction) {
                    case DragDirection.None:
                        break;
                    case DragDirection.Left:
                        newPosition.X = AnchorPt.X;
                        break;
                    case DragDirection.Right:
                        newPosition.X = (AnchorPt.X - (R.Right - R.Left));
                        break;
                    case DragDirection.Up:
                        newPosition.Y = AnchorPt.Y;
                        break;
                    case DragDirection.Down:
                        newPosition.Y = (AnchorPt.Y - (R.Bottom - R.Top));
                        break;
                    default:
                        break;
                }
            }

            return newPosition;
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

