using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Win32Api;
using WpfScreenHelper;

namespace SlideSharp
{
    [Flags]
    public enum Direction
    {
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }

    public enum Status : int
    {
        Hiding = 1,
        Showing = -1
    }

    

    public class Container
    {
        public Container(Screen screen)
        {
            CanBeDisposed = false;
            Screen = screen;
        }

        public bool CanBeDisposed { get; protected set; }
        public WindowObj ContainedWindow { get; protected set; }
        public MoveIterator Path { get; protected set; }
        public Screen Screen { get; }

        public void RemoveWindow()
        {
            ContainedWindow = null;
            Path = default;
        }

        public void UpdatePosition()
        {
            if (ContainedWindow.Exists() && Path?.CanTraverse() == true) {
                ContainedWindow.SetPosition(Path.Traverse());
            }
        }
    }

    
}