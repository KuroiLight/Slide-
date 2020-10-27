using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Win32Api;
using WpfScreenHelper;

namespace SlideSharp
{
    public class EdgeContainer : Container
    {
        public EdgeContainer(Direction direction, Screen screen) : base(screen)
        {
            Direction = direction;
            Status = Status.Showing;
            ScreenEdge = (Direction) switch
            {
                Direction.Up => new RECT(Screen.WorkingArea.Left + 1, Screen.WorkingArea.Top - 1, Screen.WorkingArea.Right - 1, Screen.WorkingArea.Top + 1),
                Direction.Down => new RECT(Screen.WorkingArea.Left + 1, Screen.WorkingArea.Bottom - 1, Screen.WorkingArea.Right - 1, Screen.WorkingArea.Bottom + 1),
                Direction.Left => new RECT(Screen.WorkingArea.Left - 1, Screen.WorkingArea.Top + 1, Screen.WorkingArea.Left + 1, Screen.WorkingArea.Bottom - 1),
                Direction.Right => new RECT(Screen.WorkingArea.Right - 1, Screen.WorkingArea.Top + 1, Screen.WorkingArea.Right + 1, Screen.WorkingArea.Bottom - 1),
                _ => throw new InvalidOperationException(),
            };
        }

        public Direction Direction { get; private set; }
        public RECT ScreenEdge { get; private set; }
        public Status Status { get; private set; }
        public static List<EdgeContainer> GetAllValidInstances()
        {
            List<EdgeContainer> NewList = new List<EdgeContainer>();
            Screen.AllScreens.ToList().ForEach((S) => {
                foreach (Direction d in Enum.GetValues(typeof(Direction))) {
                    EdgeContainer edge = GetValidInstance(d, S);
                    if (edge != null) {
                        NewList.Add(edge);
                    }
                }
            });
            return NewList;
        }

        public static EdgeContainer GetValidInstance(Direction direction, Screen screen)
        {
            var NewInstance = new EdgeContainer(direction, screen);
            return Screen.AllScreens.ToList().Exists((S) => {
                if (S == NewInstance.Screen) {
                    return false;
                } else {
                    return S.Bounds.Contains(NewInstance.ScreenEdge.ToWindowsRect());
                }
            }) ? null : NewInstance;
        }

        public bool Intersect(POINT start, POINT end)
        {
            POINT VecFromStartEnd = start - end;
            Direction vectorDirection = new Direction();
            if (VecFromStartEnd.X != 0) {
                vectorDirection |= (VecFromStartEnd.X > 0 ? Direction.Left : Direction.Right);
            }
            if (VecFromStartEnd.Y != 0) {
                vectorDirection |= (VecFromStartEnd.Y > 0 ? Direction.Up : Direction.Down);
            }
            if ((vectorDirection & Direction) != Direction) {
                return false;
            }

            var DifOfScreenToStart = ScreenEdge.Center - start;
            var divs = (Direction) switch
            {
                Direction.Left => (double)DifOfScreenToStart.X / (double)VecFromStartEnd.X,
                Direction.Up => (double)DifOfScreenToStart.Y / (double)VecFromStartEnd.Y,
                Direction.Right => (double)DifOfScreenToStart.X / (double)VecFromStartEnd.X,
                Direction.Down => (double)DifOfScreenToStart.Y / (double)VecFromStartEnd.Y,
                _ => throw new InvalidOperationException(),
            };

            var PointAtEnd = new POINT(divs * VecFromStartEnd.X, divs * VecFromStartEnd.Y);
            var EndPoint = start + PointAtEnd;
            Debug.WriteLine($"{ScreenEdge.Contains(EndPoint)}, @{EndPoint} Wants:{ScreenEdge}");
            return ScreenEdge.Contains(EndPoint);
        }

        public void SetNewWindow(IntPtr windowHandle)
        {
            ContainedWindow = new WindowObj(windowHandle);
            Status = Status.Showing;
        }

        public void SetState(Status status)
        {
            if (ContainedWindow != null && status != Status) {
                Status = status;
                GenerateNewPath();
            }
        }

        public new void UpdatePosition()
        {
            if (!Path.CanTraverse()) {
                //set window status specific decor here
            } else {
                base.UpdatePosition();
            }
        }

        private void GenerateNewPath()
        {
            POINT maxStep = new POINT(30, 30); //temporary until i get configs
            int offSet = Status == Status.Showing ? 0 : 30;
            POINT NextPoint = new POINT(
                ScreenEdge.Center.X - ContainedWindow.WindowArea.Width / 2,
                ScreenEdge.Center.Y - ContainedWindow.WindowArea.Height / 2);

            switch (Direction) {
                case Direction.Up:
                    break;

                case Direction.Down:
                    break;

                case Direction.Left:
                    break;

                case Direction.Right:
                    break;
            }

            Debug.WriteLine($"{ContainedWindow.WindowArea.ToPoint()} {NextPoint} {ScreenEdge.Center}");
            Path = new MoveIterator(ContainedWindow.WindowArea.ToPoint(), NextPoint, maxStep);
        }
    }
}
