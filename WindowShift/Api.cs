using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using HWND = System.IntPtr;

namespace WindowShift
{
    public static class Api
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);
        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(HWND hwnd, out RECT lpRect);
        //#############################################################################
        public static void SetWindowPos(HWND hWnd, POINT pt)
        {
            if(hWnd == HWND.Zero) {
                throw new ArgumentNullException("hWnd");
            }

            var returnValue = SetWindowPos(hWnd, HWND.Zero, pt.X, pt.Y, 0, 0, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_FRAMECHANGED);

            if(returnValue != true) {
                ThrowWin32Error();
            }
        }

        public static RECT GetWindowRect(HWND hWnd)
        {
            if(hWnd == HWND.Zero) {
                throw new ArgumentNullException("hWnd");
            }

            RECT Rect;
            var returnValue = GetWindowRect(hWnd, out Rect);

            if(returnValue != true) {
                ThrowWin32Error();
            }

            return Rect;
        }




        private static void ThrowWin32Error()
        {
            var lastError = GetLastError();

            throw new Win32Exception((int)lastError);
        }
        //#############################################################################

        [DllImport("user32.dll", SetLastError = true)]
        public static extern HWND FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);


        public delegate void WinEventDelegate(HWND hWinEventHook, EventType eventType, HWND hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        public delegate bool WNDENUMPROC(HWND hWnd, uint lParam);
        public delegate HWND HookProc(int code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern HWND SetWinEventHook(EventType eventMin, EventType eventMax, HWND
                                             hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
                                             uint idThread, uint dwFlags);
        public enum EventType : uint
        {
            EVENT_OBJECT_SHOW = 0x8002,
            EVENT_OBJECT_HIDE = 0x8003,
            EVENT_OBJECT_CREATE = 0x8000,
            EVENT_OBJECT_DESTROY = 0x8001,
            EVENT_OBJECT_FOCUS = 0x8005
        }

        public const int MMOUSEWHEEL = 0x020A;
        public const int WH_MOUSE_LL = 14;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern HWND SetWindowsHookEx(int hookType, HookProc lpfn, HWND hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern HWND CallNextHookEx(HWND hhk, int nCode, WM_MOUSE wParam, [In] MSLLHOOKSTRUCT lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }

        public enum WM_MOUSE : uint
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_MOUSEHWHEEL = 0x020E,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208
        }

        [DllImport("user32", SetLastError = true)]
        public static extern HWND GetWindowText(HWND hwnd, StringBuilder lptrString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern HWND WindowFromPoint(POINT p);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern HWND GetWindowModuleFileName(HWND hwnd,
                                                   StringBuilder lpszFileName, uint cchFileNameMax);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern HWND EnumWindows(WNDENUMPROC lpEnumFunc, uint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(HWND hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowEnabled(HWND hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(HWND hhk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWinEvent(HWND hWinEventHook);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern HWND GetParent(HWND hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern HWND GetWindowThreadProcessId(HWND hWnd, out HWND lpdwProcessId);

        [Flags]
        public enum SetWindowPosFlags : uint
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
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(HWND hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern HWND ChildWindowFromPointEx(HWND hWndParent, POINT pt, uint uFlags);

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();
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

        public static RECT FromRectangle(System.Drawing.Rectangle R)
        {
            return (new RECT(R.Left, R.Top, R.Right, R.Bottom));
        }

        public bool Contains(POINT pt)
        {
            if (this.Left > pt.X) {
                return false;
            }
            if (this.Right < pt.X) {
                return false;
            }
            if (this.Bottom < pt.Y) {
                return false;
            }
            if (this.Top > pt.Y) {
                return false;
            }

            return true;
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
