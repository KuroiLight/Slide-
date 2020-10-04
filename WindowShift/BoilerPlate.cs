using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DWORD = System.UInt32;
using HWND = System.IntPtr;

namespace WindowShift
{
    public class BasicWindow
    {
        public HWND hWnd;
        public Process process;
        public bool stillExists => !process.HasExited && IsWindow(hWnd);

        public const int MAXTITLELENGTH = 256;
        public POINT Position
        {
            get => GetWindowPos();
            set => MoveWindow(value);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public RECT rect { get; private set; }

        public StringBuilder title = new StringBuilder(MAXTITLELENGTH);

        public BasicWindow(HWND handle)
        {
            hWnd = handle;
            GetWindowThreadProcessId(hWnd, out uint pid);
            process = Process.GetProcessById((int)pid);
            process.EnableRaisingEvents = true;
            process.Exited += WindowClosed;
            GetWindowPos();
            GetWindowText(hWnd, title, MAXTITLELENGTH);
        }

        public void MoveWindow(POINT p)
        {
            SetWindowPos(hWnd, HWND.Zero, p.X, p.Y, 0, 0, SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOSIZE);
        }

        public void MoveWindow(int X, int Y)
        {
            SetWindowPos(hWnd, HWND.Zero, X, Y, 0, 0, SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOSIZE);
        }

        public POINT GetWindowPos()
        {
            GetWindowRect(hWnd, out RECT r);
            rect = r;
            Width = rect.Right - rect.Left;
            Height = rect.Bottom - rect.Top;
            return Position = new POINT(rect.Left, rect.Top);
        }

        private void WindowClosed(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }

        [DllImport("user32")]
        private static extern IntPtr GetWindowText(HWND hWnd, StringBuilder lptrString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern DWORD GetWindowThreadProcessId(HWND hWnd, out DWORD lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        //Window Position/Size API
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
    }

    public class DockedWindow : BasicWindow
    {
        public DOCKPOINT dockedPoint;

        public DockedWindow(HWND handle, DOCKPOINT dockPosition) : base(handle)
        {
            dockedPoint = dockPosition;
            MoveWindowToDock();
        }

        public void ShiftWindow(bool slideout)
        {
            if (slideout) {
                // slide the docked window out gracefully
            } else {
                // slide the undocked window in gracefully
            }
            throw new NotImplementedException();
        }

        private void MoveWindowToDock()
        {
            // move the window to the docked position (quickly?)
            throw new NotImplementedException();
        }
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
#pragma warning disable CS0162 // Unreachable code detected
                    break;
#pragma warning restore CS0162 // Unreachable code detected
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
#pragma warning disable CS0162 // Unreachable code detected
                    break;
#pragma warning restore CS0162 // Unreachable code detected
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