using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using HWND = System.IntPtr;

namespace WindowShift
{
    public static class Api
    {
        //############################################################################# wrapped externs
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern uint GetLastError();
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(HWND hwnd, out RECT lpRect);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern HWND FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern HWND SetWindowsHookEx(int hookType, HookProc lpfn, HWND hMod, uint dwThreadId);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(HWND hhk);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern HWND GetParent(HWND hWnd);
        //############################################################################# non-wrapped externs
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern HWND WindowFromPoint(POINT p);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowEnabled(HWND hWnd);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(HWND hWnd);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern HWND CallNextHookEx(HWND hhk, int nCode, WM_MOUSE wParam, [In] MSLLHOOKSTRUCT lParam);
        //############################################################################# wrappers
        public static HWND Wrapd_GetParent(HWND hWnd)
        {
            if(hWnd == HWND.Zero) {
                throw new ArgumentNullException(nameof(hWnd));
            }

            return GetParent(hWnd);
        }

        public static void Wrapd_SetWindowPos(HWND hWnd, POINT pt)
        {
            if (hWnd == HWND.Zero) {
                throw new ArgumentNullException(nameof(hWnd));
            }

            var returnValue = SetWindowPos(hWnd, HWND.Zero, pt.X, pt.Y, 0, 0, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_FRAMECHANGED);

            if (returnValue != true) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public static HWND Wrapd_SetWindowsHookEx(HookProc lpfn)
        {
            if(lpfn == null) {
                throw new ArgumentNullException(nameof(lpfn));
            }

            var WH_MOUSE_LL = 14;
            HWND returnValue = SetWindowsHookEx(WH_MOUSE_LL, lpfn, HWND.Zero, 0);

            if(returnValue == HWND.Zero) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return returnValue;
        }

        public static bool Wrapd_UnhookWindowsHookEx(HWND hhk)
        {
            if(hhk == HWND.Zero) {
                throw new ArgumentNullException(nameof(hhk));
            }

            var returnValue = UnhookWindowsHookEx(hhk);

            if(returnValue == false) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return returnValue;
        }

        public static RECT Wrapd_GetWindowRect(HWND hWnd)
        {
            if (hWnd == HWND.Zero) {
                throw new ArgumentNullException(nameof(hWnd));
            }

#pragma warning disable IDE0018 // Inline variable declaration
            RECT Rect; //do not inline
#pragma warning restore IDE0018 // Inline variable declaration
            var returnValue = GetWindowRect(hWnd, out Rect);

            if (returnValue != true) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return Rect;
        }

        public static HWND Wrapd_FindWindow(string className, string windowName)
        {
            HWND returnValue = FindWindow(className, windowName);

            if (returnValue == HWND.Zero) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return returnValue;
        }
        //#############################################################################
        public delegate HWND HookProc(int code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam);

        public enum EventType : uint
        {
            EVENT_OBJECT_SHOW = 0x8002,
            EVENT_OBJECT_HIDE = 0x8003,
            EVENT_OBJECT_CREATE = 0x8000,
            EVENT_OBJECT_DESTROY = 0x8001,
            EVENT_OBJECT_FOCUS = 0x8005
        }

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

        public override int GetHashCode()
        {
            return X + (19 * Y);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return base.ToString();
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
            if (Left > pt.X) {
                return false;
            }
            if (Right < pt.X) {
                return false;
            }
            if (Bottom < pt.Y) {
                return false;
            }
            if (Top > pt.Y) {
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

        public override int GetHashCode()
        {
            return Left + (15 * Top) + (17 * Right) + (21 * Bottom);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
