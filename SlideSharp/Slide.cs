﻿using System.Diagnostics;
using Win32Api;
using WpfScreenHelper;

namespace SlideSharp
{
    public abstract class Slide
    {
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
        public LeftSlide(Screen screen) : base(screen) { }
        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Left, _screen.Center.Y - WindowRect.Height / 2);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT((_screen.Left - WindowRect.Width) + Configuration.SettingsInstance.Window_Offscreen_Offset, _screen.Center.Y - WindowRect.Height / 2);
        }
    }

    public class RightSlide : Slide
    {
        public RightSlide(Screen screen) : base(screen) { }
        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Right - WindowRect.Width, _screen.Center.Y - WindowRect.Height / 2);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT(_screen.Right - Configuration.SettingsInstance.Window_Offscreen_Offset, _screen.Center.Y - WindowRect.Height / 2);
        }
    }

    public class TopSlide : Slide
    {
        public TopSlide(Screen screen) : base(screen) { }
        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Width / 2, _screen.Top);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Width / 2, (_screen.Top - WindowRect.Height) + Configuration.SettingsInstance.Window_Offscreen_Offset);
        }
    }

    public class BottomSlide : Slide
    {
        public BottomSlide(Screen screen) : base(screen) { }
        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Width / 2, _screen.Bottom - WindowRect.Height);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Width / 2, _screen.Bottom - Configuration.SettingsInstance.Window_Offscreen_Offset);
        }
    }
}
