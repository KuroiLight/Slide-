using System;
using System.Windows.Media.Animation;
using Win32Api;

namespace SlideSharp
{
    public struct Easer
    {
        private const double MaxPercent = 100.0;
        private double Percent;
        private readonly QuarticEase QuarticEase;
        private POINT StartingPoint;
        private readonly double StepSizeX, StepSizeY;


        public Easer(POINT start, POINT end)
        {
            StartingPoint = start;
            var dist = end - start;
            StepSizeX = dist.X / MaxPercent;
            StepSizeY = dist.Y / MaxPercent;
            Percent = 0;
            QuarticEase = new QuarticEase();
            QuarticEase.EasingMode = EasingMode.EaseOut;
        }

        public POINT TakeStep()
        {
            Percent += 0.025;
            var easedValue = QuarticEase.Ease(Percent) * MaxPercent;
            return StartingPoint + new POINT(StepSizeX * easedValue, StepSizeY * easedValue);
        }

        public bool CanMove()
        {
            return Math.Round(Percent * MaxPercent) < MaxPercent;
        }
    }
}