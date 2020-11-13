using System.Diagnostics;
using Win32Api;

namespace SlideSharp
{
    public struct Easer
    {
        private POINT StartingPoint;
        private readonly double StepSizeX, StepSizeY;
        public double Percent;


        public Easer(POINT start, POINT end)
        {
            StartingPoint = start;
            var dist = end - start;
            StepSizeX = dist.X / 100.0;
            StepSizeY = dist.Y / 100.0;
            Percent = 0;
        }

        public POINT TakeStep()
        {
            Percent++;
            double X, Y;
            X = StepSizeX * Percent;
            Y = StepSizeY * Percent;
            return StartingPoint + new POINT(X, Y);
        }
    }
}
