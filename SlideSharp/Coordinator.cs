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
        private List<Container> Containers;
        private readonly IntPtr HookHandle;
        private readonly User32.HookProc MouseHookProcHandle = null;
        private POINT MStart;
        private IntPtr MStartWindow;

        public Coordinator()
        {
            Containers = EdgeContainer.GetAllValidInstances().ToList<Container>();
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
            return SingletonInstance ??= new Coordinator();
        }

        private IntPtr MouseHookProc(int code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam)
        {
            if (wParam == WM_MOUSE.WM_MBUTTONDOWN) {
                MStart = lParam.pt;
                MStartWindow = User32.GetParentWindowFromPoint(MStart);
            } else if (wParam == WM_MOUSE.WM_MBUTTONUP) {
                var CapturedMStart = MStart;
                var CapturedMEnd = lParam.pt;
                var CapturedMStartWindow = MStartWindow;

                if (MStartWindow != null && lParam.pt != MStart) {
                    HookMessages.Enqueue(new Task(() => {
                        Container toContainer = Containers.Find((C) => C is EdgeContainer edge && edge.Intersect(CapturedMStart, CapturedMEnd));

                        Container fromContainer = Containers.Find((C) => C is EdgeContainer edge && edge.ContainedWindow?.GetHandle() == CapturedMStartWindow);
                        if(toContainer != null) {
                            Debug.WriteLine($"{((EdgeContainer)toContainer).Direction}");
                        }
                        
                        if ((toContainer?.ContainedWindow?.Exists()) == true) {
                            
                            fromContainer?.RemoveWindow();
                            Containers.Add(new CenterContainer(toContainer.Screen, toContainer.ContainedWindow.GetHandle()));
                        }

                        (toContainer as EdgeContainer)?.SetNewWindow(CapturedMStartWindow);
                    }));
                }
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
            var WindowUnderMouse = Win32Api.User32.GetParentWindowFromPoint(new POINT((int)MousePoint.X, (int)MousePoint.Y));
            Containers.ForEach((WC) => {
                if (WC.ContainedWindow?.Exists() == true) {
                    if (WC is EdgeContainer edge) {
                        Debug.Write($"{edge} {edge.Status} => ");
                        if (WC.ContainedWindow.GetHandle() == WindowUnderMouse) {
                            edge.SetState(Status.Showing);
                        } else {
                            edge.SetState(Status.Hiding);
                        }
                        Debug.WriteLine($"{edge.Status}.");
                    }

                    WC.UpdatePosition();
                }
            });

            Dispatcher.Start();
        }
    }
}