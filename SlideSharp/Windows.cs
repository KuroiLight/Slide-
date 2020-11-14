using System;
using System.Linq;
using System.Threading;
using Win32Api;
using WpfScreenHelper;
using static Win32Api.User32;

namespace SlideSharp
{
    public class Windows
    {
        private readonly FixedList<BoxedWindow> AllWindows;
        private Ray? _ray;
        public Windows()
        {
            AllWindows = new FixedList<BoxedWindow>(Screen.AllScreens.Count() * 5);
        }

        public void Dispose()
        {
            AllWindows.ForEach(W => {
                W.Slide = new CenterSlide(W.Slide);
                W.SetStatus(Status.Showing);
                while (!W.FinishedMoving()) {
                    W.Move();
                }
            });
            AllWindows.Clear();
        }
        public void SetRay(Ray ray)
        {
            Interlocked.Exchange(ref _ray, ray);
        }

        public void UpdateWindows()
        {
            HandleNewWindow();

            IntPtr WindowAtCursor = GetRootWindow(GetCursorPos());

            AllWindows.ForEachAt((Window, ind) => {

                if (!User32.IsWindow(Window.hWnd) || (Window.Slide is CenterSlide && Window.FinishedMoving())) {
                    Window.SetStatus(Status.Showing);
                    AllWindows.RemoveAt(ind);
                    return;
                }
                Window.SetStatus(Window.hWnd == WindowAtCursor ? Status.Showing : Status.Hiding);
                Window.Move();

            });
        }

        private void HandleNewWindow()
        {
            BoxedWindow newWindow = WindowFromRay();

            if (newWindow != null) {
                AllWindows.ForEachAt((W, i) => {
                    if (newWindow.hWnd == W.hWnd) {
                        W.SetStatus(Status.Showing);
                        AllWindows.RemoveAt(i);
                        return;
                    }
                    if (W.Slide == newWindow.Slide) {
                        W.Slide = new CenterSlide(W.Slide);
                    }
                });

                newWindow.SetStatus(Status.Hiding);
                AllWindows.Add(newWindow);
            }
        }

        private BoxedWindow WindowFromRay()
        {
            Ray ray = _ray;
            if (ray == null) return null;
            _ray = null;

            IntPtr RootWindowAtCursorTitlebar = GetRootWindowFromTitlebar(ray.Position);
            if (RootWindowAtCursorTitlebar == IntPtr.Zero) return null;

            return new BoxedWindow(RootWindowAtCursorTitlebar, SlideFactory.SlideFromRay(ray));
        }
    }
}
