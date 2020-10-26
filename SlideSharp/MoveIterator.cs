using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Win32Api;

namespace SlideSharp
{
    public class MoveIterator
    {
        private POINT Vector;
        private POINT Start;
        private POINT Step;

        public MoveIterator(POINT start, POINT end, POINT maxStep)
        {
            Start = start;
            Vector = end - start;
            Step = maxStep;
            Debug.WriteLine($"New MoveIterator [{Start}, {Vector}, {Step}]");
        }

        public bool CanTraverse()
        {
            return Start + Vector != Start;
        }

        public POINT Traverse()
        {
            var NextStep = new POINT(Math.Clamp(Vector.X, -1 * Step.X, Step.X), Math.Clamp(Vector.Y, -1 * Step.Y, Step.Y));
            Start = Start + NextStep;
            Vector = Vector - NextStep;
            Debug.WriteLine($"Traverse: {Start} : {Vector}");
            return Start;
        }
    }
}
