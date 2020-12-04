using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static Win32Api.Imports;

namespace Win32Api
{
    public static class User32
    {
        public static TITLEBARINFO GetTitleBarInfo(IntPtr hWnd)
        {
            TITLEBARINFO TBI = new();
            TBI.cbSize = (uint)Marshal.SizeOf(TBI);

            bool returnValue = Imports.GetTitleBarInfo(hWnd, ref TBI);

            if (!returnValue)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return TBI;
        }

        public static IntPtr CallNextHookEx(IntPtr hhk, int nCode, WM_MOUSE wParam, [In] MSLLHOOKSTRUCT lParam)
        {
            return Imports.CallNextHookEx(hhk, nCode, wParam, lParam);
        }

        public static POINT GetCursorPos()
        {
            Imports.GetCursorPos(out POINT pOINT);
            return pOINT;
        }

        public static IntPtr GetRootWindowFromTitlebar(POINT pt)
        {
            IntPtr rootWindow = GetRootWindow(pt);
            if (rootWindow != IntPtr.Zero && GetTitleBarInfo(rootWindow).rcTitleBar.Contains(pt))
            {
                return rootWindow;
            }
            return IntPtr.Zero;
        }

        public static IntPtr GetRootWindow(POINT pt)
        {
            IntPtr WFPhWnd = Imports.WindowFromPoint(pt);
            return Imports.GetAncestor(WFPhWnd, Ancestor.GetRoot);
        }

        public static int GetWindowLong(IntPtr hWnd, int nIndex)
        {
            return Imports.GetWindowLong(hWnd, nIndex);
        }

        public static RECT GetWindowRect(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(hWnd));
            }

            Imports.GetWindowRect(hWnd, out RECT Rect);

            return Rect;
        }

        public static bool IsWindow(IntPtr hWnd)
        {
            return Imports.IsWindow(hWnd);
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
            if (hWnd == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(hWnd));
            }

            Imports.SetWindowPos(hWnd, IntPtr.Zero, pt.X, pt.Y, 0, 0, Imports.SetWindowsPos.SWP_NOACTIVATE | Imports.SetWindowsPos.SWP_NOZORDER | Imports.SetWindowsPos.SWP_NOSIZE);
        }

        public static void SetWindowPos(IntPtr hWnd, HWND_INSERTAFTER hWndInsertAfter)
        {
            if (hWnd == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(hWnd));
            }

            Imports.SetWindowPos(hWnd, (IntPtr)hWndInsertAfter, 0, 0, 0, 0, Imports.SetWindowsPos.SWP_NOACTIVATE | Imports.SetWindowsPos.SWP_NOMOVE | Imports.SetWindowsPos.SWP_NOSIZE);
        }

        public static IntPtr SetWindowsHookEx(HookProc lpfn)
        {
            if (lpfn == null)
            {
                throw new ArgumentNullException(nameof(lpfn));
            }

            const int WH_MOUSE_LL = 14;
            IntPtr returnValue = Imports.SetWindowsHookEx(WH_MOUSE_LL, lpfn, IntPtr.Zero, 0);

            if (returnValue == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return returnValue;
        }

        public static bool UnhookWindowsHookEx(IntPtr hhk)
        {
            if (hhk == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(hhk));
            }

            var returnValue = Imports.UnhookWindowsHookEx(hhk);

            if (!returnValue)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return returnValue;
        }
    }
}