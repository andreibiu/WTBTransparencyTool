using System;
using System.Runtime.InteropServices;

namespace WTBTransparencyTool
{    
    #region System Constants

    public static class Constants
    {
        public const int MONITOR_DEFAULTTONULL = 0;
        public const int MONITOR_DEFAULTTOPRIMARY = 1;
        public const int MONITOR_DEFAULTTONEAREST = 2;
    }

    #endregion

    #region System Enums

    public enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    public enum AccentFlags
    {
        NONE = 0,
        ENABLE_COLORTINT = 2
    }

    public enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }

    #endregion

    #region System Other Constant Values

    public static class SystemEvents
    {
        public static readonly uint WM_TASKBARCREATED = Externals.RegisterWindowMessage("TaskbarCreated");
        public static readonly uint WM_DISPLAYCHANGE = 0x7E;
        public static readonly uint WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;
    }

    #endregion

    #region System Structs

    [StructLayout(LayoutKind.Sequential)]
    public struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public uint GradientColor;
        public int AnimationId;

        public AccentPolicy(AccentState accentState, int accentFlags, uint gradientColor, int animationId)
        {
            AccentState = accentState;
            AccentFlags = accentFlags;
            GradientColor = gradientColor;
            AnimationId = animationId;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WinCompatTrData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;

        public WinCompatTrData(WindowCompositionAttribute attribute, IntPtr data, int sizeOfData)
        {
            Attribute = attribute;
            Data = data;
            SizeOfData = sizeOfData;
        }
    }

    #endregion

    #region System Delegate Types

    public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    #endregion

    #region Imported System Methods (@ DllImport-ed)

    public static class Externals
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WinCompatTrData data);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint RegisterWindowMessage(string msgString);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
    }

    #endregion
}
