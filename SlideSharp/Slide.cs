using System.Diagnostics;
using Win32Api;
using WpfScreenHelper;

namespace SlideSharp
{
    public abstract class Slide
    {
        public Direction Direction;
        public Screen Screen;
        protected RECT _screen;

        public Slide(Screen screen)
        {
            Screen = screen;
            _screen = new RECT(screen.WorkingArea.Left, screen.WorkingArea.Top, screen.WorkingArea.Right, screen.WorkingArea.Bottom);
        }


        public abstract POINT ShownPosition(RECT WindowRect);

        public abstract POINT HiddenPosition(RECT WindowRect);
    }

    public class CenterSlide : Slide
    {
        public new Direction Direction = Direction.Center;
        public CenterSlide(Screen screen) : base(screen) { }

        public override POINT ShownPosition(RECT WindowRect)
        {
            return _screen.Center - WindowRect.Center;
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return _screen.Center - WindowRect.Center;
        }
    }

    public class LeftSlide : Slide
    {
        public new Direction Direction = Direction.Left;
        public LeftSlide(Screen screen) : base(screen) { }
        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Left, _screen.Center.Y - WindowRect.Center.Y);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT((_screen.Left - WindowRect.Width) + 30, _screen.Center.Y - WindowRect.Center.Y);
        }
    }

    public class RightSlide : Slide
    {
        public new Direction Direction = Direction.Right;
        public RightSlide(Screen screen) : base(screen) { }
        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Right - WindowRect.Width, _screen.Center.Y - WindowRect.Center.Y);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT(_screen.Right - Configuration.SettingsInstance.Window_Offscreen_Offset, _screen.Center.Y - WindowRect.Center.Y);
        }
    }

    public class TopSlide : Slide
    {
        public new Direction Direction = Direction.Up;
        public TopSlide(Screen screen) : base(screen) { }
        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Center.X, _screen.Top);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Center.X, (_screen.Top - WindowRect.Height) + Configuration.SettingsInstance.Window_Offscreen_Offset);
        }
    }

    public class BottomSlide : Slide
    {
        public new Direction Direction = Direction.Down;
        public BottomSlide(Screen screen) : base(screen) { }
        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Center.X, _screen.Bottom - WindowRect.Height);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Center.X, _screen.Bottom - Configuration.SettingsInstance.Window_Offscreen_Offset);
        }
    }
}
