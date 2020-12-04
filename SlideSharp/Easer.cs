using System;
using System.Runtime.InteropServices;
using Win32Api;

namespace SlideSharp
{
    [StructLayout(LayoutKind.Auto)]
    public struct Easer
    {
        private double _percent;
        private readonly POINT _start, _dist;
        public bool AtEnd { get => (1.0 - _percent) < Config.GetInstance.WindowMovementSpeed; }

        public Easer(POINT start, POINT end)
        {
            _percent = 0;
            _start = start;
            _dist = end - start;
        }

        public POINT Step()
        {
            _percent += Config.GetInstance.WindowMovementSpeed;

            var easedPercent = EaseOut(_percent);
            return _start + (_dist * easedPercent);
        }

        private static double EaseOut(double k)
        {
            return k == 1.0 ? 1.0 : 1.0 - Math.Pow(2.0, -10.0 * k);
        }
    }
}