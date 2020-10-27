using System;
using System.Collections.Generic;
using System.Text;
using Win32Api;
using WpfScreenHelper;

namespace SlideSharp
{
    public class CenterContainer : Container
    {
        public CenterContainer(Screen screen, IntPtr windowHandle) : base(screen)
        {
            POINT maxStep = new POINT(30, 30); //temporary until i get configs
            ContainedWindow = new WindowObj(windowHandle);
            var NextPoint = new POINT(
                ((int)Screen.WorkingArea.Width / 2) + ContainedWindow.WindowArea.Center.X,
                ((int)Screen.WorkingArea.Height / 2) + ContainedWindow.WindowArea.Center.Y);
            Path = new MoveIterator(ContainedWindow.WindowArea.ToPoint(), NextPoint, maxStep);
            //undecorate window here
        }

        public new void UpdatePosition()
        {
            if (!Path.CanTraverse()) {
                CanBeDisposed = true;
            } else {
                base.UpdatePosition();
            }
        }
    }
}
