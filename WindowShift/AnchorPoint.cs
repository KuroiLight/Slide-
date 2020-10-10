using System;
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
        public HWND hWindow { get; private set; }
        public POINT AnchorPt { get; private set; }
        private AnchorStatus State;

        public DragDirection Direction;
        public RECT MonitorArea;

        public AnchorPoint(DragDirection direction, Screen monitor)
        {
            this.Direction = direction;
            MonitorArea = RECT.FromRectangle(monitor.WorkingArea);

            switch (direction) {
                case DragDirection.Left:
                    AnchorPt = new POINT(MonitorArea.Left, (MonitorArea.Bottom - MonitorArea.Top) / 2);
                    break;
                case DragDirection.Right:
                    AnchorPt = new POINT(MonitorArea.Right, (MonitorArea.Bottom - MonitorArea.Top) / 2);
                    break;
                case DragDirection.Up:
                    AnchorPt = new POINT((MonitorArea.Right - MonitorArea.Left) / 2, MonitorArea.Top);
                    break;
                case DragDirection.Down:
                    AnchorPt = new POINT((MonitorArea.Right - MonitorArea.Left) / 2, MonitorArea.Bottom);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        ~AnchorPoint()
        {
            hWindow = HWND.Zero;
            this.Direction = 0;
        }

        public void AttachWindow(HWND newWindow)
        {
            this.ChangeState(AnchorStatus.CenterScreen);
            this.hWindow = newWindow;
            this.ChangeState(AnchorStatus.Offscreen);
        }

        public void RemoveWindow()
        {
            this.hWindow = HWND.Zero;
            this.State = AnchorStatus.Empty;
        }


        public void ChangeState(AnchorStatus toState)
        {
            if (this.hWindow != HWND.Zero && Api.IsWindow(this.hWindow) && this.State != toState) {
                var newPoint = GetNewPosition(toState);
                Api.SetWindowPos(this.hWindow, newPoint);
                this.State = toState;
            }
        }

        private POINT GetNewPosition(AnchorStatus toState)
        {
            int OffSet = 30; // => user-defined setting
            RECT R = Api.GetWindowRect(this.hWindow);

            var newPosition = new POINT((this.MonitorArea.Right - this.MonitorArea.Left) / 2 - ((R.Right - R.Left) / 2), (this.MonitorArea.Bottom - this.MonitorArea.Top) / 2 - ((R.Bottom - R.Top) / 2)); //default center screen

            if (toState == AnchorStatus.Offscreen) {
                switch (this.Direction) {
                    case DragDirection.None:
                        break;
                    case DragDirection.Left:
                        newPosition.X = (this.AnchorPt.X - (R.Right - R.Left)) + OffSet;
                        break;
                    case DragDirection.Right:
                        newPosition.X = this.AnchorPt.X - OffSet;
                        break;
                    case DragDirection.Up:
                        newPosition.Y = (this.AnchorPt.Y - (R.Bottom - R.Top)) + OffSet;
                        break;
                    case DragDirection.Down:
                        newPosition.Y = this.AnchorPt.Y - OffSet;
                        break;
                    default:
                        break;
                }
            } else if(toState == AnchorStatus.OnScreen) {
                switch (this.Direction) {
                    case DragDirection.None:
                        break;
                    case DragDirection.Left:
                        newPosition.X = this.AnchorPt.X;
                        break;
                    case DragDirection.Right:
                        newPosition.X = (this.AnchorPt.X - (R.Right - R.Left));
                        break;
                    case DragDirection.Up:
                        newPosition.Y = this.AnchorPt.Y;
                        break;
                    case DragDirection.Down:
                        newPosition.Y = (this.AnchorPt.Y - (R.Bottom - R.Top));
                        break;
                    default:
                        break;
                }
            }

            return newPosition;
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

