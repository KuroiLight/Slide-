using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Win32Api;

namespace SlideSharp
{
    internal class Coordinator
    {
        private List<Container> Containers = new List<Container>();
        private readonly DispatcherTimer Dispatcher = new DispatcherTimer();
        private readonly ConcurrentQueue<Task> HookMessages = new ConcurrentQueue<Task>();
        private POINT MStart, MEnd;
        private IntPtr MStartWindow;

        public Coordinator()
        {
            Dispatcher.Tick += UpdateStates;
            Dispatcher.Interval = new TimeSpan(0, 0, 0, 0, 16);
            Dispatcher.Start();
            Win32Api.User32.Wrapd_SetWindowsHookEx(MouseHookProc);
        }

        private void UpdateStates(object sender, EventArgs e)
        {
            Dispatcher.Stop();

            while (!HookMessages.IsEmpty) {
                var dequeSuccess = HookMessages.TryDequeue(out Task messageTask);
                if (dequeSuccess && messageTask is Task) {
                    messageTask.RunSynchronously();
                }
            }

            Containers = Containers.Where((WC) => !WC.CanBeDisposed).ToList();

            var MousePoint = WpfScreenHelper.MouseHelper.MousePosition;
            var WindowUnderMouse = Win32Api.User32.WindowFromPoint(new POINT((int)MousePoint.X, (int)MousePoint.Y));
            Containers.ForEach((WC) => {
                if (WC is EdgeContainer edge) {
                    if (WC.ContainedWindow.GetHandle() == WindowUnderMouse) {
                        edge.SetState(Status.Showing);
                    } else {
                        edge.SetState(Status.Hiding);
                    }
                }

                WC.UpdatePosition();
            });

            Dispatcher.Start();
        }

        private IntPtr MouseHookProc(int code, Win32Api.WM_MOUSE wParam, Win32Api.MSLLHOOKSTRUCT lParam)
        {
            if (wParam == Win32Api.WM_MOUSE.WM_MOUSEMOVE) {
            } else if (wParam == Win32Api.WM_MOUSE.WM_MBUTTONDOWN) {
                MStart = lParam.pt;
                MStartWindow = Win32Api.User32.WindowFromPoint(MStart);
            } else if (wParam == Win32Api.WM_MOUSE.WM_MBUTTONUP) {
                MEnd = lParam.pt;

                HookMessages.Enqueue(new Task(() => {
                    var CapturedMStart = MStart;
                    var CapturedMEnd = MEnd;
                    var CapturedMStartWindow = MStartWindow;

                    var toContainer = Containers.Find((C) => {
                        return C is EdgeContainer edge && edge.Intersect(CapturedMStart, CapturedMEnd);
                    });

                    var fromContainer = Containers.Find((C) => {
                        return C is EdgeContainer edge && edge.ContainedWindow.GetHandle() == CapturedMStartWindow;
                    });

                    if (toContainer != null) {
                        if (toContainer.ContainedWindow.Exists()) {
                            fromContainer?.RemoveWindow();
                            Containers.Add(new CenterContainer(toContainer.Screen, toContainer.ContainedWindow.GetHandle()));
                        }

                        (toContainer as EdgeContainer)?.SetNewWindow(CapturedMStartWindow);
                    }
                }));
            }

            return Win32Api.User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }
    }
}