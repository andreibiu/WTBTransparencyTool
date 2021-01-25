using System;
using System.Windows.Media;

namespace WTBTransparencyTool
{
    public class ColorChangedEventArgs: EventArgs
    {
        public Color NewColor { get; private set; }

        public ColorChangedEventArgs(Color color)
        {
            NewColor = color;
        }
    }

    public class AccentModeChangedEventArgs: EventArgs
    {
        public AccentMode NewAccentMode { get; private set; }

        public AccentModeChangedEventArgs(AccentMode accentMode)
        {
            NewAccentMode = accentMode;
        }
    }

    public class AccentColorSourceChangedEventArgs: EventArgs
    {
        public AccentColorSource NewAccentColorSource { get; private set; }

        public AccentColorSourceChangedEventArgs(AccentColorSource accentColorSource)
        {
            NewAccentColorSource = accentColorSource;
        }
    }

    public class BooleanValueChangedEventArgs: EventArgs
    {
        public bool NewValue { get; private set; }

        public BooleanValueChangedEventArgs(bool value)
        {
            NewValue = value;
        }
    }
}
