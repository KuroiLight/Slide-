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
        private readonly object MouseDataLock = new object();
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

        private readonly MouseHook _mouseHook;

        private POINT MStart;
        private readonly List<SlidingWindow> Windows;

        public Coordinator()
        {
            Windows = new List<SlidingWindow>(Screen.AllScreens.Count() * 5);

            Dispatcher.Tick += UpdateStates;
            Dispatcher.Interval = new TimeSpan(0, 0, 0, 0, Configuration.SettingsInstance.Update_Interval);
            Dispatcher.Start();

            _mouseHook = new MouseHook((int code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam) => {
                if (wParam == WM_MOUSE.WM_MBUTTONDOWN) {
                    MStart = lParam.pt;
                } else if (wParam == WM_MOUSE.WM_MBUTTONUP) {
                    Ray = new Ray((POINT)MStart, MStart - lParam.pt);
                }


                return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            });
        }

        ~Coordinator()
        {
            Dispatcher.Stop();
        }

        private void UpdateStates(object sender, EventArgs e)
        {
            Dispatcher.Stop();

            Ray localRay = Ray;
            Ray = null;

            if (localRay != null) {
                IntPtr capturedWindow;

                if ((capturedWindow = GetRootWindowFromTitlebar(localRay.Position)) != IntPtr.Zero) {
                    Windows.Find(window => window.HasWindow(capturedWindow)).MarkForDeletion();
                    var toSlider = SlidingWindow.CreateFromRay(localRay);
                    toSlider.ManageWindow(capturedWindow);
                    Windows.Add(toSlider);
                }
            }

            Windows.RemoveAll(window => window.MarkedForDeletion);

            var WindowUnderCursor = GetRootWindow(GetCursorPos());

            Windows.ForEach(Window => {
                if (Window.HasWindow(WindowUnderCursor))
                    Window.SetWindowState(Status.Showing);
                else
                    Window.SetWindowState(Status.Hiding);

                Window.MoveNextStep();
            });

            Dispatcher.Start();
        }
    }
}