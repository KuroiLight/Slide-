using System;
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
    public struct MoveIterator
    {
        private POINT End;
        private POINT Start;
        private POINT StepSize;

        public MoveIterator(POINT start, POINT end, POINT maxStep)
        {
            Start = start;
            End = end;
            StepSize = maxStep;
        }

        public bool CanTraverse()
        {
            return (Start - End != default);
        }

        public POINT Traverse()
        {
            Start += new POINT(Math.Clamp(Start.X - End.X, -1 * StepSize.X, StepSize.X), Math.Clamp(Start.Y - End.Y, -1 * StepSize.Y, StepSize.Y));
            return Start;
        }
    }

    public class DockedWindow : WindowContainer
    {
        public DockedWindow(Orientation orientation, SideModifier sideModifier, Screen screen) : base(screen)
        {
            Orientation = orientation;
            SideModifier = sideModifier;
            Status = Status.Showing;
        }

        public Orientation Orientation { get; private set; }
        public SideModifier SideModifier { get; private set; }
        public Status Status { get; private set; }
        public void SetNewWindow(IntPtr windowHandle)
        {
            ContainedWindow = new WindowObj(windowHandle);
            Status = Status.Showing;
        }

        public void SetState(Status status)
        {
            if (status != Status) {
                Status = status;
                GenerateNewPath();
            }
        }

        public void UpdatePosition()
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
            POINT NextPoint = (Orientation, SideModifier) switch
            {
                (Orientation.Horizontal, SideModifier.Positive) => new POINT(
                    (int)Screen.WorkingArea.Right + ((int)Status * (int)SideModifier * ContainedWindow.WindowArea.Width),
                    ((int)Screen.WorkingArea.Height / 2) + ContainedWindow.WindowArea.Center.Y),
                (Orientation.Vertical, SideModifier.Positive) => new POINT(
                    ((int)Screen.WorkingArea.Width / 2) + ContainedWindow.WindowArea.Center.X,
                    (int)Screen.WorkingArea.Bottom + ((int)Status * (int)SideModifier * ContainedWindow.WindowArea.Height)),
                (Orientation.Horizontal, SideModifier.Negative) => new POINT(
                    (int)Screen.WorkingArea.X + ((int)Status * (int)SideModifier * ContainedWindow.WindowArea.Width),
                    ((int)Screen.WorkingArea.Height / 2) + ContainedWindow.WindowArea.Center.Y),
                (Orientation.Vertical, SideModifier.Negative) => new POINT(
                    ((int)Screen.WorkingArea.Width / 2) + ContainedWindow.WindowArea.Center.X,
                    (int)Screen.WorkingArea.Y + ((int)Status * (int)SideModifier * ContainedWindow.WindowArea.Height)),
                _ => throw new NotImplementedException(),
            };
            Path = new MoveIterator(ContainedWindow.WindowArea.ToPoint(), NextPoint, maxStep);
        }
    }

    public class UndockedWindow : WindowContainer
    {
        public UndockedWindow(Screen screen, IntPtr windowHandle) : base(screen)
        {
            POINT maxStep = new POINT(30, 30); //temporary until i get configs
            ContainedWindow = new WindowObj(windowHandle);
            var NextPoint = new POINT(
                ((int)Screen.WorkingArea.Width / 2) + ContainedWindow.WindowArea.Center.X,
                ((int)Screen.WorkingArea.Height / 2) + ContainedWindow.WindowArea.Center.Y);
            Path = new MoveIterator(ContainedWindow.WindowArea.ToPoint(), NextPoint, maxStep);
            //undecorate window here
        }

        public void UpdatePosition()
        {
            if (!Path.CanTraverse()) {
                CanBeDisposed = true;
            } else {
                base.UpdatePosition();
            }
        }
    }

    public abstract class WindowContainer
    {
        public WindowContainer(Screen screen)
        {
            CanBeDisposed = false;
            Screen = screen;
        }

        public bool CanBeDisposed { get; protected set; }
        public WindowObj ContainedWindow { get; protected set; }
        public MoveIterator Path { get; protected set; }
        public Screen Screen { get; private set; }
        public void UpdatePosition()
        {
            if (ContainedWindow.Exists() && Path.CanTraverse()) {
                ContainedWindow.SetPosition(Path.Traverse());
            }
        }
    }
}
