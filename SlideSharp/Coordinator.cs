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

        public Coordinator()
        {
            Dispatcher.Tick += UpdateStates;
            Dispatcher.Interval = new TimeSpan(0, 0, 0, 0, 16);
            Dispatcher.Start();
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
                if (WC is EdgeContainer container) {
                    if (WC.ContainedWindow.GetHandle() == WindowUnderMouse) {
                        container.SetState(Status.Showing);
                    } else {
                        container.SetState(Status.Hiding);
                    }
                }

                WC.UpdatePosition();
            });

            Dispatcher.Start();
        }
    }
}