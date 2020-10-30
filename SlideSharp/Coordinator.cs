using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Win32Api;

namespace SlideSharp
{
    public class Coordinator
    {
        private static Coordinator SingletonInstance = null;
        private readonly DispatcherTimer Dispatcher = new DispatcherTimer();

        private List<WindowSlider> Sliders;
        private readonly IntPtr HookHandle;
        private readonly User32.HookProc MouseHookProcHandle = null;

        private IntPtr MStartWindow;

        public Coordinator()
        {
            if (SingletonInstance == null) {
                SingletonInstance = this;
            } else {
                throw new Exception("Singleton already initialized.");
            }

            Sliders = WindowSlider.GetAllValidInstances().ToList<WindowSlider>();
            Dispatcher.Tick += UpdateStates;
            Dispatcher.Interval = new TimeSpan(0, 0, 0, 0, 16);
            Dispatcher.Start();
            MouseHookProcHandle = MouseHookProc;
            HookHandle = User32.Wrapd_SetWindowsHookEx(MouseHookProcHandle);
        }

        ~Coordinator()
        {
            User32.Wrapd_UnhookWindowsHookEx(HookHandle);
        }

        internal static Coordinator GetInstance()
        {
            return SingletonInstance != null ? SingletonInstance : new Coordinator();
        }

        private POINT? MStart, MEnd;
        private readonly object MouseDataLock = new object();

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

            return User32.CallNextHookEx(HookHandle, code, wParam, lParam);
        }

        private void UpdateStates(object sender, EventArgs e)
        {
            // In the event that UpdateStates takes longer than our interval, we stop and give it some breathing room.
            Dispatcher.Stop();

            POINT? lMStart = null, lMEnd = null;
            lock (MouseDataLock) {
                if (MStart != null && MEnd != null) {
                    (lMStart, MStart) = (MStart, lMStart);
                    (lMEnd, MEnd) = (MEnd, lMEnd);
                }
            }

            var CurMousePosition = new POINT(WpfScreenHelper.MouseHelper.MousePosition.X, WpfScreenHelper.MouseHelper.MousePosition.Y);
            var WindowUnderMouse = Win32Api.User32.GetParentWindowFromPoint(CurMousePosition);
            IntPtr? WindowUnderlMEnd = null;

            if (lMEnd != null && lMStart != null) {
                WindowUnderlMEnd = Win32Api.User32.GetParentWindowFromPoint((POINT)lMStart);
            }

            WindowSlider toSlider = null, centerSlider = null;

            Sliders.ForEach((Slider) => {
                if (Slider.Window != null) {
                    if (Slider.Window.GetHandle() == WindowUnderMouse) {
                        Slider.SetState(Status.Showing);
                    } else {
                        Slider.SetState(Status.Hiding);
                    }
                    Slider.UpdatePosition();

                    if (Slider.Window.GetHandle() == WindowUnderlMEnd) {
                        Slider.SetWindow(IntPtr.Zero);
                    }
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

            if (WindowUnderlMEnd != null && toSlider != null) {
                if (toSlider.Window != null && toSlider.Window.Exists() && centerSlider != null) {
                    centerSlider.SetWindow(toSlider.Window.GetHandle());
                }
                toSlider.SetWindow((IntPtr)WindowUnderlMEnd);
            }

            Dispatcher.Start();
        }
    }
}