using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Win32Api;
using static Win32Api.Imports;

namespace SlideSharp
{
    public class MouseHook
    {
        private readonly HookProc _mouseHookProcHandle;
        private readonly IntPtr _hookHandle;

        public MouseHook(HookProc hookProc)
        {
            _mouseHookProcHandle = hookProc;
            _hookHandle = User32.SetWindowsHookEx(_mouseHookProcHandle);
            if (_hookHandle == IntPtr.Zero) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        ~MouseHook()
        {
            User32.UnhookWindowsHookEx(_hookHandle);
        }
    }
}
