using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using HWND = System.IntPtr;

namespace WindowShift
{

    public class AnchorPoint
    {
        public HWND ContainedWindow;
        public POINT? AnchorPt;
        public DragDirection Direction;
        public int MonitorIndex;

        public AnchorPoint(DragDirection direction, int monitor, POINT anchorPt)
        {
            this.Direction = direction;
            this.MonitorIndex = monitor;
            this.AnchorPt = anchorPt;
        }

        ~AnchorPoint()
        {
            ContainedWindow = HWND.Zero;
            AnchorPt = null;
            this.Direction = 0;
        }
    }



    /*    public class WindowObj
        {
            public HWND hWnd;
            public RECT Rect
            {
                get {
                    GetWindowRect(hWnd, out RECT r);
                    return r;
                }
            }
            public DOCKPOINT? dockedPoint;
            public bool Selected = false;
            private HWND rHook_Destroy = HWND.Zero;
            private HWND rHook_Focus = HWND.Zero;

            public WindowObj(HWND handle)
            {
                hWnd = handle;
                uint ThreadId = GetWindowThreadProcessId(this.hWnd, out uint ProcId);
                this.rHook_Destroy = SetWinEventHook(EventType.EVENT_OBJECT_DESTROY, EventType.EVENT_OBJECT_DESTROY, HWND.Zero, this.EventHookProc, ProcId, ThreadId, 0x0000 | 0x0002);
                this.rHook_Focus = SetWinEventHook(EventType.EVENT_OBJECT_FOCUS, EventType.EVENT_OBJECT_FOCUS, HWND.Zero, this.EventHookProc, ProcId, ThreadId, 0x0000 | 0x0002);
                var stringContainer = new StringBuilder(256);
                GetWindowText(hWnd, stringContainer, 256);
                Debug.WriteLine(hWnd + " - " + stringContainer.ToString());
            }

            ~WindowObj()
            {
                this.SlideOut();
                this.dockedPoint = null;
                UnhookWinEvent(this.rHook_Destroy);
                UnhookWinEvent(this.rHook_Focus);
            }

            private void MoveWindow(int X, int Y, int Speed)
            {
                SetWindowPos(hWnd, HWND.Zero, X, Y, 0, 0, SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOSIZE);
            }

            public void SlideOut()
            {
                throw new NotImplementedException();
            }

            public void SlideIn()
            {
                throw new NotImplementedException();
            }

            private void EventHookProc(IntPtr hWinEventHook, EventType eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
            {
                if(eventType == EventType.EVENT_OBJECT_DESTROY) {
                    WindowManager.Singleton.Windows.Remove(this);
                    var stringContainer = new StringBuilder(256);
                    GetWindowText(hwnd, stringContainer, 256);
                    Debug.WriteLine(Enum.GetName(typeof(EventType), eventType) + " - " + stringContainer.ToString());
                } else if(eventType == EventType.EVENT_OBJECT_FOCUS && this.dockedPoint != null) {
                    this.SlideOut();
                }
            }
        }*/

    /*public struct DOCKPOINT
    {
        POINT CenterPos;
        readonly DragDirection Direction;


        public DOCKPOINT(DragDirection dir)
        {
            
            Direction = dir;
            System.Drawing.Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;

            switch (Direction) {
                case DragDirection.Left:
                    CenterPos = new POINT(workingArea.Left, (workingArea.Bottom - workingArea.Top) / 2);
                    break;
                case DragDirection.Right:
                    CenterPos = new POINT(workingArea.Right, (workingArea.Bottom - workingArea.Top) / 2);
                    break;
                case DragDirection.Up:
                    CenterPos = new POINT((workingArea.Right - workingArea.Left) / 2, workingArea.Top);
                    break;
                case DragDirection.Down:
                    CenterPos = new POINT((workingArea.Right - workingArea.Left) / 2, workingArea.Bottom);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            //need more comprehensive test for taskbar here

            if(ChildWindowFromPointEx(this.CenterPos) == Api.FindWindow("Shell_TrayWnd", null))
        }

        public static DragDirection GetDirection(POINT start, POINT end)
        {
            //var mon = Screen.FromPoint(new System.Drawing.Point(end.X, end.Y)).;
            (int xDif, int yDif) = (start.X - end.X, start.Y - end.Y);
            DragDirection dir = (xDif > 0, yDif > 0) switch
            {
                (_, false) when (xDif < yDif) => DragDirection.Down,
                (_, true) when (xDif < yDif) => DragDirection.Up,
                (false, _) when (xDif > yDif) => DragDirection.Left,
                (true, _) when (xDif > yDif) => DragDirection.Right,
                _ => 0
            };

            return dir;

            *//*System.Drawing.Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            switch (Direction = dir) {
                case DragDirection.Left:
                    CenterPos = new POINT(workingArea.Left, (workingArea.Bottom - workingArea.Top) / 2);
                    break;
                case DragDirection.Right:
                    CenterPos = new POINT(workingArea.Right, (workingArea.Bottom - workingArea.Top) / 2);
                    break;
                case DragDirection.Up:
                    CenterPos = new POINT((workingArea.Right - workingArea.Left) / 2, workingArea.Top);
                    break;
                case DragDirection.Down:
                    CenterPos = new POINT((workingArea.Right - workingArea.Left) / 2, workingArea.Bottom);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }*//*

        }

        public static bool operator ==(DOCKPOINT p1, DOCKPOINT p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(DOCKPOINT p1, DOCKPOINT p2)
        {
            return !p1.Equals(p2);
        }

        public bool Equals(DOCKPOINT p)
        {
            return p.Direction == Direction; // && this.Monitor == p.Monitor;
        }

        public override bool Equals(object obj)
        {
            if (obj is DOCKPOINT) {
                return Equals(((DOCKPOINT)obj));
            } else {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return CenterPos.X + (19 * CenterPos.Y);
        }

        public override string ToString()
        {
            return string.Format("Direction: {0}, X: {1}, Y: {2}", Direction.ToString(), CenterPos.X, CenterPos.Y);
        }
    }*/

    public enum DragDirection
    {
        None = 0,
        Left = 1,
        Right,
        Up,
        Down
    }
}

