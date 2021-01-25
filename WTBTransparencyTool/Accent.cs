using System;
using System.Windows.Media;

namespace WTBTransparencyTool
{
    public enum AccentMode { Invalid = 0, Solid = 1, Transparent = 2, Acrylic = 3 };

    public enum AccentColorSource { System = 0, Custom = 1 };

    public class Accent
    {
        #region Readonly Values

        public static Accent DefaultAccent => FromAccentPolicy(TaskbarHelper.DefaultAcentPolicy);

        #endregion

        #region Properties

        public AccentMode Mode { get; set; }
        public Color Color { get; set; }

        #endregion

        #region Static Methods

        public static Accent FromAccentPolicy(AccentPolicy accentPolicy)
        {
            Accent accent = new Accent();

            switch (accentPolicy.AccentState)
            {
                case AccentState.ACCENT_ENABLE_GRADIENT:
                    accent.Mode = AccentMode.Solid;
                    break;
                case AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT:
                    accent.Mode = AccentMode.Transparent;
                    break;
                case AccentState.ACCENT_ENABLE_BLURBEHIND:
                    accent.Mode = AccentMode.Acrylic;
                    break;
                default:
                    accent.Mode = AccentMode.Invalid;
                    break;
            }

            accent.Color = Utilities.ConvertToColor(accentPolicy.GradientColor);

            return accent;
        }

        public static AccentPolicy ToAccentPolicy(Accent accent, AccentColorSource accentColorSource = AccentColorSource.Custom)
        {
            AccentPolicy accentPolicy = TaskbarHelper.AccentPolicy;

            switch (accent.Mode)
            {
                case AccentMode.Invalid:
                    break;
                case AccentMode.Solid:
                    accentPolicy.AccentState = AccentState.ACCENT_ENABLE_GRADIENT;
                    break;
                case AccentMode.Transparent:
                    accentPolicy.AccentState = AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
                    break;
                case AccentMode.Acrylic:
                    accentPolicy.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
                    break;
            }

            if (accentColorSource == AccentColorSource.Custom) accentPolicy.GradientColor = Utilities.ConvertToUInt(accent.Color);
            else accentPolicy.GradientColor = (uint)(accent.Color.A << 24) + ((accentPolicy.GradientColor << 8) >> 8);

            return accentPolicy;
        }

        #endregion
    }
}
