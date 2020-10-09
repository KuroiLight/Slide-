using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using DWORD = System.Int32;
using HWND = System.IntPtr;

namespace WindowShift
{
    internal class Main
    {
        private List<AnchorPoint> Anchors = new List<AnchorPoint>();
        private HWND hMouseLLHook = HWND.Zero;

        public Main()
        {
            hMouseLLHook = Api.SetWindowsHookEx(Api.WH_MOUSE_LL, MouseHookProc, HWND.Zero, 0);
            FindAllAnchorPoints();
        }

        ~Main()
        {
            Api.UnhookWinEvent(hMouseLLHook);
        }

        private HWND TrayhWnd = Api.FindWindow("Shell_TrayWnd", null);
        private HWND DesktophWnd = Api.GetDesktopWindow();

        private void FindAllAnchorPoints()
        {
            Api.GetWindowRect(TrayhWnd, out RECT trayRect);

            foreach (var S in Screen.AllScreens.AsParallel()) {
                foreach (var D in Enumerable.Range(1, 4)) {
                    var Anchor = new AnchorPoint((DragDirection)D, S);
                    if (!trayRect.Contains(Anchor.AnchorPt)) {
                        Anchors.Add(new AnchorPoint((DragDirection)D, S));
                    }
                }
            }
        }

        private void TransitionWindow(HWND window, POINT pt)
        {
            //SetWindowPos?
        }

        private static HWND WindowFromCursor()
        {
            Api.GetCursorPos(out POINT p);
            return WindowFrom(p);
        }

        private static HWND WindowFrom(POINT pt)
        {
            HWND CurrentWindow = Api.WindowFromPoint(pt);

            while (Api.GetParent(CurrentWindow) != IntPtr.Zero) {
                CurrentWindow = Api.GetParent(CurrentWindow);
            }

            return CurrentWindow;
        }

        private DragDirection DirectionFromPts(POINT start, POINT end, int deadzone = 0)
        {
            (int xDif, int yDif) = (start.X - end.X, start.Y - end.Y);
            (int axDif, int ayDif) = (Math.Abs(xDif), Math.Abs(yDif));
            return (xDif > 0, yDif > 0) switch
            {
                (_, false) when (axDif < ayDif) => DragDirection.Down,
                (_, true) when (axDif < ayDif) => DragDirection.Up,
                (false, _) when (axDif > ayDif) => DragDirection.Right,
                (true, _) when (axDif > ayDif) => DragDirection.Left,
                _ => 0
            };
        }

        private HWND MButtonWindow;
        private POINT MButtonStartPoint;

        private HWND MouseHookProc(DWORD code, Api.WM_MOUSE wParam, Api.MSLLHOOKSTRUCT lParam)
        {
            if (wParam.HasFlag(Api.WM_MOUSE.WM_MOUSEMOVE)) {
                Task.Run(() => { //temporary solution, so CallNextHookEx doesn't fail
                    var Anchor = Anchors.FirstOrDefault(A => A.WindowHandle == WindowFrom(lParam.pt));
                    if (Anchor != null) {
                        Anchor.TransitionWindow(false);
                    } else {
                        foreach (var A in Anchors) {
                            if (!A.Hidden) {
                                A.TransitionWindow(true);
                            }
                        }
                    }
                });
            }
            if (!wParam.HasFlag(Api.WM_MOUSE.WM_MOUSEWHEEL) && wParam.HasFlag(Api.WM_MOUSE.WM_MBUTTONDOWN)) {
                MButtonStartPoint = lParam.pt;
                MButtonWindow = WindowFrom(lParam.pt);
            } else if (!wParam.HasFlag(Api.WM_MOUSE.WM_MOUSEWHEEL) && wParam.HasFlag(Api.WM_MOUSE.WM_MBUTTONUP)) {
                Task.Run(() => { //temporary solution, so CallNextHookEx doesn't fail
                    var dir = DirectionFromPts(MButtonStartPoint, lParam.pt);
                    if (dir != DragDirection.None) {
                        var Anchor = Anchors.FirstOrDefault(A => A.Direction == dir && A.MonitorArea.Contains(lParam.pt));
                        if (Anchor != null) {
                            var prevAnchor = Anchors.FirstOrDefault(A => A.WindowHandle == MButtonWindow);
                            if (prevAnchor != null) {
                                prevAnchor.WindowHandle = HWND.Zero;
                            }
                            Anchor.WindowHandle = MButtonWindow;
                        }
                    }

                    MButtonWindow = HWND.Zero;
                });
            }


            //always proceed as though we werent here
            return Api.CallNextHookEx(HWND.Zero, code, wParam, lParam);
        }

    }
}
