using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace WTBTransparencyTool
{
    public static class TaskbarHelper
    {
        #region Constants & Readonly Values

        const string TaskbarSystemClassName = "Shell_TrayWnd";
        const string SecondaryTaskbarSystemClassName = "Shell_SecondaryTrayWnd";
        const int WaitPeriodForUpdateHandlesInLoop = 500;

        public static AccentPolicy DefaultAcentPolicy => new AccentPolicy(AccentState.ACCENT_ENABLE_BLURBEHIND, (int) AccentFlags.ENABLE_COLORTINT, 0x00000000, 0);

        #endregion

        #region Components & Flags

        private static List<IntPtr> taskbarHwnds;
        private static AccentPolicy accentPolicy;
        private static WinCompatTrData winCompatTrData;
        private static HwndSource hookerHwndSource;

        private static bool handleSystemColorUpdates;
        private static bool applyToMultipleMonitors;

        #endregion

        #region Properties

        public static bool UseSystemColor
        {
            get => handleSystemColorUpdates;
            set => handleSystemColorUpdates = value;
        }

        public static bool ApplyOnMultipleMonitors
        {
            get => applyToMultipleMonitors;
            set
            {
                applyToMultipleMonitors = value;
                UpdateTaskbarHandles();
            }
        }

        public static AccentPolicy AccentPolicy
        {
            get => accentPolicy;
            set
            {
                accentPolicy = value;
                if (handleSystemColorUpdates) UpdateGradietColorFromSystem();
                UpdateWinCompatTrData(accentPolicy);
            }
        }

        #endregion

        #region Static Constructor

        static TaskbarHelper()
        {
            UpdateTaskbarHandles();
            accentPolicy = DefaultAcentPolicy;
            UpdateWinCompatTrData(DefaultAcentPolicy);
        }

        #endregion

        #region Methods

        private static void UpdateGradietColorFromSystem()
        {
            byte[] colorBytes = BitConverter.GetBytes(Utilities.GetSystemColor());
            colorBytes[3] = (byte) (accentPolicy.GradientColor >> 24);
            accentPolicy.GradientColor = BitConverter.ToUInt32(colorBytes, 0);
        }

        private static void UpdateWinCompatTrData(AccentPolicy accentPolicy)
        {
            int sizeOfPolicy = Marshal.SizeOf(accentPolicy);
            IntPtr policyPtr = Marshal.AllocHGlobal(sizeOfPolicy);
            Marshal.StructureToPtr(accentPolicy, policyPtr, false);

            TaskbarHelper.winCompatTrData = new WinCompatTrData(WindowCompositionAttribute.WCA_ACCENT_POLICY, policyPtr, sizeOfPolicy);
        }

        private static bool SameHwnds(List<IntPtr> lTaskbarHwnds, List<IntPtr> rTaskbarHwnds)
        {
            bool same = lTaskbarHwnds.Count == rTaskbarHwnds.Count;

            for (int index = 0; index < lTaskbarHwnds.Count && same; index++)
                same = same && lTaskbarHwnds[index] == rTaskbarHwnds[index];

            return same;
        }

        public static void UpdateTaskbarHandles()
        {
            taskbarHwnds = new List<IntPtr>(2);
            taskbarHwnds.Add(Externals.FindWindow(TaskbarSystemClassName, null));

            if (applyToMultipleMonitors)
            {
                IntPtr nextTaskbarHwnd = IntPtr.Zero;
                while (true)
                {
                    nextTaskbarHwnd = Externals.FindWindowEx(IntPtr.Zero, nextTaskbarHwnd, SecondaryTaskbarSystemClassName, "");
                    if (nextTaskbarHwnd == IntPtr.Zero) break;
                    else taskbarHwnds.Add(nextTaskbarHwnd);
                }
            }
        }

        private static void UpdateTaskbarHandlesInLoop()
        {
            int counter = 0;
            List<IntPtr> newTaskbarHwnds;

            while (true)
            {
                ++counter;
                newTaskbarHwnds = new List<IntPtr>(2);
                newTaskbarHwnds.Add(Externals.FindWindow(TaskbarSystemClassName, null));

                if (applyToMultipleMonitors)
                {
                    IntPtr nextTaskbarHwnd = IntPtr.Zero;
                    while (true)
                    {
                        nextTaskbarHwnd = Externals.FindWindowEx(IntPtr.Zero, nextTaskbarHwnd, SecondaryTaskbarSystemClassName, "");
                        if (nextTaskbarHwnd == IntPtr.Zero) break;
                        else newTaskbarHwnds.Add(nextTaskbarHwnd);
                    }
                }

                if (counter > 5 || !SameHwnds(taskbarHwnds, newTaskbarHwnds)) break;
                Thread.Sleep(WaitPeriodForUpdateHandlesInLoop);
            }

            taskbarHwnds = newTaskbarHwnds;
        }

        private static IntPtr UpdateOperationsHook(IntPtr Hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (message == SystemEvents.WM_TASKBARCREATED)
            {
                UpdateTaskbarHandles();
                handled = true;
            }
            else if (message == SystemEvents.WM_DISPLAYCHANGE)
            {
                UpdateTaskbarHandlesInLoop();
                handled = true;
            }
            else if (message == SystemEvents.WM_DWMCOLORIZATIONCOLORCHANGED && handleSystemColorUpdates)
            {
                UpdateGradietColorFromSystem();
                UpdateWinCompatTrData(accentPolicy);
                handled = true;
            }
            return IntPtr.Zero;
        }

        public static void HookUpdateOperations(Window window)
        {
            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            hookerHwndSource = HwndSource.FromHwnd(windowHandle);

            hookerHwndSource.AddHook(UpdateOperationsHook);
        }

        public static void ApplyAccentPolicy()
        {
            taskbarHwnds.ForEach(taskbarHwnd => Externals.SetWindowCompositionAttribute(taskbarHwnd, ref winCompatTrData));
        }

        #endregion
    }
}
