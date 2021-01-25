using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using Microsoft.Win32;

namespace WTBTransparencyTool
{
    public static class Utilities
    {
        #region Contants

        const string SystemColorKeyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Accent";
        const string SystemColorValueName = "AccentColorMenu";
        const string StartupLaunchKeyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\";
        const string StartupLaunchValueName = "WTBTransparencyTool";
        const int MinTransparencyValue = 0;
        const int MaxTransparencyValue = 100;
        const double TransparencyToAlphaCoefficient = (double) byte.MaxValue / MaxTransparencyValue;

        #endregion

        #region Components

        private static TypeConverter toColorConverter = TypeDescriptor.GetConverter(typeof(Color));

        #endregion

        #region Other Methods

        public static uint GetSystemColor() => (uint) (int) Registry.CurrentUser.OpenSubKey(SystemColorKeyName).GetValue(SystemColorValueName, 0x00000000);

        public static bool GetIsStartupLaunching()
        {
            string valueData = (Assembly.GetExecutingAssembly().CodeBase.Remove(0, 8).Insert(0, "\"") + "\"").Replace('/', '\\');
            return (string) Registry.CurrentUser.OpenSubKey(StartupLaunchKeyName).GetValue(StartupLaunchValueName, "") == valueData;
        }

        public static void SetIsStartupLaunching(bool value)
        {
            string valueData = (Assembly.GetExecutingAssembly().CodeBase.Remove(0, 8).Insert(0, "\"") + "\"").Replace('/', '\\');
            if (value) Registry.CurrentUser.OpenSubKey(StartupLaunchKeyName, true).SetValue(StartupLaunchValueName, valueData);
            else Registry.CurrentUser.OpenSubKey(StartupLaunchKeyName, true).DeleteValue(StartupLaunchValueName);
        }

        #endregion

        #region Conversion Methods

        public static Color ConvertToColor(uint colorUInt)
        {
            byte[] colorBytes = BitConverter.GetBytes(colorUInt);
            return Color.FromArgb(colorBytes[3], colorBytes[0], colorBytes[1], colorBytes[2]);
        }

        public static Color ConvertToColor(string colorString)
        {
            try
            {
                return (Color) toColorConverter.ConvertFromString(colorString);
            }
            catch
            {
                return Color.FromArgb(0, 0, 0, 0);
            }
        }

        public static uint ConvertToUInt(Color color) => BitConverter.ToUInt32(new byte[] { color.R, color.G, color.B, color.A }, 0);

        public static string ConvertToString(Color color) => "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");

        public static byte ConvertToTransparencyByte(string transparencyString)
        {
            double transparency;
            if (!double.TryParse(transparencyString, out transparency)) return MaxTransparencyValue;
            else
            {
                if (transparency > MaxTransparencyValue) return MaxTransparencyValue;
                else if (transparency < MinTransparencyValue) return MinTransparencyValue;
                else return (byte) transparency;
            }
        }

        public static byte ConvertToTransparencyByte(byte alpha)
        {
            return (byte) ((byte.MaxValue - alpha) / TransparencyToAlphaCoefficient);
        }

        public static byte ConvertToAlphaByte(byte transparency)
        {
            return (byte) (TransparencyToAlphaCoefficient * (MaxTransparencyValue - transparency)); 
        }

        #endregion
    }
}
