using Win32Api;

namespace SlideSharp
{
    public struct Easer
    {
        public POINT StartingPosition;
        public POINT EndingPosition;
        public int Percent;


        public Easer(POINT start, POINT end)
        {
            StartingPosition = start;
            EndingPosition = end;
            Percent = 0;
        }

        public POINT TakeStep()
        {
            Percent++;
            return ((EndingPosition - StartingPosition) / 100) * Percent;
        }
    }
}
