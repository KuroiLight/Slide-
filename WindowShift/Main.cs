using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
        }

        ~Main()
        {
            Api.UnhookWinEvent(hMouseLLHook);
        }

        private void FindAllAnchorPoints()
        {
            //Screen.AllScreens.
        }

        private void TransitionWindow(HWND window, POINT pt)
        {
            //SetWindowPos?
        }

        private HWND WindowFromCursor()
        {
            Api.GetCursorPos(out POINT cPos);
            return Api.WindowFromPoint(cPos); //childwindowfrompointex
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


        private POINT mmUpLast;
        public POINT mouseLastPoint;
        private HWND lastHwnd = HWND.Zero;


        private HWND MButtonWindow = HWND.Zero;
        private POINT MButtonStartPoint;

        private HWND MouseHookProc(DWORD code, Api.WM_MOUSE wParam, Api.MSLLHOOKSTRUCT lParam)
        {
            if (!wParam.HasFlag(Api.WM_MOUSE.WM_MOUSEWHEEL)) {
                if (wParam.HasFlag(Api.WM_MOUSE.WM_MBUTTONDOWN)) {
                    MButtonStartPoint = lParam.pt;

                    MessageBox.Show(Api.ChildWindowFromPointEx(HWND.Zero, lParam.pt, 0).ToString());
                } else if (wParam.HasFlag(Api.WM_MOUSE.WM_MBUTTONUP)) {
                    //MessageBox.Show(Enum.GetName(typeof(DragDirection), DirectionFromPts(MButtonStartPoint, lParam.pt)));
                }
            }

            //always proceed as though we werent here
            return Api.CallNextHookEx(HWND.Zero, code, wParam, lParam);
        }

    }
}
