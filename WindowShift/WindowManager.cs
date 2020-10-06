using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DWORD = System.Int32;
using HWND = System.IntPtr;

namespace WindowShift
{
    internal class WindowManager
    {
        private POINT mmUpLast;
        public POINT mouseLastPoint;
        private WindowObj? windowSelected = null;
        private HWND lastHwnd = HWND.Zero;
        private EventType lastEvent = 0;

        private HWND rWINEVENTHOOK;
        private HWND rMOUSELLHOOK;

        private readonly List<WindowObj> Windows = new List<WindowObj>();

        #region pinvoke
        delegate void WinEventDelegate(IntPtr hWinEventHook, EventType eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        private delegate bool WNDENUMPROC(IntPtr hWnd, uint lParam);
        private delegate IntPtr HookProc(int code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(EventType eventMin, EventType eventMax, IntPtr
                                             hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
                                             uint idThread, uint dwFlags);
        private enum EventType : uint
        {
            EVENT_OBJECT_SHOW = 0x8002,
            EVENT_OBJECT_HIDE = 0x8003,
            EVENT_OBJECT_CREATE = 0x8000,
            EVENT_OBJECT_DESTROY = 0x8001,
            EVENT_OBJECT_FOCUS = 0x8005
        }

        private const int MMOUSEWHEEL = 0x020A;
        private const int WH_MOUSE_LL = 14;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern HWND CallNextHookEx(IntPtr hhk, int nCode, WM_MOUSE wParam, [In] MSLLHOOKSTRUCT lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }

        public enum WM_MOUSE : uint
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_MOUSEHWHEEL = 0x020E,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208
        }

        [DllImport("user32")]
        private static extern IntPtr GetWindowText(IntPtr hwnd, StringBuilder lptrString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern HWND WindowFromPoint(POINT p);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindowModuleFileName(IntPtr hwnd,
                                                   StringBuilder lpszFileName, uint cchFileNameMax);

        [DllImport("user32.dll")]
        private static extern IntPtr EnumWindows(WNDENUMPROC lpEnumFunc, uint lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowEnabled(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();
        #endregion

        public WindowManager()
        {
            bool OnWindowEnum(IntPtr hwnd, uint lparam)
            {
                if (IsWindowValid(hwnd)) {
                    Windows.Add(new WindowObj(hwnd));
                }

                return true;
            }

            EnumWindows(OnWindowEnum, 0);

            rWINEVENTHOOK = SetWinEventHook(EventType.EVENT_OBJECT_DESTROY, EventType.EVENT_OBJECT_SHOW, IntPtr.Zero, WinEventProc, 0, 0, 0x0000 | 0x0002);
            rMOUSELLHOOK = SetWindowsHookEx(WH_MOUSE_LL, MouseHookProc, HWND.Zero, 0);
        }

        ~WindowManager()
        {
            this.Windows.Clear();
            UnhookWindowsHookEx(rMOUSELLHOOK);
            UnhookWinEvent(rWINEVENTHOOK);
        }

        private void WinEventProc(IntPtr hWinEventHook, EventType e, HWND hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            //skip duplicate events
            if(lastEvent == e && lastHwnd == hwnd) {
                return;
            } else {
                lastHwnd = hwnd;
                lastEvent = e;
            }
            
            if (IsWindowValid(hwnd)) {
                switch(e) {
                    case EventType.EVENT_OBJECT_SHOW:
                        if(Windows.Find(w => w.hWnd == hwnd) != null) {
                            Windows.Add(new WindowObj(hwnd));
                        }
                        break;
                    case EventType.EVENT_OBJECT_DESTROY:
                        Windows.Remove(Windows.Find(w => w.hWnd == hwnd));
                        break;
                    case EventType.EVENT_OBJECT_FOCUS:
                        Windows.Find(w => w.hWnd == WindowFromPoint(this.mouseLastPoint) && w.dockedPoint != null).SlideOut();
                        break;
                }

                /*var stringContainer = new StringBuilder(512);
                GetWindowText(hwnd, stringContainer, 256);
                Debug.WriteLine(Enum.GetName(typeof(EventType), e) + " - " + stringContainer.ToString());*/
            }
        }

        private HWND MouseHookProc(DWORD code, WM_MOUSE wParam, MSLLHOOKSTRUCT lParam)
        {
            //Debug.WriteLine(Enum.GetName(typeof(WM_MOUSE), wParam));
            if (!wParam.HasFlag(WM_MOUSE.WM_MOUSEWHEEL)) {
                if (wParam.HasFlag(WM_MOUSE.WM_MOUSEMOVE)) {
                    this.mouseLastPoint = lParam.pt;
                } else if (wParam.HasFlag(WM_MOUSE.WM_MBUTTONDOWN)) {
                    windowSelected = Windows.Find(w => w.hWnd == WindowFromPoint(lParam.pt));
                    this.mmUpLast = lParam.pt;
                } else if (wParam.HasFlag(WM_MOUSE.WM_MBUTTONUP)) {
                    if(windowSelected != null) {
                        windowSelected.dockedPoint = new DOCKPOINT(this.mmUpLast, lParam.pt);
                        windowSelected = null;
                    }
                }
            }

            //always proceed as though we werent here
            return CallNextHookEx(HWND.Zero, code, wParam, lParam);
        }

        private static bool IsWindowValid(HWND hwnd)
        {
            if (!IsWindowEnabled(hwnd))
                return false;
            if (!IsWindowVisible(hwnd))
                return false;

            var stringContainer = new StringBuilder(512);
            GetWindowText(hwnd, stringContainer, 256);
            if (stringContainer.Length == 0) {
                return false;
            }
            stringContainer.Clear();

            GetWindowModuleFileName(hwnd, stringContainer, (uint)stringContainer.Capacity);
            if (stringContainer.ToString().EndsWith("shell32.dll", StringComparison.OrdinalIgnoreCase)) {
                return false;
            }
            stringContainer.Clear();

            return true;
        }

        private bool UnhookMouseHook(IntPtr hP)
        {
            return UnhookWindowsHookEx(hP);
        }

        /*[DllImport("user32.dll")]
        private static extern IntPtr GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);*/
    }
}
