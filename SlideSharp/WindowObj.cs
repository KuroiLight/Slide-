using System;
using Win32Api;
using static Win32Api.Imports;
using static Win32Api.User32;

namespace SlideSharp
{
    public class WindowObj
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "GetLayeredWindowAttributes requires pre-initialize out reference to LWA_ALPHA")]
        public WindowObj(IntPtr Handle)
        {
            uint lwaa = LWA_ALPHA;

            this.Handle = Handle;
            WindowArea = GetWindowRect(Handle);
            WindowStyle = GetWindowLong(Handle, GWL_EXSTYLE);
            GetLayeredWindowAttributes(Handle, 0, out byte trans, out lwaa);
            Transparency = trans;
            Enabled = IsWindowEnabled(Handle);
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
        /// Adds POINT pt to the windows current position
        /// </summary>
        /// <param name="pt">amount to add to windows current position</param>
        public void MoveWindow(POINT pt)
        {
            SetWindowPos(Handle, WindowArea.ToPoint + pt);
            WindowArea = GetWindowRect(Handle);
        }

        /// <summary>
        /// Reset the windows attributes: Transparency and Enabled state to default
        /// </summary>
        public void ResetAttributes()
        {
            SetLayeredWindowAttributes(Handle, 0, Transparency, LWA_ALPHA);
            SetWindowLong(Handle, GWL_EXSTYLE, WindowStyle);
            SetEnabled(Enabled);
        }

        /// <summary>
        /// Set the Enabled state of the window
        /// </summary>
        /// <param name="Enable">bool true to enable, otherwise false</param>
        public void SetEnabled(bool Enable)
        {
            EnableWindow(Handle, Enable);
        }

        /// <summary>
        /// Set the windows absolute position
        /// </summary>
        /// <param name="pt">POINT to set the window to</param>
        public void SetPosition(POINT pt)
        {
            SetWindowPos(Handle, pt);
            WindowArea = GetWindowRect(Handle);
        }

        /// <summary>
        /// Set the transparency of the window
        /// </summary>
        /// <param name="percent">the transparency percent, 100 being opaque and 0 being invisible</param>
        public void SetTransparency(int percent)
        {
            SetWindowLong(Handle, GWL_EXSTYLE, WindowStyle ^ WS_EX_LAYERED);
            byte newTrans = (byte)(255 * (percent / 100));
            SetLayeredWindowAttributes(Handle, 0, newTrans, LWA_ALPHA);
        }
    }
}