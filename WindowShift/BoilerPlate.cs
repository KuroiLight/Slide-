using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HWND = System.IntPtr;

namespace WindowShift
{
    public class WindowObj
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

        public WindowObj(HWND handle)
        {
            hWnd = handle;
        }

        ~WindowObj()
        {
            this.SlideOut();
            this.dockedPoint = null;
        }
        /*        private void MoveWindow(POINT p)
                {
                    SetWindowPos(hWnd, HWND.Zero, p.X, p.Y, 0, 0, SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOSIZE);
                }
        */
        private void MoveWindow(int X, int Y, int Speed)
        {
            SetWindowPos(hWnd, HWND.Zero, X, Y, 0, 0, SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOSIZE);
        }

        /*public POINT GetWindowPos()
        {
            GetWindowRect(hWnd, out RECT r);
            Rect = r;
            Width = Rect.Right - Rect.Left;
            Height = Rect.Bottom - Rect.Top;
            return Position = new POINT(Rect.Left, Rect.Top);
        }*/

        public void SlideOut()
        {
            throw new NotImplementedException();
        }

        public void SlideIn()
        {
            throw new NotImplementedException();
        }

/*        public bool MouseOver()
        {
            GetCursorPos(out POINT p);
            return MouseOver(p);
        }

        public bool MouseOver(POINT p)
        {
            bool Between(int min, int max, int v)
            {
                return min < v && v < max;
            }

            return Between(this.Rect.Left, p.X, this.Rect.Right) && Between(this.Rect.Top, this.Rect.Bottom, p.Y);
        }*/

        //Window Position/Size API
        [DllImport("user32.dll")]
        private static extern HWND WindowFromPoint(POINT p);
        [Flags]
        private enum SetWindowPosFlags : uint
        {
            SWP_ASYNCWINDOWPOS = 0x4000,
            SWP_DEFERERASE = 0x2000,
            SWP_DRAWFRAME = 0x0020,
            SWP_FRAMECHANGED = 0x0020,
            SWP_HIDEWINDOW = 0x0080,
            SWP_NOACTIVATE = 0x0010,
            SWP_NOCOPYBITS = 0x0100,
            SWP_NOMOVE = 0x0002,
            SWP_NOOWNERZORDER = 0x0200,
            SWP_NOREDRAW = 0x0008,
            SWP_NOREPOSITION = 0x0200,
            SWP_NOSENDCHANGING = 0x0400,
            SWP_NOSIZE = 0x0001,
            SWP_NOZORDER = 0x0004,
            SWP_SHOWWINDOW = 0x0040
        }
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(HWND hwnd, out RECT lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);
    }

    public struct DOCKPOINT
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
        }

        public DOCKPOINT(POINT start, POINT end)
        {
            //var mon = Screen.FromPoint(new System.Drawing.Point(end.X, end.Y)).;
            (int xDif, int yDif) = (start.X - end.X, start.Y - end.Y);
            DragDirection dir = (xDif > 0, yDif > 0) switch
            {
                (_, false) when (xDif < yDif) => DragDirection.Down,
                (_, true) when (xDif < yDif) => DragDirection.Up,
                (false, _) when (xDif > yDif) => DragDirection.Left,
                (true, _) when (xDif > yDif) => DragDirection.Right,
                _ => throw new NotSupportedException()
            };

            System.Drawing.Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
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
            }
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
    }

    public enum DragDirection
    {
        Left = 1,
        Right,
        Up,
        Down
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(POINT p1, POINT p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(POINT p1, POINT p2)
        {
            return !p1.Equals(p2);
        }

        public bool Equals(POINT p)
        {
            return p.X == X && p.Y == Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is POINT) {
                return Equals(((POINT)obj));
            } else {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return X + (19 * Y);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public static bool operator ==(RECT r1, RECT r2)
        {
            return r1.Equals(r2);
        }

        public static bool operator !=(RECT r1, RECT r2)
        {
            return !r1.Equals(r2);
        }

        public bool Equals(RECT r)
        {
            return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
        }

        public override bool Equals(object obj)
        {
            if (obj is RECT) {
                return Equals(((RECT)obj));
            } else {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Left + (15 * Top) + (17 * Right) + (21 * Bottom);
        }
    }
}