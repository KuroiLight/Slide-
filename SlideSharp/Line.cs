using Win32Api;

namespace SlideSharp
{
    internal struct Line
    {
        private readonly POINT pt1;
        private readonly POINT pt2;

        public Line(POINT p1, POINT p2)
        {
            pt1 = p1;
            pt2 = p2;
        }

        public POINT Center()
        {
            return new POINT((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2);
        }
    }
}