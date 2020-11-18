using System;
using Win32Api;

namespace SlideSharp
{
    public struct Easer
    {
        private const double MaxPercent = 100.0f;
        private double Percent;
        private POINT StartingPoint;
        private readonly double StepSizeX, StepSizeY;

        public Easer(POINT start, POINT end)
        {
            StartingPoint = start;
            var dist = end - start;
            StepSizeX = dist.X / MaxPercent;
            StepSizeY = dist.Y / MaxPercent;
            Percent = 0;
        }

        public POINT TakeStep()
        {
            Percent += Configuration.Config.WINDOW_ANIM_SPEED;
            var easedValue = Out(Percent) * MaxPercent;
            return StartingPoint + new POINT(StepSizeX * easedValue, StepSizeY * easedValue);
        }

        public bool CanMove()
        {
            return Math.Round(Percent * MaxPercent) < MaxPercent;
        }
        public static double Out(double k)
        {
            return k == 1.0 ? 1.0 : 1.0 - Math.Pow(2.0, -10.0 * k);
        }
    }
}