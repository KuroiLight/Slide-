using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Win32Api;
using Screen_Drop_In;
using static Win32Api.User32;

namespace SlideSharp
{
    public sealed class Windows : IDisposable, IEquatable<Windows>
    {
        private readonly Queue<BoxedWindow> AllWindows;
        private BoxedWindow? _newBoxedWindow;
        private bool _disposed;
        public Windows()
        {
            AllWindows = new Queue<BoxedWindow>(Screen.AllScreens.Length * 5);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this._disposed) {
                if (disposing) {
                    while (AllWindows.Count > 0) {
                        var window = AllWindows.Dequeue();

                        window.Slide = new CenterSlide(window.Slide);
                        window.SetStatus(Status.Showing);
                        while (!window.FinishedMoving()) {
                            window.Move();
                        }
                    }
                }
                _disposed = true;
            }
        }

        public void SetNewWindow(BoxedWindow window)
        {
            Interlocked.Exchange(ref _newBoxedWindow, window);
        }

        public void UpdateWindows()
        {
            static void LoopQueue<T>(Queue<T> q, Func<T, bool> method)
            {
                var total = q.Count;
                for (int i = 0; i < total; i++) {
                    var item = q.Dequeue();
                    if (method(item)) {
                        q.Enqueue(item);
                    }
                }
            }

            BoxedWindow? localBoxedWindow = _newBoxedWindow;
            _newBoxedWindow = null;

            IntPtr WindowAtCursor = GetRootWindow(GetCursorPos());

            LoopQueue<BoxedWindow>(AllWindows, (w) => {
                if (localBoxedWindow?.hWnd == w.hWnd) {
                    w.SetStatus(Status.Undefined);
                    return false;
                }
                if (!User32.IsWindow(w.hWnd) || (w.Slide is CenterSlide && w.FinishedMoving())) {
                    return false;
                }

                if (localBoxedWindow?.Slide.Equals(w.Slide) == true) {
                    w.Slide = new CenterSlide(w.Slide);
                    w.SetStatus(Status.Undefined);
                }

                w.SetStatus(w.hWnd == WindowAtCursor ? Status.Showing : Status.Hiding);
                w.Move();

                return true;
            });

            if (localBoxedWindow != null) {
                localBoxedWindow!.SetStatus(Status.Hiding);
                AllWindows.Enqueue(localBoxedWindow!);
            }
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
    }
}
