using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Win32Api;

namespace SlideSharp
{
    public class Coordinator
    {
        private static Coordinator SingletonInstance = null;
        private readonly DispatcherTimer Dispatcher = new DispatcherTimer();
        private readonly ConcurrentQueue<Task> HookMessages = new ConcurrentQueue<Task>();
        private List<Container> Containers = new List<Container>();
        private IntPtr HookHandle;
        private User32.HookProc MouseHookProcHandle = null;
        private POINT MStart, MEnd;
        private IntPtr MStartWindow;

        public Coordinator()
        {
            Dispatcher.Tick += UpdateStates;
            Dispatcher.Interval = new TimeSpan(0, 0, 0, 3, 0);
            Dispatcher.Start();
            MouseHookProcHandle = MouseHookProc;
            HookHandle = User32.Wrapd_SetWindowsHookEx(MouseHookProcHandle);
        }

        internal static Coordinator GetInstance()
        {
            if (SingletonInstance == null) {
                SingletonInstance = new Coordinator();
            }
            return SingletonInstance;
        }

        private IntPtr MouseHookProc(int code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam)
        {
            if (wParam == WM_MOUSE.WM_MBUTTONDOWN) {
                MStart = lParam.pt;
                MStartWindow = User32.WindowFromPoint(MStart);
            } else if (wParam == WM_MOUSE.WM_MBUTTONUP) {
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

                    if ((toContainer?.ContainedWindow.Exists()) == true) {
                        fromContainer?.RemoveWindow();
                        Containers.Add(new CenterContainer(toContainer.Screen, toContainer.ContainedWindow.GetHandle()));
                    }

                    (toContainer as EdgeContainer)?.SetNewWindow(CapturedMStartWindow);
                }));
            }

            return User32.CallNextHookEx(HookHandle, code, wParam, lParam);
        }

        private void UpdateStates(object sender, EventArgs e)
        {
            // In the event that UpdateStates takes longer than our interval, we stop and give it some breathing room.
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
                        Debug.WriteLine($"{edge.ToString()} Showing");
                    } else {
                        edge.SetState(Status.Hiding);
                        Debug.WriteLine($"{edge.ToString()} Hiding");
                    }
                }

                WC.UpdatePosition();
            });

            Dispatcher.Start();
        }
    }
}