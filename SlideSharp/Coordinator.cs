using System;
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
        private readonly FixedList<BoxedWindow> Windows;

        public Coordinator()
        {
            Windows = new FixedList<BoxedWindow>(Screen.AllScreens.Count() * 5);

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

            BoxedWindow newWindow = HasNewWindow();
            IntPtr WindowAtCursor = GetRootWindow(GetCursorPos());

            Windows.RemoveAll((Window) => {
                if (Window.Slide is CenterSlide && Window.FinishedMoving()) return true;
                if (newWindow != null && newWindow.hWnd == Window.hWnd) return true;
                if (!User32.IsWindow(Window.hWnd)) return true;
                return false;
            });

            Windows.ForEach((Window) => {
                if (newWindow != null && Window.Slide == newWindow.Slide) Window.Slide = new CenterSlide(Window.Slide.Screen);
                Window.SetStatus(Window.hWnd == WindowAtCursor ? Status.Showing : Status.Hiding);
                Window.Move();
            });

            if (newWindow != null) {
                newWindow.SetStatus(Status.Hiding);
                Windows.Add(newWindow);
            }

            Dispatcher.Start();
        }

        private BoxedWindow HasNewWindow()
        {
            Ray localRay = Ray;
            Ray = null;
            if (localRay == null) return null;

            IntPtr RootWindowAtCursorTitlebar = GetRootWindowFromTitlebar(localRay.Position);
            if (RootWindowAtCursorTitlebar == IntPtr.Zero) return null;

            return new BoxedWindow(RootWindowAtCursorTitlebar, SlideFactory.SlideFromRay(localRay));
        }
    }
}