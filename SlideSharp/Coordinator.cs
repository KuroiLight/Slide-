using System;
using System.Collections.Generic;
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
            Dispatcher.Interval = new TimeSpan(0, 0, 0, 0, 16);
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
                WindowUnderlMEnd = GetRootWindow((POINT)lMStart);
                MiddleMouseData = (null, null);
            }

            POINT MousePos = GetCursorPos();
            var WindowUnderMouse = GetRootWindow(MousePos);
            WindowSlider toSlider = null, centerSlider = null;

            Sliders.ForEach((Slider) => {
                if (Slider.Window?.GetHandle() == WindowUnderMouse) {
                    Slider.SetState(Status.Showing).UpdatePosition();
                } else {
                    Slider.SetState(Status.Hiding).UpdatePosition();
                }

                if (Slider.Window?.GetHandle() == WindowUnderlMEnd) {
                    Slider.SetWindow(IntPtr.Zero);
                }

                if (WindowUnderlMEnd != null) {
                    if (Slider.WillIntersect((POINT)lMStart, (POINT)lMEnd)) {
                        toSlider = Slider;
                    }

                    if (Slider.Direction == Direction.Center && Slider.Screen.Contains((POINT)lMStart)) {
                        centerSlider = Slider;
                    }
                }
            });

            if (WindowUnderlMEnd != null) {
                if (toSlider?.Window?.Exists() == true && centerSlider != null) {
                    centerSlider?.SetWindow(toSlider.Window.GetHandle());
                }
                toSlider?.SetWindow((IntPtr)WindowUnderlMEnd);
            }

            Dispatcher.Start();
        }
    }
}