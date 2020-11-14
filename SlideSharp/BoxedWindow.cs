using System;
using Win32Api;

namespace SlideSharp
{


    public class BoxedWindow
    {
        public readonly IntPtr hWnd;
        private Easer Easer;
        private Status Status;
        public Slide Slide;

        public BoxedWindow(IntPtr hwnd, Slide slide)
        {
            Status = Status.Undefined;
            Slide = slide;
            hWnd = hwnd;
            Easer = new Easer();
        }

        public void SetStatus(Status status)
        {
            if (status != Status) {
                Status = status;
                var windowRect = User32.GetWindowRect(hWnd);
                User32.SetWindowPos(hWnd, Status == Status.Showing ? Imports.HWND_INSERTAFTER.HWND_NOTOPMOST : Imports.HWND_INSERTAFTER.HWND_TOPMOST);
                Easer = new Easer(windowRect.ToPoint, Status != Status.Showing ? Slide.HiddenPosition(windowRect) : Slide.ShownPosition(windowRect));
            }
        }

        public void Move()
        {
            if (FinishedMoving()) return;
            User32.SetWindowPos(hWnd, Easer.TakeStep());
        }

        public bool FinishedMoving()
        {
            return !Easer.CanMove();
        }
    }
}