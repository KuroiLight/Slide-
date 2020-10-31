using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
            Dispatcher.Interval = new TimeSpan(0, 0, 0, 0, 12);
            Dispatcher.Start();
            MouseHookProcHandle = MouseHookProc;
            HookHandle = SetWindowsHookEx(MouseHookProcHandle);
        }

        ~Coordinator()
        {
            Dispatcher.Stop();
            UnhookWindowsHookEx(HookHandle);
        }

        private POINT? MStart, MEnd;
        private readonly object MouseDataLock = new object();

        private (POINT?, POINT?) MiddleMouseData
        {
            get
            {
                lock (MouseDataLock) {
                    return (MStart, MEnd);
                }
            }
            set
            {
                lock (MouseDataLock) {
                    (MStart, MEnd) = value;
                }
            }
        }

        private IntPtr MouseHookProc(int code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam)
        {
            if (wParam == WM_MOUSE.WM_MBUTTONDOWN) {
                lock (MouseDataLock) {
                    MStart = lParam.pt;
                }
            } else if (wParam == WM_MOUSE.WM_MBUTTONUP) {
                lock (MouseDataLock) {
                    MEnd = lParam.pt;
                }
            }

            return CallNextHookEx(HookHandle, code, wParam, lParam);
        }

        private void UpdateStates(object sender, EventArgs e)
        {
            // In the event that UpdateStates takes longer than our interval, we stop and give it some breathing room.
            Dispatcher.Stop();

            IntPtr? WindowUnderlMEnd = null;
            (POINT? lMStart, POINT? lMEnd) = MiddleMouseData;
            if (lMEnd != null && lMStart != null) {
                WindowUnderlMEnd = GetRootWindowIf((POINT)lMStart, (hwnd) => GetTitleBarInfo(hwnd).rcTitleBar.Contains((POINT)lMStart));
                MiddleMouseData = (null, null);
            }

            POINT MousePos = GetCursorPos();
            var WindowUnderMouse = GetRootWindow(MousePos);
            WindowSlider toSlider = null, centerSlider = null, fromSlider = null;

            Sliders.AsParallel().ForAll((Slider) => {
                if (Slider.Window != null) {
                    if (Slider.Window.GetHandle() == WindowUnderMouse) {
                        Slider.Assign(Status.Showing);
                    } else {
                        Slider.Assign(Status.Hiding);
                    }
                    Slider.UpdatePosition();
                }

                if (WindowUnderlMEnd != null) {
                    if (Slider.WillIntersect((POINT)lMStart, (POINT)lMEnd)) {
                        toSlider = Slider;
                    }

                    if (Slider.Direction == Direction.Center && Slider.Screen.Contains((POINT)lMStart)) {
                        centerSlider = Slider;
                    }

                    if (Slider.Window?.GetHandle() == WindowUnderlMEnd) {
                        fromSlider = Slider;
                        Slider.Assign(IntPtr.Zero);
                    }
                }
            });

            if (WindowUnderlMEnd != null) {
                if (toSlider?.Window?.Exists() == true && centerSlider != null) {
                    centerSlider?.Assign(toSlider.Window.GetHandle());
                }
                if (fromSlider != null && toSlider == null) {
                    centerSlider?.Assign((IntPtr)WindowUnderlMEnd);
                } else {
                    if (toSlider?.Window?.Exists() == true && centerSlider != null) {
                        centerSlider?.Assign(toSlider.Window.GetHandle());
                    }
                    toSlider?.Assign((IntPtr)WindowUnderlMEnd);
                }
            }

            Dispatcher.Start();
        }
    }
}