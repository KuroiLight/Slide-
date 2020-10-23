using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Win32Api;

namespace SlideSharp
{
    public struct Line
    {
        POINT p1, p2;

        public Line(POINT one, POINT two)
        {
            p1 = one; p2 = two;
        }

        public POINT Intersect(Line l2)
        {
            var A1 = p2.Y - p1.Y;
            var B1 = p2.X - p1.X;
            var C1 = A1 * p1.X + B1 * p1.Y;

            var A2 = l2.p2.Y - l2.p1.Y;
            var B2 = l2.p2.X - l2.p1.X;
            var C2 = A2 * p2.X + B2 * p2.Y;

            float delta = A1 * B2 - A2 * B1;
            float x = (B2 * C1 - B1 * C2) / delta;
            float y = (A1 * C2 - A2 * C1) / delta;
            return new POINT((int)x, (int)y);
        }
    }

    public class MouseHelper
    {
        public Line MouseActive_Line;
        public POINT MouseActive_Position;
        public IntPtr MouseActive_Window;
        public IntPtr InitMouseHook()
        {
            return Win32Api.User32.Wrapd_SetWindowsHookEx(MouseHookProc);
        }

        /*public void updatestuff()
        {

            if (MouseActivateWindow != null && MouseActivateVector != null && MouseActivateEndPoint != null) {
                Screen MouseEndScreen = Screen.FromPoint(new Point(((POINT)MouseActivateEndPoint).X, ((POINT)MouseActivateEndPoint).Y));
                var toContainer = ContainerFromVector((Vector)MouseActivateVector);
                if (toContainer != null) {
                    if (toContainer.ContainedWindow != null && MouseEndScreen != null) {
                        Containers.Add(new UndockedWindow(MouseEndScreen, toContainer.ContainedWindow.GetHandle()));
                    }
                    toContainer.SetNewWindow((IntPtr)MouseActivateWindow);
                }

                MouseActivateWindow = null; MouseActivateVector = null; MouseActivateEndPoint = null;
            }
        }*/


        private IntPtr MouseHookProc(int code, Win32Api.WM_MOUSE wParam, Win32Api.MSLLHOOKSTRUCT lParam)
        {
            if (wParam == Win32Api.WM_MOUSE.WM_MOUSEMOVE) {

            } else if (wParam == Win32Api.WM_MOUSE.WM_MBUTTONDOWN) {
                MouseActive_Position = lParam.pt;
            } else if (wParam == Win32Api.WM_MOUSE.WM_MBUTTONUP) {
                
            }

            return Win32Api.User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }


    }
}
