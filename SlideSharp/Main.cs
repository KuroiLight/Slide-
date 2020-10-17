using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DWORD = System.Int32;
using HWND = System.IntPtr;


namespace WindowShift
{
    public class Main
    {
        private readonly List<AnchorPoint> Anchors;
        private readonly HWND hMouseLLHook;
        private readonly Timer TaskScheduler = new Timer();
        private HWND WindowFromDragStart = HWND.Zero;
        private POINT FromDragStartPoint = new POINT();
        private POINT MouseLastPosition = new POINT();
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
            static bool ScreenAtPoint(int X, int Y)
            {
                for (var i = 0; i < Screen.AllScreens.Length; i++) {
                    if (Screen.AllScreens[i].WorkingArea.Contains(X, Y)) {
                        return true;
                    }
                }

                return false;
            }

            static bool isValidAnchorPoint(AnchorPoint AP)
            {
                if(!ScreenAtPoint(AP.AnchorPt.X, AP.AnchorPt.Y)) { //check to see if AP fall outside of work area (into taskbar)
                    return false;
                }

                return AP.Direction switch
                {
                    DragDirection.None => true, //center screen
                    DragDirection.Left => !ScreenAtPoint(AP.AnchorPt.X - 100, AP.AnchorPt.Y),
                    DragDirection.Right => !ScreenAtPoint(AP.AnchorPt.X + 100, AP.AnchorPt.Y),
                    DragDirection.Up => !ScreenAtPoint(AP.AnchorPt.X, AP.AnchorPt.Y - 100),
                    DragDirection.Down => !ScreenAtPoint(AP.AnchorPt.X, AP.AnchorPt.Y + 100),
                    _ => false,
                };
            }

            var tempAnchors = new List<AnchorPoint>();

            Screen.AllScreens.ToList().ForEach(curScreen => {
                foreach (DragDirection direction in Enum.GetValues(typeof(DragDirection))) {
                    var curAnchor = new AnchorPoint(direction, curScreen);
                    if (isValidAnchorPoint(curAnchor)) {
                        tempAnchors.Add(curAnchor);
                    }
                }
            });

            return tempAnchors;
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
            return (Math.Abs(vector.X) > Math.Abs(vector.Y)) switch
            {
                true when (vector.X > deadzone) => DragDirection.Left,
                true when (vector.X < -1 * deadzone) => DragDirection.Right,
                false when (vector.Y > deadzone) => DragDirection.Up,
                false when (vector.Y < -1 * deadzone) => DragDirection.Down,
                _ => DragDirection.None
            };
        }

        private AnchorPoint GetAnchorFrom(HWND hWindow)
        {
            return GetAnchorFrom(A => A.WindowHandle == hWindow);
        }

        private AnchorPoint GetAnchorFrom(POINT ScreenAt, DragDirection Direction)
        {
            return GetAnchorFrom(A => A.MonitorArea.Contains(ScreenAt) && A.Direction == Direction);
        }

        private AnchorPoint GetAnchorFrom(Func<AnchorPoint, bool> MatchPredicate)
        {
            for (var i = 0; i < Anchors.Count; i++) {
                if (MatchPredicate(Anchors[i])) {
                    return Anchors[i];
                }
            }

            return null;
        }

        private void UpdateTick(object sender, EventArgs e)
        {
            HWND WindowUnderCursor = WindowFrom(MouseLastPosition);
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
            if(wParam == Api.WM_MOUSE.WM_MOUSEMOVE) {
                MouseLastPosition = lParam.pt;
            } else if (wParam == Api.WM_MOUSE.WM_MBUTTONDOWN) {
                FromDragStartPoint = lParam.pt;
                WindowFromDragStart = WindowFrom(lParam.pt);
            } else if (wParam == Api.WM_MOUSE.WM_MBUTTONUP) {
                DragDirection dir = DirectionFromPts(FromDragStartPoint, lParam.pt);
                AnchorPoint centerAnchor = GetAnchorFrom(lParam.pt, DragDirection.None);
                AnchorPoint toAnchor = GetAnchorFrom(lParam.pt, dir);
                AnchorPoint fromAnchor = GetAnchorFrom(WindowFromDragStart);

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
