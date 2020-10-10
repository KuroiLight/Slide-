using System;
using System.Collections.Generic;
using System.Linq;
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

        ~Main() => Api.UnhookWinEvent(hMouseLLHook);



        private HWND DesktophWnd = Api.GetDesktopWindow();

        private void FindAllAnchorPoints()
        {
            HWND TrayhWnd = Api.FindWindow("Shell_TrayWnd", null);
            RECT TrayRect = Api.GetWindowRect(TrayhWnd);

            List<Screen> AllScreens = Screen.AllScreens.ToList();
            AllScreens.ForEach((Screen S) => {
                Enumerable.Range((int)DragDirection.Left, (int)DragDirection.Down).ToList().ForEach((int dir) => {
                    DragDirection D = (DragDirection)dir;
                    var AP = new AnchorPoint(D, S);
                    bool shouldAdd = false;

                    switch (D) {
                        case DragDirection.Left:
                            shouldAdd = !AllScreens.Exists(S2 => S2.WorkingArea.Contains(new System.Drawing.Point(AP.AnchorPt.X - 100, AP.AnchorPt.Y)));
                            break;
                        case DragDirection.Right:
                            shouldAdd = !AllScreens.Exists(S2 => S2.WorkingArea.Contains(new System.Drawing.Point(AP.AnchorPt.X + 100, AP.AnchorPt.Y)));
                            break;
                        case DragDirection.Up:
                            shouldAdd = !AllScreens.Exists(S2 => S2.WorkingArea.Contains(new System.Drawing.Point(AP.AnchorPt.X, AP.AnchorPt.Y - 100)));
                            break;
                        case DragDirection.Down:
                            shouldAdd = !AllScreens.Exists(S2 => S2.WorkingArea.Contains(new System.Drawing.Point(AP.AnchorPt.X, AP.AnchorPt.Y + 100)));
                            break;
                    }

                    if (shouldAdd && !TrayRect.Contains(AP.AnchorPt)) {
                        Anchors.Add(AP);
                    }
                });
            });
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
            if (wParam == Api.WM_MOUSE.WM_MOUSEMOVE) {
                var WindowUnderCursor = WindowFrom(lParam.pt);
                Anchors.ForEach(delegate(AnchorPoint Anchor) {
                    if (Anchor.hWindow == WindowUnderCursor) {
                        Anchor.ChangeState(AnchorStatus.OnScreen);
                    } else {
                        Anchor.ChangeState(AnchorStatus.Offscreen);
                    }
                });
            } else if (wParam == Api.WM_MOUSE.WM_MBUTTONDOWN) {
                MButtonStartPoint = lParam.pt;
                MButtonWindow = WindowFrom(lParam.pt);
            } else if (wParam == Api.WM_MOUSE.WM_MBUTTONUP) {
                var dir = DirectionFromPts(MButtonStartPoint, lParam.pt);
                var toAnchor = Anchors.Find(Anchor => Anchor.Direction == dir && Anchor.MonitorArea.Contains(lParam.pt));
                if(toAnchor != null) {
                    Anchors.ForEach(delegate (AnchorPoint Anchor) {
                        if(Anchor.hWindow == MButtonWindow) {
                            Anchor.RemoveWindow();
                        }
                    });
                    toAnchor.AttachWindow(MButtonWindow);
                }
                MButtonWindow = HWND.Zero;
            }

            //always proceed as though we werent here
            return Api.CallNextHookEx(HWND.Zero, code, wParam, lParam);
        }

    }
}
