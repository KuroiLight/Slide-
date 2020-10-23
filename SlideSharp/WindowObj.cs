using System;

namespace SlideSharp
{
    public class WindowObj
    {
        public WindowObj(IntPtr Handle)
        {
            handle = Handle;
            WindowArea = Win32Api.User32.Wrapd_GetWindowRect(Handle);
            //get transparency, enabled state
        }

        public RECT WindowArea { get; private set; }
        private bool enabled { get; set; }
        private IntPtr handle { get; set; }
        private int transparency { get; set; }
        /// <summary>
        /// Checks to see if window still exists
        /// </summary>
        /// <returns>true if window exists false otherwise</returns>
        public bool Exists()
        {
            return Win32Api.User32.IsWindow(handle);
        }

        /// <summary>
        /// Returns the windows handle
        /// </summary>
        /// <returns>IntPtr handle</returns>
        public IntPtr GetHandle()
        {
            return handle;
        }

        /// <summary>
        /// Adds POINT pt to the windows current position
        /// </summary>
        /// <param name="pt">amount to add to windows current position</param>
        public void MoveWindow(POINT pt)
        {
            Win32Api.User32.Wrapd_SetWindowPos(handle, WindowArea.ToPoint() + pt);
            WindowArea = Win32Api.User32.Wrapd_GetWindowRect(handle);
        }

        /// <summary>
        /// Set the windows absolute position
        /// </summary>
        /// <param name="pt">POINT to set the window to</param>
        public void SetPosition(POINT pt)
        {
            Win32Api.User32.Wrapd_SetWindowPos(handle, pt);
            WindowArea = Win32Api.User32.Wrapd_GetWindowRect(handle);
        }
    }
}
