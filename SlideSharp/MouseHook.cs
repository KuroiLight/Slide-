using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using Win32Api;
using static Win32Api.Imports;

namespace SlideSharp
{
    public class MouseHook : IDisposable
    {
        private HookProc? _mouseHookProcHandle;
        private IntPtr _hookHandle;
        private bool disposedValue;

        public MouseHook(HookProc hookProc)
        {
            _mouseHookProcHandle = hookProc;
            _hookHandle = User32.SetWindowsHookEx(_mouseHookProcHandle);
            if (_hookHandle == IntPtr.Zero)
            {
                MessageBox.Show($"{new Win32Exception(Marshal.GetLastWin32Error()).Message}\nwith no MouseHook application must now close.");
                Application.Current.Shutdown();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                User32.UnhookWindowsHookEx(_hookHandle);
                if (disposing)
                {
                    _mouseHookProcHandle = null;
                    _hookHandle = IntPtr.Zero;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}