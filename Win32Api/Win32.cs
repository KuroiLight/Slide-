using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using static Win32Api.Imports;

namespace Win32Api
{
    public static class User32
    {
        public static IntPtr CallNextHookEx(IntPtr hhk, int nCode, WM_MOUSE wParam, [In] MSLLHOOKSTRUCT lParam)
        {
            return Imports.CallNextHookEx(hhk, nCode, wParam, lParam);
        }

        public static bool EnableWindow(IntPtr hWnd, bool bEnable)
        {
            return Imports.EnableWindow(hWnd, bEnable);
        }

        public static POINT GetCursorPos()
        {
            Imports.GetCursorPos(out POINT pOINT);
            return pOINT;
        }

        public static bool GetLayeredWindowAttributes(IntPtr hwnd, uint crKey, out byte bAlpha, out uint dwFlags)
        {
            return Imports.GetLayeredWindowAttributes(hwnd, crKey, out bAlpha, out dwFlags);
        }

        public static IntPtr GetParent(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) {
                throw new ArgumentNullException(nameof(hWnd));
            }

            return Imports.GetParent(hWnd);
        }

        public static IntPtr GetRootWindow(POINT pt)
        {
            IntPtr WFPhWnd = Imports.WindowFromPoint(pt);
            return Imports.GetAncestor(WFPhWnd, GetAncestorFlags.GetRoot);
        }

        public static int GetWindowLong(IntPtr hWnd, int nIndex)
        {
            return Imports.GetWindowLong(hWnd, nIndex);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0018:Inline variable declaration", Justification = "Incase of failure we dont know if Rect will be initialized by GetWindowRect")]
        public static RECT GetWindowRect(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) {
                throw new ArgumentNullException(nameof(hWnd));
            }

            RECT Rect; //do not inline
            var returnValue = Imports.GetWindowRect(hWnd, out Rect);

            if (!returnValue) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return Rect;
        }

        public static string GetWindowText(IntPtr handle)
        {
            var sb = new StringBuilder(512);
            Imports.GetWindowText(handle, sb, sb.Capacity);
            return sb.ToString();
        }

        public static bool IsWindow(IntPtr hWnd)
        {
            return Imports.IsWindow(hWnd);
        }

        public static bool IsWindowEnabled(IntPtr hWnd)
        {
            return Imports.IsWindowEnabled(hWnd);
        }

        public static bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags)
        {
            return Imports.SetLayeredWindowAttributes(hwnd, crKey, bAlpha, dwFlags);
        }

        public static int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            return Imports.SetWindowLong(hWnd, nIndex, dwNewLong);
        }

        /// <summary>
        /// Sets the windows new position
        /// uses flags SWP_NOZORDER, SWP_NOSIZE and SWP_FRAMECHANGED
        /// fails silently outputting exception to debug console
        /// </summary>
        /// <param name="hWnd">window to move</param>
        /// <param name="pt">absolute position to be set</param>
        public static void SetWindowPos(IntPtr hWnd, POINT pt)
        {
            if (hWnd == IntPtr.Zero) {
                throw new ArgumentNullException(nameof(hWnd));
            }

            var returnValue = Imports.SetWindowPos(hWnd, IntPtr.Zero, pt.X, pt.Y, 0, 0, Imports.SetWindowPosFlags.SWP_NOZORDER | Imports.SetWindowPosFlags.SWP_NOSIZE | Imports.SetWindowPosFlags.SWP_FRAMECHANGED);

            if (!returnValue) {
                //this usually fails during a race condition, e.g window is closed right before SetWindowPos is called
                //instead of throwing here, we should just write the exception to debug
                Debug.WriteLine(new Win32Exception(Marshal.GetLastWin32Error()).Message);
                //throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public static IntPtr SetWindowsHookEx(HookProc lpfn)
        {
            if (lpfn == null) {
                throw new ArgumentNullException(nameof(lpfn));
            }

            const int WH_MOUSE_LL = 14;
            IntPtr returnValue = Imports.SetWindowsHookEx(WH_MOUSE_LL, lpfn, IntPtr.Zero, 0);

            if (returnValue == IntPtr.Zero) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return returnValue;
        }

        public static bool UnhookWindowsHookEx(IntPtr hhk)
        {
            if (hhk == IntPtr.Zero) {
                throw new ArgumentNullException(nameof(hhk));
            }

            var returnValue = Imports.UnhookWindowsHookEx(hhk);

            if (!returnValue) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return returnValue;
        }
    }
}