using System.Diagnostics;
using Win32Api;

namespace SlideSharp
{
    public struct Easer
    {
        private POINT DistanceFromStartToEnd;
        public int Percent;


        public Easer(POINT start, POINT end)
        {
            DistanceFromStartToEnd = end - start;
            Percent = 0;
        }

        public POINT TakeStep()
        {
            Percent++;
            return (DistanceFromStartToEnd / 100) * Percent;
        }
    }
}
