using System;
using System.Windows.Forms;
using HWND = System.IntPtr;

namespace WindowShift
{

    public class AnchorPoint
    {
        private HWND windowHandle;
        public HWND WindowHandle { get => windowHandle; set { this.TransitionWindow(false); windowHandle = value; this.TransitionWindow(true); } }

        public POINT AnchorPt;
        public DragDirection Direction;
        public RECT MonitorArea;
        public bool InTransit = false;
        public bool Hidden = false;

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
            windowHandle = HWND.Zero;
            this.Direction = 0;
        }

        public void TransitionWindow(bool Hide, int speed = 5)
        {
            if (this.windowHandle != HWND.Zero) {
                POINT newPosition = this.GetNewPosition(Hide);

                //animate transition heere
                Api.SetWindowPos(this.windowHandle, HWND.Zero, newPosition.X, newPosition.Y, 0, 0, Api.SetWindowPosFlags.SWP_NOSIZE | Api.SetWindowPosFlags.SWP_NOZORDER);
            }
        }

        private POINT GetNewPosition(bool Hide, int OffSet = 30)
        {
            Api.GetWindowRect(this.windowHandle, out RECT R);
            POINT newPosition = new POINT((this.MonitorArea.Right - this.MonitorArea.Left) / 2 - ((R.Right - R.Left) / 2), (this.MonitorArea.Bottom - this.MonitorArea.Top) / 2 - ((R.Bottom - R.Top) / 2)); //default center screen

            if (Hide) {
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
            } else {
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

