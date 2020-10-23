using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SlideSharp
{
    internal class Coordinator
    {
        private List<WindowContainer> Containers = new List<WindowContainer>();
        private ConcurrentQueue<Task> HookMessages = new ConcurrentQueue<Task>();
        //Implement async task to run UpdateStates in a loop

        private void UpdateStates()
        {
            //process Hook messages
            while (!HookMessages.IsEmpty) {
                var dequeSuccess = HookMessages.TryDequeue(out Task messageTask);
                if (dequeSuccess && messageTask is Task) {
                    messageTask.RunSynchronously();
                }
            }

            //remove dead containers
            Containers = Containers.Where((WC) => WC.CanBeDisposed == false).ToList();

            //update container positions and statuses
            var MousePoint = WpfScreenHelper.MouseHelper.MousePosition;
            var WindowUnderMouse = Win32Api.User32.WindowFromPoint(new POINT((int)MousePoint.X, (int)MousePoint.Y));
            Containers.ForEach((WC) => {
                if (WC is DockedWindow) {
                    if (WC.ContainedWindow.GetHandle() == WindowUnderMouse) {
                        ((DockedWindow)WC).SetState(Status.Showing);
                    } else {
                        ((DockedWindow)WC).SetState(Status.Hiding);
                    }
                }

                WC.UpdatePosition();
            });
        }
    }
}