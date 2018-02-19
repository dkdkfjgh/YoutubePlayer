using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

public static class MouseHook
{
    public static event EventHandler MouseActionDown = delegate { };
    public static event EventHandler MouseActionUp = delegate { };
    public static event EventHandler MouseActionMove = delegate { };

    public static void Start()
    {
        _hookID = SetHook(_proc);
    }
    public static void stop()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private static LowLevelMouseProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    private static IntPtr SetHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc,
              GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);
    
    
    private static IntPtr HookCallback(int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam)
    {
        //down과 up을 동시에 막아줘야 한다. up만 막으면 응용프로그램에서 down메시지 수신 후 up을 기다리는 루프가 돌아
        //특정 프로그램이 포커스나 마우스 클릭 동작을 빼앗아감
        if (nCode >= 0)
        {
            //if (MouseMessages.WM_XBUTTONDOWN == (MouseMessages)wParam && HiWord(lParam.mouseData) == (int)MouseMessages.XBUTTON1)//앞으로가기 버튼
            if (MouseMessages.WM_MBUTTONDOWN == (MouseMessages)wParam)
            {
                if (youtubePlayer.Form1.isLoading == false)
                {
                    if (youtubePlayer.Form1.mainFrm.Opacity == 0)
                    {
                        youtubePlayer.Form1.mainFrm.Opacity = youtubePlayer.Form1.preOpacity;
                    }
                    else
                    {
                        youtubePlayer.Form1.preOpacity = youtubePlayer.Form1.mainFrm.Opacity;
                        youtubePlayer.Form1.mainFrm.Opacity = 0;
                        Debug.WriteLine("2");
                    }
                }
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, ref lParam);
    }

    private const int WH_MOUSE_LL = 14;

    static int HiWord(uint Number)
    {
        return (ushort)((Number >> 16) & 0xffff);
    }

    private enum MouseMessages
    {
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_MOUSEMOVE = 0x0200,
        WM_MOUSEWHEEL = 0x020A,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205,
        WM_XBUTTONUP = 0x020C,
        WM_XBUTTONDOWN = 0x020B,
        XBUTTON1 = 1,
        XBUTTON2 = 2,
        XBUTTON3 = 3,
        WM_MBUTTONDOWN = 0x207
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
      LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}
