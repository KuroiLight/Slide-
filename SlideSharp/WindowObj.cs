using System;
using System.Diagnostics;
using Win32Api;
using static Win32Api.Imports;
using static Win32Api.User32;

namespace SlideSharp
{
    public class WindowObj
    {
        public WindowObj(IntPtr Handle)
        {
            this.Handle = Handle;
            Rect = GetWindowRect(Handle);
            TopMost = (GetWindowLong(Handle, GWL_EXSTYLE) & WS_EX_TOPMOST) == WS_EX_TOPMOST;
            SetTopMost(true);
        }

        /// <summary>
        /// RECT structure of the Window
        /// </summary>
        public RECT Rect { get; private set; }
        private IntPtr Handle { get; }
        internal bool TopMost { get; }

        /// <summary>
        /// Checks to see if window still exists
        /// </summary>
        /// <returns>true if window exists false otherwise</returns>
        public bool Exists()
        {
            return IsWindow(Handle);
        }

        /// <summary>
        /// Returns the windows handle
        /// </summary>
        /// <returns>IntPtr handle</returns>
        public IntPtr GetHandle()
        {
            return Handle;
        }

        /// <summary>
        /// Sets the windows Topmost attribute
        /// </summary>
        /// <param name="topmost">true for HWND_TOPMOST, false for HWND_NOTOPMOST</param>
        internal void SetTopMost(bool topmost)
        {
            SetWindowPos(Handle, topmost ? HWND_INSERTAFTER.HWND_TOPMOST : HWND_INSERTAFTER.HWND_NOTOPMOST);
        }

        /// <summary>
        /// Set the windows absolute position
        /// </summary>
        /// <param name="pt">POINT to set the window to</param>
        public void SetPosition(POINT pt)
        {
            SetWindowPos(Handle, pt);
            Rect = GetWindowRect(Handle); // update new rect locally
        }
    }
}