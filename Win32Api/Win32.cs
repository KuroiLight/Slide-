using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Win32Api
{
    public struct Constants
    {
        public const int GWL_EXSTYLE = -20;
        public const int GWL_STYLE = -16;
        public const uint LWA_ALPHA = 0x2;
        public const int WS_EX_LAYERED = 0x80000;
    }

    [Flags]
    public enum SetWindowPosFlags : uint
    {
        SWP_NONE = 0x0000,
        SWP_NOSIZE = 0x0001,
        SWP_NOMOVE = 0x0002,
        SWP_NOZORDER = 0x0004,
        SWP_NOREDRAW = 0x0008,
        SWP_NOACTIVATE = 0x0010,
        SWP_DRAWFRAME = 0x0020,
        SWP_FRAMECHANGED = SWP_DRAWFRAME,
        SWP_SHOWWINDOW = 0x0040,
        SWP_HIDEWINDOW = 0x0080,
        SWP_NOCOPYBITS = 0x0100,
        SWP_NOOWNERZORDER = 0x0200,
        SWP_NOREPOSITION = SWP_NOOWNERZORDER,
        SWP_NOSENDCHANGING = 0x0400,
        SWP_DEFERERASE = 0x2000,
        SWP_ASYNCWINDOWPOS = 0x4000
    }

    public enum WM_MOUSE : uint
    {
        WM_MOUSEMOVE = 0x0200,
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205,
        WM_MBUTTONDOWN = 0x0207,
        WM_MBUTTONUP = 0x0208,
        WM_MOUSEWHEEL = 0x020A,
        WM_MOUSEHWHEEL = 0x020E
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

    public static class User32
    {
        public delegate IntPtr HookProc(int code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetLayeredWindowAttributes(IntPtr hwnd, uint crKey, out byte bAlpha, out uint dwFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        /// <summary>
        /// Calls the next hook in the chain, should be called if not blocking the event
        /// </summary>
        /// <param name="hhk"></param>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, WM_MOUSE wParam, [In] MSLLHOOKSTRUCT lParam);

        /// <summary>
        /// Checks if given handle is a valid window
        /// </summary>
        /// <param name="hWnd">handle to the window being tested</param>
        /// <returns>bool true if hadnle is a window, otherwise false</returns>
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        /// <summary>
        /// Checks if a given window handle is a window thats enabled
        /// </summary>
        /// <param name="hWnd">handle to the widnow to check</param>
        /// <returns>bool true if window is enabled, false otherwise</returns>
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowEnabled(IntPtr hWnd);

        /// <summary>
        /// Checks if a given window handle is a window thats visible
        /// </summary>
        /// <param name="hWnd">handle to the widnow to check</param>
        /// <returns>bool true if window is visible, false otherwise</returns>
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// Get the window handle of the window at POINT p
        /// </summary>
        /// <param name="p">POINT p of location to find window at</param>
        /// <returns>IntPtr handle of window</returns>
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr WindowFromPoint(POINT p);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public static IntPtr Wrapd_GetParent(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) {
                throw new ArgumentNullException(nameof(hWnd));
            }

            return GetParent(hWnd);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0018:Inline variable declaration", Justification = "Incase of failure we dont know if Rect will be initialized by GetWindowRect")]
        public static RECT Wrapd_GetWindowRect(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) {
                throw new ArgumentNullException(nameof(hWnd));
            }

            RECT Rect; //do not inline
            var returnValue = GetWindowRect(hWnd, out Rect);

            if (!returnValue) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return Rect;
        }

        /// <summary>
        /// Sets the windows new position
        /// uses flags SWP_NOZORDER, SWP_NOSIZE and SWP_FRAMECHANGED
        /// fails silently outputting exception to debug console
        /// </summary>
        /// <param name="hWnd">window to move</param>
        /// <param name="pt">absolute position to be set</param>
        public static void Wrapd_SetWindowPos(IntPtr hWnd, POINT pt)
        {
            if (hWnd == IntPtr.Zero) {
                throw new ArgumentNullException(nameof(hWnd));
            }

            var returnValue = SetWindowPos(hWnd, IntPtr.Zero, pt.X, pt.Y, 0, 0, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_FRAMECHANGED);

            if (!returnValue) {
                //this usually fails during a race condition, e.g window is closed right before SetWindowPos is called
                //instead of throwing here, we should just write the exception to debug
                Debug.WriteLine(new Win32Exception(Marshal.GetLastWin32Error()).Message);
                //throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public static string GetWindowText(IntPtr handle)
        {
            var sb = new StringBuilder(512);
            GetWindowText(handle, sb, sb.Capacity);
            return sb.ToString();
        }

        public static IntPtr GetParentWindowFromPoint(POINT pt)
        {
            IntPtr currentWindow = WindowFromPoint(pt);
            //Debug.Write($"{currentWindow}  ({GetWindowText(currentWindow)}) ");
            if (currentWindow != IntPtr.Zero) {
                IntPtr parent = GetParent(currentWindow);
                while (parent != IntPtr.Zero) {
                    currentWindow = parent;
                    //Debug.Write($"{currentWindow} ({GetWindowText(currentWindow)}) ");
                    parent = GetParent(currentWindow);
                }
                //Debug.WriteLine($"{currentWindow} ({GetWindowText(currentWindow)}) [{parent}].");
                return currentWindow;
            } else {
                return IntPtr.Zero;
            }
        }

        public static IntPtr Wrapd_SetWindowsHookEx(HookProc lpfn)
        {
            if (lpfn == null) {
                throw new ArgumentNullException(nameof(lpfn));
            }

            const int WH_MOUSE_LL = 14;
            IntPtr returnValue = SetWindowsHookEx(WH_MOUSE_LL, lpfn, IntPtr.Zero, 0);

            if (returnValue == IntPtr.Zero) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return returnValue;
        }

        public static bool Wrapd_UnhookWindowsHookEx(IntPtr hhk)
        {
            if (hhk == IntPtr.Zero) {
                throw new ArgumentNullException(nameof(hhk));
            }

            var returnValue = UnhookWindowsHookEx(hhk);

            if (!returnValue) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return returnValue;
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        //These functions are wrapped
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    }
}