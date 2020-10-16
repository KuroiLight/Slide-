using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DWORD = System.Int32;
using HWND = System.IntPtr;


namespace WindowShift
{
    public class Main
    {
        private List<AnchorPoint> Anchors;
        private HWND hMouseLLHook;
        private readonly Timer TaskScheduler = new Timer();
        private HWND WindowFromDragStart = HWND.Zero;
        private POINT FromDragStartPoint = new POINT();
        private Settings Config = Settings.SettingsInstance;

        public Main()
        {
            hMouseLLHook = Api.Wrapd_SetWindowsHookEx(MouseHookProc);
            Anchors = FindAllAnchorPoints();
            TaskScheduler.Tick += UpdateTick;
            Anchors.ForEach(Anchor => TaskScheduler.Tick += Anchor.UpdateTick);
            TaskScheduler.Interval = Config.Update_Interval;
            TaskScheduler.Start();
        }

        public void Dispose()
        {
            Api.Wrapd_UnhookWindowsHookEx(hMouseLLHook);
            Anchors.ForEach(Anchor => Anchor.WindowHandle = HWND.Zero);
            TaskScheduler.Stop();
        }

        private List<AnchorPoint> FindAllAnchorPoints()
        {
            HWND TrayhWnd = Api.Wrapd_FindWindow("Shell_TrayWnd", null);
            RECT TrayRect = Api.Wrapd_GetWindowRect(TrayhWnd);

            var allAnchors = new List<AnchorPoint>();
            var AllScreens = Screen.AllScreens.ToList();

            AllScreens.ForEach((Screen S) => {
                Enumerable.Range((int)DragDirection.None, (int)DragDirection.Down).ToList().ForEach((int dir) => {
                    var D = (DragDirection)dir;
                    var AP = new AnchorPoint(D, S);
                    var shouldAdd = D switch
                    {
                        DragDirection.Left => !AllScreens.Exists(S2 => S2.WorkingArea.Contains(new System.Drawing.Point(AP.AnchorPt.X - 100, AP.AnchorPt.Y))),
                        DragDirection.Right => !AllScreens.Exists(S2 => S2.WorkingArea.Contains(new System.Drawing.Point(AP.AnchorPt.X + 100, AP.AnchorPt.Y))),
                        DragDirection.Up => !AllScreens.Exists(S2 => S2.WorkingArea.Contains(new System.Drawing.Point(AP.AnchorPt.X, AP.AnchorPt.Y - 100))),
                        DragDirection.Down => !AllScreens.Exists(S2 => S2.WorkingArea.Contains(new System.Drawing.Point(AP.AnchorPt.X, AP.AnchorPt.Y + 100))),
                        DragDirection.None => true,
                        _ => throw new IndexOutOfRangeException(nameof(D)),
                    };

                    if (shouldAdd && !TrayRect.Contains(AP.AnchorPt)) {
                        allAnchors.Add(AP);
                    }
                });
            });

            return allAnchors;
        }

        private static HWND WindowFrom(POINT pt)
        {
            HWND CurrentWindow = Api.WindowFromPoint(pt);
            if (CurrentWindow != HWND.Zero) {
                HWND ParentWindow = Api.Wrapd_GetParent(CurrentWindow);

                while (ParentWindow != HWND.Zero) {
                    CurrentWindow = ParentWindow;
                    ParentWindow = Api.Wrapd_GetParent(CurrentWindow);
                }
            }
            return CurrentWindow;
        }

        private DragDirection DirectionFromPts(POINT start, POINT end)
        {
            var deadzone = Config.Middle_Button_DeadZone;

            //get absolute direction
            var vector = new POINT(start.X - end.X, start.Y - end.Y);
            if (Math.Abs(vector.X) > Math.Abs(vector.Y)) { //horizontal movement
                if (vector.X > deadzone) {
                    return DragDirection.Left;
                } else if (vector.X < -1 * deadzone) {
                    return DragDirection.Right;
                }
            } else { //vertical movement
                if (vector.Y > deadzone) {
                    return DragDirection.Up;
                } else if (vector.Y < -1 * deadzone) {
                    return DragDirection.Down;
                }
            }
            return DragDirection.None;
        }

        private void UpdateTick(object sender, EventArgs e)
        {
            HWND WindowUnderCursor = WindowFrom(Api.Wrapd_GetCursorPos());
            Anchors.ForEach(delegate (AnchorPoint Anchor) {
                if (Anchor.WindowHandle == WindowUnderCursor) {
                    Anchor.State = AnchorStatus.OnScreen;
                } else {
                    Anchor.State = AnchorStatus.Offscreen;
                }
            });
        }

        private HWND MouseHookProc(DWORD code, Api.WM_MOUSE wParam, Api.MSLLHOOKSTRUCT lParam)
        {
            if (wParam == Api.WM_MOUSE.WM_MBUTTONDOWN) {
                FromDragStartPoint = lParam.pt;
                WindowFromDragStart = WindowFrom(lParam.pt);
            } else if (wParam == Api.WM_MOUSE.WM_MBUTTONUP) {
                DragDirection dir = DirectionFromPts(FromDragStartPoint, lParam.pt);
                AnchorPoint centerAnchor = null;
                AnchorPoint toAnchor = null;
                AnchorPoint fromAnchor = null;

                Anchors.ForEach((Anchor) => {
                    if (Anchor.SameScreen(lParam.pt)) {
                        if (Anchor.Direction == DragDirection.None) {
                            centerAnchor = Anchor;
                        } else if (Anchor.Direction == dir) {
                            toAnchor = Anchor;
                        }
                    }
                    if (Anchor.WindowHandle == WindowFromDragStart) {
                        fromAnchor = Anchor;
                    }
                });

                if (toAnchor != null) {
                    if (fromAnchor != null) {
                        fromAnchor.WindowHandle = HWND.Zero;
                    }
                    if (centerAnchor != null && toAnchor.WindowHandle != HWND.Zero) {
                        centerAnchor.WindowHandle = toAnchor.WindowHandle;
                    }
                    toAnchor.WindowHandle = WindowFromDragStart;
                }
            }

            //always proceed as though we werent here
            return Api.CallNextHookEx(HWND.Zero, code, wParam, lParam);
        }

    }
}
