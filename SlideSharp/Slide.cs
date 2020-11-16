using System;
using System.Collections.Generic;
using Win32Api;
using WpfScreenHelper;

namespace SlideSharp
{
    public abstract class Slide
    {
        protected readonly RECT _screen;
        public Screen Screen { get; }

        public Slide(Screen screen)
        {
            Screen = screen;
            _screen = new RECT(screen.WorkingArea.Left, screen.WorkingArea.Top, screen.WorkingArea.Right, screen.WorkingArea.Bottom);
        }
        public Slide(Slide slide)
        {
            Screen = slide.Screen;
            _screen = new RECT(Screen.WorkingArea.Left, Screen.WorkingArea.Top, Screen.WorkingArea.Right, Screen.WorkingArea.Bottom);
        }

        public abstract POINT ShownPosition(RECT WindowRect);

        public abstract POINT HiddenPosition(RECT WindowRect);

        public override bool Equals(object obj)
        {
            return obj is Slide slide &&
                   EqualityComparer<Screen>.Default.Equals(Screen, slide.Screen) &&
                   _screen.Equals(slide._screen);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Screen, _screen);
        }
    }

    public sealed class CenterSlide : Slide
    {
        public CenterSlide(Screen screen) : base(screen) { }

        public CenterSlide(Slide slide) : base(slide) { }

        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Width / 2, _screen.Center.Y - WindowRect.Height / 2);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Width / 2, _screen.Center.Y - WindowRect.Height / 2);
        }

        public override bool Equals(object obj)
        {
            return obj is CenterSlide slide &&
                   base.Equals(obj) &&
                   EqualityComparer<Screen>.Default.Equals(Screen, slide.Screen) &&
                   _screen.Equals(slide._screen);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), "Center");
        }
    }

    public sealed class LeftSlide : Slide
    {
        public LeftSlide(Screen screen) : base(screen) { }

        public LeftSlide(Slide slide) : base(slide) { }
        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Left, _screen.Center.Y - WindowRect.Height / 2);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT((_screen.Left - WindowRect.Width) + Configuration.Config.HIDDEN_OFFSET, _screen.Center.Y - WindowRect.Height / 2);
        }

        public override bool Equals(object obj)
        {
            return obj is LeftSlide slide &&
                   base.Equals(obj) &&
                   EqualityComparer<Screen>.Default.Equals(Screen, slide.Screen) &&
                   _screen.Equals(slide._screen);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), "Left");
        }
    }

    public sealed class RightSlide : Slide
    {
        public RightSlide(Screen screen) : base(screen) { }

        public RightSlide(Slide slide) : base(slide) { }
        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Right - WindowRect.Width, _screen.Center.Y - WindowRect.Height / 2);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT(_screen.Right - Configuration.Config.HIDDEN_OFFSET, _screen.Center.Y - WindowRect.Height / 2);
        }

        public override bool Equals(object obj)
        {
            return obj is RightSlide slide &&
                   base.Equals(obj) &&
                   EqualityComparer<Screen>.Default.Equals(Screen, slide.Screen) &&
                   _screen.Equals(slide._screen);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), "Right");
        }
    }

    public sealed class TopSlide : Slide
    {
        public TopSlide(Screen screen) : base(screen) { }

        public TopSlide(Slide slide) : base(slide) { }
        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Width / 2, _screen.Top);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Width / 2, (_screen.Top - WindowRect.Height) + Configuration.Config.HIDDEN_OFFSET);
        }

        public override bool Equals(object obj)
        {
            return obj is TopSlide slide &&
                   base.Equals(obj) &&
                   EqualityComparer<Screen>.Default.Equals(Screen, slide.Screen) &&
                   _screen.Equals(slide._screen);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), "Top");
        }
    }

    public sealed class BottomSlide : Slide
    {
        public BottomSlide(Screen screen) : base(screen) { }

        public BottomSlide(Slide slide) : base(slide) { }
        public override POINT ShownPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Width / 2, _screen.Bottom - WindowRect.Height);
        }

        public override POINT HiddenPosition(RECT WindowRect)
        {
            return new POINT(_screen.Center.X - WindowRect.Width / 2, _screen.Bottom - Configuration.Config.HIDDEN_OFFSET);
        }

        public override bool Equals(object obj)
        {
            return obj is BottomSlide slide &&
                   base.Equals(obj) &&
                   EqualityComparer<Screen>.Default.Equals(Screen, slide.Screen) &&
                   _screen.Equals(slide._screen);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), "Bottom");
        }
    }
}
