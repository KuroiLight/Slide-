using Screen_Drop_In;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using Win32Api;
using static Win32Api.User32;

namespace SlideSharp
{
    public sealed class Windows : IDisposable, IEquatable<Windows>
    {
        private readonly Queue<BoxedWindow> AllWindows;

        private bool _disposed;

        private BoxedWindow? _newBoxedWindow;

        public Windows()
        {
            AllWindows = new Queue<BoxedWindow>(Screen.AllScreens.Length * 5);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Equals(Windows? other)
        {
            return other != null && AllWindows == other.AllWindows;
        }

        public override bool Equals(object? obj)
        {
            return obj is Window window && Equals(window);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this);
        }

        public void SetNewWindow(BoxedWindow window)
        {
            Interlocked.Exchange(ref _newBoxedWindow, window);
        }

        public void UpdateWindows()
        {
            BoxedWindow? localBoxedWindow = _newBoxedWindow;
            _newBoxedWindow = null;

            IntPtr WindowAtCursor = GetRootWindow(GetCursorPos());

            AllWindows.ReQueue((w) =>
            {
                if (localBoxedWindow?.hWnd == w.hWnd)
                {
                    w.SetStatus(Status.Undefined);
                    return false;
                }
                if (!User32.IsWindow(w.hWnd) || (w.Slide is CenterSlide && w.FinishedMoving()))
                {
                    return false;
                }

                if (localBoxedWindow?.Slide.Equals(w.Slide) == true)
                {
                    w.Slide = new CenterSlide(w.Slide);
                    w.SetStatus(Status.Undefined);
                }

                w.SetStatus(w.hWnd == WindowAtCursor ? Status.Showing : Status.Hiding);
                w.Move();

                return true;
            });

            if (localBoxedWindow != null)
            {
                localBoxedWindow!.SetStatus(Status.Hiding);
                AllWindows.Enqueue(localBoxedWindow!);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    while (AllWindows.Count > 0)
                    {
                        var window = AllWindows.Dequeue();

                        window.Slide = new CenterSlide(window.Slide);
                        window.SetStatus(Status.Showing);
                        while (!window.FinishedMoving())
                        {
                            window.Move();
                        }
                    }
                }
                _disposed = true;
            }
        }
    }
}