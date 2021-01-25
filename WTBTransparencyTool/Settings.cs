using System;
using System.Configuration;

namespace WTBTransparencyTool
{
    public sealed class Settings: ApplicationSettingsBase
    {
        [UserScopedSetting()]
        public Accent Accent
        {
            get => (Accent) this["Accent"];
            set => this["Accent"] = value;
        }

        [UserScopedSetting()]
        [DefaultSettingValue("Custom")]
        public AccentColorSource AccentColorSource
        {
            get => (AccentColorSource) this["AccentColorSource"];
            set => this["AccentColorSource"] = value;
        }

        [UserScopedSetting()]
        public bool IsStartupLaunching
        {
            get => Utilities.GetIsStartupLaunching();
            set => Utilities.SetIsStartupLaunching(value);
        }

        [UserScopedSetting()]
        [DefaultSettingValue("false")]
        public bool ApplyOnMultipleMonitors
        {
            get => (bool) this["ApplyOnMultipleMonitors"];
            set => this["ApplyOnMultipleMonitors"] = value;
        }

        public Settings(): base()
        {
            if (Accent == null)
            {
                Accent = Accent.DefaultAccent;
                Save();
            }
        }
    }
}
