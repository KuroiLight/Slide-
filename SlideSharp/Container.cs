using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Win32Api;
using WpfScreenHelper;

namespace SlideSharp
{
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    public enum SideModifier : int
    {
        Positive = 1,
        Negative = -1
    }

    public enum Status : int
    {
        Hiding = 1,
        Showing = -1
    }

    public class MoveIterator
    {
        private POINT Distance;
        private POINT Start;
        private POINT Step;

        public MoveIterator(POINT start, POINT end, POINT maxStep)
        {
            Start = start;
            Distance = start - end;
            Step = maxStep;
        }

        public bool CanTraverse()
        {
            return Start + Distance != Start;
        }

        public POINT Traverse()
        {
            
            var NextStep = new POINT(Math.Clamp(Distance.X, -1 * Step.X, Step.X), Math.Clamp(Distance.Y, -1 * Step.Y, Step.Y));
            Debug.Write($"Distance Left: {Distance}, Start {Start} + ");
            Start = Start + NextStep;
            Distance = Distance - NextStep;
            Debug.WriteLine($"{NextStep} = {Start}");
            return Start;
        }
    }

    public class Container
    {
        public Container(Screen screen)
        {
            CanBeDisposed = false;
            Screen = screen;
        }

        public bool CanBeDisposed { get; protected set; }
        public WindowObj ContainedWindow { get; protected set; }
        public MoveIterator Path { get; protected set; }
        public Screen Screen { get; }

        public void UpdatePosition()
        {
            if (ContainedWindow.Exists() && Path.CanTraverse()) {
                ContainedWindow.SetPosition(Path.Traverse());
            }
        }

        public void RemoveWindow()
        {
            ContainedWindow = null;
            Path = default;
        }
    }

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

    public class EdgeContainer : Container
    {
        public EdgeContainer(Orientation orientation, SideModifier sideModifier, Screen screen) : base(screen)
        {
            Orientation = orientation;
            SideModifier = sideModifier;
            Status = Status.Showing;
            ScreenEdge = (orientation, sideModifier) switch
            {
                (Orientation.Horizontal, SideModifier.Positive) => new RECT(Screen.Bounds.Right - 1, Screen.Bounds.Top + 1, Screen.Bounds.Right + 1, Screen.Bounds.Bottom - 1),
                (Orientation.Horizontal, SideModifier.Negative) => new RECT(Screen.Bounds.Left - 1, Screen.Bounds.Top + 1, Screen.Bounds.Left + 1, Screen.Bounds.Bottom - 1),
                (Orientation.Vertical, SideModifier.Positive) => new RECT(Screen.Bounds.Left + 1, Screen.Bounds.Bottom - 1, Screen.Bounds.Right - 1, Screen.Bounds.Bottom + 1),
                (Orientation.Vertical, SideModifier.Negative) => new RECT(Screen.Bounds.Left + 1, Screen.Bounds.Top - 1, Screen.Bounds.Right - 1, Screen.Bounds.Top + 1),
                _ => throw new InvalidOperationException(),
            };
        }

        public Orientation Orientation { get; }
        public SideModifier SideModifier { get; }
        public Status Status { get; private set; }
        public RECT ScreenEdge { get; private set; }

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
                ScreenEdge.Center.X - (ContainedWindow.WindowArea.Width / 2),
                ScreenEdge.Center.Y - (ContainedWindow.WindowArea.Height / 2));

            if (Orientation == Orientation.Horizontal) {
                NextPoint.X += ((int)Status * (int)SideModifier * (ContainedWindow.WindowArea.Width / 2)) + ((int)SideModifier * offSet);
            } else if(Orientation == Orientation.Vertical) {
                NextPoint.Y += ((int)Status * (int)SideModifier * (ContainedWindow.WindowArea.Height / 2)) + ((int)SideModifier * offSet);
            }
            Debug.WriteLine($"{NextPoint} {ScreenEdge.Center}");
            Path = new MoveIterator(ContainedWindow.WindowArea.ToPoint(), NextPoint, maxStep);
        }

        public bool Intersect(POINT start, POINT end)
        {
            var DifOfScreenToStart = ScreenEdge.Center - start;
            var VecFromStartEnd = start - end;
            var divs = (Orientation) switch
            {
                Orientation.Horizontal => (double)DifOfScreenToStart.X / (double)VecFromStartEnd.X,
                Orientation.Vertical => (double)DifOfScreenToStart.Y / (double)VecFromStartEnd.Y,
                _ => throw new InvalidOperationException(),
            };
            var PointAtEnd = new POINT(divs * VecFromStartEnd.X, divs * VecFromStartEnd.Y);
            var EndPoint = start + PointAtEnd;

            return ScreenEdge.Contains(EndPoint);
        }

        public static EdgeContainer GetValidInstance(Orientation orientation, SideModifier sideModifier, Screen screen)
        {
            var NewInstance = new EdgeContainer(orientation, sideModifier, screen);
            return Screen.AllScreens.ToList().Exists((S) => {
                if (S == NewInstance.Screen) {
                    return false;
                } else {
                    return S.Bounds.Contains(NewInstance.ScreenEdge.ToWindowsRect());
                }
            }) ? null : NewInstance;
        }

        public static List<EdgeContainer> GetAllValidInstances()
        {
            List<EdgeContainer> NewList = new List<EdgeContainer>();
            Screen.AllScreens.ToList().ForEach((S) => {
                foreach(Orientation o in Enum.GetValues(typeof(Orientation))) {
                    foreach(SideModifier sm in Enum.GetValues(typeof(SideModifier))) {
                        EdgeContainer edge = GetValidInstance(o, sm, S);
                        if (edge != null) {
                            NewList.Add(edge);
                        }
                    }
                }
            });
            return NewList;
        }
    }
}