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
        private readonly Windows windows = new Windows();
        private readonly DispatcherTimer Dispatcher = new DispatcherTimer();
        private POINT MStart;

        public Coordinator()
        {
            Dispatcher.Tick += UpdateStates;
            Dispatcher.Interval = new TimeSpan(0, 0, 0, 0, 16);
            Dispatcher.Start();

            _mouseHook = new MouseHook(MouseHookProc);
        }

        private IntPtr MouseHookProc(int code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam)
        {
            if (wParam == WM_MOUSE.WM_MBUTTONDOWN) {
                MStart = lParam.pt;
            } else if (wParam == WM_MOUSE.WM_MBUTTONUP) {
                windows.SetRay(new Ray((POINT)MStart, MStart - lParam.pt));
            }


            return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        ~Coordinator()
        {
            windows.Dispose();
            Dispatcher.Stop();
        }

        private void UpdateStates(object sender, EventArgs e)
        {
            Dispatcher.Stop();

            windows.UpdateWindows();

            Dispatcher.Start();
        }
    }
}