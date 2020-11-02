using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using Win32Api;
using static Win32Api.Imports;
using static Win32Api.User32;

namespace SlideSharp
{
    public class Coordinator
    {
        private readonly DispatcherTimer Dispatcher = new DispatcherTimer();
        private readonly List<WindowSlider> Sliders;
        private readonly IntPtr HookHandle;
        private readonly HookProc MouseHookProcHandle = null;

        public Coordinator()
        {
            Sliders = WindowSlider.GetAllValidInstances().ToList<WindowSlider>();
            Dispatcher.Tick += UpdateStates;
            Dispatcher.Interval = new TimeSpan(0, 0, 0, 0, MainWindow.config.Update_Interval);
            Dispatcher.Start();
            MouseHookProcHandle = MouseHookProc;
            HookHandle = SetWindowsHookEx(MouseHookProcHandle);
        }

        ~Coordinator()
        {
            Dispatcher.Stop();
            UnhookWindowsHookEx(HookHandle);
        }

        private Ray? _ray;

        private Ray? Ray
        {
            get
            {
                lock (MouseDataLock) {
                    return _ray;
                }
            }
            set
            {
                lock (MouseDataLock) {
                    _ray = value;
                }
            }
        }

        private POINT? MStart;
        private readonly object MouseDataLock = new object();

        private IntPtr MouseHookProc(int code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam)
        {
            if (wParam == WM_MOUSE.WM_MBUTTONDOWN) {
                MStart = lParam.pt;
            } else if (wParam == WM_MOUSE.WM_MBUTTONUP) {
                if (MStart != null) {
                    Ray = new Ray((POINT)MStart, lParam.pt);
                    MStart = null;
                }
            }

            return CallNextHookEx(HookHandle, code, wParam, lParam);
        }

        private void UpdateStates(object sender, EventArgs e)
        {
            // In the event that UpdateStates takes longer than our interval, we stop and give it some breathing room.
            Dispatcher.Stop();

            IntPtr WindowUnderlMEnd = IntPtr.Zero;
            Ray localRay = default;
            if (Ray.HasValue) {
                localRay = Ray.Value;
                WindowUnderlMEnd = GetRootWindowIf(localRay.Position, (hwnd) => GetTitleBarInfo(hwnd).rcTitleBar.Contains(localRay.Position));
                Ray = null;
            }

            POINT MousePos = GetCursorPos();
            var WindowUnderMouse = GetRootWindow(MousePos);
            WindowSlider toSlider = null, centerSlider = null, fromSlider = null;

            Sliders.AsParallel().ForAll((Slider) => {
                if (Slider.Window != null) {
                    if (Slider.Window?.Handle == WindowUnderMouse) {
                        Slider.AssignStatus(Status.Showing);
                    } else {
                        Slider.AssignStatus(Status.Hiding);
                    }
                    Slider.UpdatePosition();
                }

                if (WindowUnderlMEnd != IntPtr.Zero) {
                    if (Slider.WillIntersect(localRay)) {
                        toSlider = Slider;
                    }

                    if (Slider.Direction == Direction.Center && Slider.Screen.Contains(localRay.Position)) {
                        centerSlider = Slider;
                    }

                    if (Slider.Window?.Handle == WindowUnderlMEnd) {
                        Slider.UnAssignWindow();
                    }
                }
            });

            if (WindowUnderlMEnd != IntPtr.Zero) {
                if (toSlider != null) {
                    if (toSlider.Window != null) {
                        centerSlider?.AssignWindow(toSlider.Window);
                    }
                    toSlider.AssignWindow(WindowUnderlMEnd);
                }
            }

            if (toSlider != null || fromSlider != null || centerSlider != null) {
                Debug.WriteLine($"{toSlider?.Direction}={toSlider?.Window?.Handle}, {fromSlider?.Direction}={fromSlider?.Window?.Handle}, {centerSlider?.Direction}={centerSlider?.Window?.Handle}");
            }

            Dispatcher.Start();
        }
    }
}