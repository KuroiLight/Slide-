using System;
using Win32Api;
using static Win32Api.Imports;
using static Win32Api.User32;

namespace SlideSharp
{
    public class WindowObj
    {
        public RECT Rect { get; private set; }
        public IntPtr Handle { get; }
        public bool TopMost { get; }

        public WindowObj(IntPtr handle)
        {
            Handle = handle;
            Rect = GetWindowRect(Handle);
            TopMost = (GetWindowLong(Handle, GWL_EXSTYLE) & WS_EX_TOPMOST) == WS_EX_TOPMOST;
        }

        public void UpdateRect() => Rect = GetWindowRect(Handle);

        public bool Exists() => IsWindow(Handle);

        public void SetTopMost(bool topmost) => SetWindowPos(Handle, topmost ? HWND_INSERTAFTER.HWND_TOPMOST : HWND_INSERTAFTER.HWND_NOTOPMOST);

        public void SetPosition(POINT pt) => SetWindowPos(Handle, pt);
    }
}