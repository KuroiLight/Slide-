using System;
using Win32Api;

namespace SlideSharp
{
    public class WindowObj
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "GetLayeredWindowAttributes requires pre-initialize out reference to LWA_ALPHA")]
        public WindowObj(IntPtr Handle)
        {
            uint lwaa = Win32Api.Constants.LWA_ALPHA;

            this.Handle = Handle;
            WindowArea = Win32Api.User32.Wrapd_GetWindowRect(Handle);
            WindowStyle = Win32Api.User32.GetWindowLong(Handle, Win32Api.Constants.GWL_EXSTYLE);
            Win32Api.User32.GetLayeredWindowAttributes(Handle, 0, out byte trans, out lwaa);
            Transparency = trans;
            Enabled = Win32Api.User32.IsWindowEnabled(Handle);
        }

        /// <summary>
        /// RECT structure of the Window
        /// </summary>
        public RECT WindowArea { get; private set; }

        private bool Enabled { get; }
        private IntPtr Handle { get; }
        private byte Transparency { get; }
        private int WindowStyle { get; }

        /// <summary>
        /// Checks to see if window still exists
        /// </summary>
        /// <returns>true if window exists false otherwise</returns>
        public bool Exists()
        {
            return Win32Api.User32.IsWindow(Handle);
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
        /// Adds POINT pt to the windows current position
        /// </summary>
        /// <param name="pt">amount to add to windows current position</param>
        public void MoveWindow(POINT pt)
        {
            Win32Api.User32.Wrapd_SetWindowPos(Handle, WindowArea.ToPoint + pt);
            WindowArea = Win32Api.User32.Wrapd_GetWindowRect(Handle);
        }

        /// <summary>
        /// Reset the windows attributes: Transparency and Enabled state to default
        /// </summary>
        public void ResetAttributes()
        {
            Win32Api.User32.SetLayeredWindowAttributes(Handle, 0, Transparency, Win32Api.Constants.LWA_ALPHA);
            Win32Api.User32.SetWindowLong(Handle, Win32Api.Constants.GWL_EXSTYLE, WindowStyle);
            SetEnabled(Enabled);
        }

        /// <summary>
        /// Set the Enabled state of the window
        /// </summary>
        /// <param name="Enable">bool true to enable, otherwise false</param>
        public void SetEnabled(bool Enable)
        {
            Win32Api.User32.EnableWindow(Handle, Enable);
        }

        /// <summary>
        /// Set the windows absolute position
        /// </summary>
        /// <param name="pt">POINT to set the window to</param>
        public void SetPosition(POINT pt)
        {
            Win32Api.User32.Wrapd_SetWindowPos(Handle, pt);
            WindowArea = Win32Api.User32.Wrapd_GetWindowRect(Handle);
        }

        /// <summary>
        /// Set the transparency of the window
        /// </summary>
        /// <param name="percent">the transparency percent, 100 being opaque and 0 being invisible</param>
        public void SetTransparency(int percent)
        {
            Win32Api.User32.SetWindowLong(Handle, Win32Api.Constants.GWL_EXSTYLE, WindowStyle ^ Win32Api.Constants.WS_EX_LAYERED);
            byte newTrans = (byte)(255 * (percent / 100));
            Win32Api.User32.SetLayeredWindowAttributes(Handle, 0, newTrans, Win32Api.Constants.LWA_ALPHA);
        }
    }
}