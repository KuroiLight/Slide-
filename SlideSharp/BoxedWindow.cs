using System;
using Win32Api;

namespace SlideSharp
{
    public class BoxedWindow
    {
        public readonly IntPtr hWnd;
        private Easer Easer;
        private Status Status;

        public Slide Slide { get; set; }

        public BoxedWindow(IntPtr hwnd, Slide slide)
        {
            Status = Status.Undefined;
            Slide = slide;
            hWnd = hwnd;
            Easer = new Easer();
        }

        public void SetStatus(Status status)
        {
            if (status != Status)
            {
                Status = status;
                var windowRect = User32.GetWindowRect(hWnd);
                User32.SetWindowPos(hWnd, Status != Status.Hiding ? Imports.HWND_INSERTAFTER.HWND_TOPMOST : Imports.HWND_INSERTAFTER.HWND_NOTOPMOST);
                Easer = new Easer(windowRect.XY, Status != Status.Showing ? Slide.HiddenPosition(windowRect) : Slide.ShownPosition(windowRect));
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