using System;
using System.Windows.Threading;
using Win32Api;
using static Win32Api.Imports;
using static Win32Api.User32;

namespace SlideSharp
{
    public class Coordinator
    {
        private readonly MouseHook _mouseHook;
        private readonly Windows windows = new();
        private readonly DispatcherTimer Dispatcher = new();
        private POINT MStart; private IntPtr MStartWindow;

        public Coordinator()
        {
            Dispatcher.Tick += UpdateStates;
            Dispatcher.Interval = new TimeSpan(0, 0, 0, 0, 16);
            Dispatcher.Start();

            _mouseHook = new MouseHook(MouseHookProc);
        }

        private IntPtr MouseHookProc(int code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam)
        {
            if (wParam == WM_MOUSE.WM_MBUTTONDOWN)
            {
                MStartWindow = User32.GetRootWindowFromTitlebar(lParam.pt);
                MStart = lParam.pt;
            }
            else if (wParam == WM_MOUSE.WM_MBUTTONUP && MStartWindow != IntPtr.Zero)
            {
                var s = SlideFactory.SlideFromMouseMovement(MStart, lParam.pt);
                var bw = new BoxedWindow(MStartWindow, s);
                windows.SetNewWindow(bw);
            }

            return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        ~Coordinator()
        {
            windows.Dispose();
            Dispatcher.Stop();
        }

        private void UpdateStates(object? sender, EventArgs e)
        {
            Dispatcher.Stop();

            windows.UpdateWindows();

            Dispatcher.Start();
        }
    }
}