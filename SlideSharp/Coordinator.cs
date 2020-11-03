using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Win32Api;
using WpfScreenHelper;
using static Win32Api.Imports;
using static Win32Api.User32;

namespace SlideSharp
{
    public class Coordinator
    {
        private readonly DispatcherTimer Dispatcher = new DispatcherTimer();
        private readonly IntPtr HookHandle;
        private readonly object MouseDataLock = new object();
        private readonly HookProc MouseHookProcHandle;

        private Ray? _ray;

        private POINT? MStart;
        private readonly List<SlidingWindow> Windows;

        public Coordinator()
        {
            Windows = new List<SlidingWindow>(Screen.AllScreens.Count() * 5);
            Dispatcher.Tick += UpdateStates;
            Dispatcher.Interval = new TimeSpan(0, 0, 0, 0, MainWindow.config.Update_Interval);
            Dispatcher.Start();
            MouseHookProcHandle = MouseHookProc;
            HookHandle = SetWindowsHookEx(MouseHookProcHandle);
        }

        private Ray? Ray
        {
            get
            {
                lock (MouseDataLock)
                {
                    return _ray;
                }
            }
            set
            {
                lock (MouseDataLock)
                {
                    _ray = value;
                }
            }
        }

        ~Coordinator()
        {
            Dispatcher.Stop();
            UnhookWindowsHookEx(HookHandle);
        }

        private IntPtr MouseHookProc(int code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam)
        {
            if (wParam == WM_MOUSE.WM_MBUTTONDOWN)
                MStart = lParam.pt;
            else if (wParam == WM_MOUSE.WM_MBUTTONUP)
                if (MStart != null)
                {
                    Ray = new Ray((POINT) MStart, lParam.pt);
                    MStart = null;
                }

            return CallNextHookEx(HookHandle, code, wParam, lParam);
        }

        private void UpdateStates(object sender, EventArgs e)
        {
            Dispatcher.Stop();

            var WindowUnderlMEnd = IntPtr.Zero;
            Ray localRay = default;
            if (Ray.HasValue)
            {
                localRay = Ray.Value;
                WindowUnderlMEnd = GetRootWindowIf(localRay.Position,
                    hwnd => GetTitleBarInfo(hwnd).rcTitleBar.Contains(localRay.Position));
                Ray = null;
            }

            var WindowUnderCursor = GetRootWindow(GetCursorPos());
            var MousePos = GetCursorPos();


            Windows.AsParallel().ForAll(Window =>
            {
                if (Window.HasWindow(WindowUnderCursor))
                    Window.SetWindowState(Status.Showing);
                else
                    Window.SetWindowState(Status.Hiding);

                if (Window.HasWindow(WindowUnderlMEnd))
                    Window.MarkForDeletion();
                else
                    Window.MoveNextStep();
            });

            if (WindowUnderlMEnd != IntPtr.Zero)
            {
                var toSlider = SlidingWindow.CreateFromRay(localRay);
                toSlider.ManageWindow(WindowUnderlMEnd);
                Windows.Add(toSlider);
            }

            Windows.RemoveAll(window => window.MarkedForDeletion);

            Dispatcher.Start();
        }
    }
}