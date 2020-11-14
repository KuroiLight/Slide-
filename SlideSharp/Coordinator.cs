using System;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Win32Api;
using WpfScreenHelper;
using static Win32Api.Imports;
using static Win32Api.User32;

namespace SlideSharp
{
    public class Coordinator
    {
        private readonly MouseHook _mouseHook;
        #nullable enable
        private Ray? _ray;
        #nullable disable
        private readonly DispatcherTimer Dispatcher = new DispatcherTimer();
        private readonly object MouseDataLock = new object();
        private POINT MStart;
        private readonly FixedList<BoxedWindow> Windows;

        public Coordinator()
        {
            Windows = new FixedList<BoxedWindow>(Screen.AllScreens.Count() * 5);

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
                Interlocked.Exchange(ref _ray, new Ray((POINT)MStart, MStart - lParam.pt));
            }


            return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        ~Coordinator()
        {
            Dispatcher.Stop();
        }

        private void UpdateStates(object sender, EventArgs e)
        {
            Dispatcher.Stop();

            UpdateBoxedWindows();

            Dispatcher.Start();
        }

        private void UpdateBoxedWindows()
        {
            IntPtr WindowAtCursor = GetRootWindow(GetCursorPos());
            BoxedWindow newWindow = GetNewWindow();

            if (newWindow != null) {
                Windows.ForEachAt((W, i) => {
                    if (newWindow.hWnd == W.hWnd) {
                        Windows.RemoveAt(i);
                        return;
                    }
                    if (W.Slide == newWindow.Slide) {
                        W.Slide = new CenterSlide(W.Slide.Screen);
                    }
                });

                newWindow.SetStatus(Status.Hiding);
                Windows.Add(newWindow);
            }

            Windows.ForEachAt((Window, ind) => {

                if (!User32.IsWindow(Window.hWnd) || (Window.Slide is CenterSlide && Window.FinishedMoving())) {
                    Windows.RemoveAt(ind);
                    return;
                }
                Window.SetStatus(Window.hWnd == WindowAtCursor ? Status.Showing : Status.Hiding);
                Window.Move();

            });
        }

        private BoxedWindow GetNewWindow()
        {
            Ray localRay = Interlocked.Exchange(ref _ray, null);
            if (localRay == null) return null;

            IntPtr RootWindowAtCursorTitlebar = GetRootWindowFromTitlebar(localRay.Position);
            if (RootWindowAtCursorTitlebar == IntPtr.Zero) return null;

            return new BoxedWindow(RootWindowAtCursorTitlebar, SlideFactory.SlideFromRay(localRay));
        }
    }
}