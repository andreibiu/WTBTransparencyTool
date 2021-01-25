using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace WTBTransparencyTool
{
    public sealed class Application: System.Windows.Application
    {
        #region Constants

        public const string Name = "WTBTransparencyTool";
        private const int ApplyingPeriod = 10;

        #endregion

        #region Static Properties

        public new static Application Current { get; private set; }

        #endregion

        #region Static Constructor

        static Application()
        {
            Current = new Application();
        }

        #endregion

        #region Properties & Components & Flags

        public Settings Settings { get; private set; }
        public NotifyIcon TrayIcon { get; private set; }
        public Thread WorkingThread { get; private set; }
        public SettingsWindow SettingsWindow { get; private set; }

        bool paused = false;

        #endregion

        #region Constructor

        Application(): base()
        {
            Settings = new Settings();

            TaskbarHelper.ApplyOnMultipleMonitors = Settings.ApplyOnMultipleMonitors;

            SettingsWindow = new SettingsWindow();
            SettingsWindow.StartupLaunchingChanged += OnStartupLaunchingChanged;
            SettingsWindow.MultipleMonitorsChanged += OnMultipleMonitorsChanged;
            SettingsWindow.AccentModeChanged += OnAccentModeChanged;
            SettingsWindow.AccentColorSourceChanged += OnAccentColorSourceChanged;
            SettingsWindow.AccentColorChanged += OnAccentColorChanged;
            SettingsWindow.Closing += OnSettingsWindowClosing;

            WorkingThread = new Thread(() => { while (true) { TaskbarHelper.ApplyAccentPolicy(); Thread.Sleep(ApplyingPeriod); } });

            TrayIcon = new NotifyIcon() {
                Text = "WTBTransparencyTool\nVersion 1.0.0.0",
                Icon = new System.Drawing.Icon(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Remove(0, 8)) + "\\WTBTransparencyTool.ico"),
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Start/Pause", OnStartPauseSelected),
                    new MenuItem("Refresh", OnRefresh),
                    new MenuItem("Settings", OnOpenSelected),
                    new MenuItem("Exit", OnExitSelected)
                })
            };
        }

        #endregion

        #region Event Handlers

        private void OnOpenSelected(object seder, EventArgs eventArgs)
        {
            Current.TrayIcon.ContextMenu.MenuItems[2].Enabled = false;
            Current.SettingsWindow.Open();
        }

        private void OnStartPauseSelected(object seder, EventArgs eventArgs)
        {
            if (paused)
            {
                Current.WorkingThread.Resume();
                paused = false;
                TrayIcon.ContextMenu.MenuItems[1].Enabled = true;
            }
            else
            {
                Current.WorkingThread.Suspend();
                paused = true;
                TrayIcon.ContextMenu.MenuItems[1].Enabled = false;
            }
        }

        private void OnExitSelected(object seder, EventArgs eventArgs)
        {
            Shutdown();
        }

        private void OnStartupLaunchingChanged(object seder, EventArgs eventArgs)
        {
            Settings.IsStartupLaunching = (eventArgs as BooleanValueChangedEventArgs).NewValue;
        }

        private void OnMultipleMonitorsChanged(object seder, EventArgs eventArgs)
        {
            WorkingThread.Suspend();
            Settings.ApplyOnMultipleMonitors = (eventArgs as BooleanValueChangedEventArgs).NewValue;
            TaskbarHelper.ApplyOnMultipleMonitors = (eventArgs as BooleanValueChangedEventArgs).NewValue;
            WorkingThread.Resume();
        }

        private void OnAccentModeChanged(object seder, EventArgs eventArgs)
        {
            WorkingThread.Suspend();
            Settings.Accent.Mode = (eventArgs as AccentModeChangedEventArgs).NewAccentMode;
            TaskbarHelper.AccentPolicy = Accent.ToAccentPolicy(Settings.Accent, Settings.AccentColorSource);
            WorkingThread.Resume();
        }

        private void OnAccentColorSourceChanged(object seder, EventArgs eventArgs)
        {
            WorkingThread.Suspend();
            Settings.AccentColorSource = (eventArgs as AccentColorSourceChangedEventArgs).NewAccentColorSource;
            TaskbarHelper.UseSystemColor = (Settings.AccentColorSource == AccentColorSource.System);
            TaskbarHelper.AccentPolicy = Accent.ToAccentPolicy(Settings.Accent, Settings.AccentColorSource);
            WorkingThread.Resume();
        }

        private void OnAccentColorChanged(object seder, EventArgs eventArgs)
        {
            WorkingThread.Suspend();
            Settings.Accent.Color = (eventArgs as ColorChangedEventArgs).NewColor;
            TaskbarHelper.AccentPolicy = Accent.ToAccentPolicy(Settings.Accent, Settings.AccentColorSource);
            WorkingThread.Resume();
        }

        private void OnSettingsWindowClosing(object seder, EventArgs eventArgs)
        {
            TrayIcon.ContextMenu.MenuItems[2].Enabled = true;
            Settings.Save();
        }

        private void OnRefresh(object sender, EventArgs eventArgs)
        {
            WorkingThread.Suspend();
            TaskbarHelper.UpdateTaskbarHandles();
            TaskbarHelper.AccentPolicy = Accent.ToAccentPolicy(Settings.Accent, Settings.AccentColorSource);
            WorkingThread.Resume();
        }

        #endregion

        #region Methods

        public new void Run()
        {
            Settings.Reload();

            SettingsWindow.Load(Settings);

            TaskbarHelper.UseSystemColor = (Settings.AccentColorSource == AccentColorSource.System);
            TaskbarHelper.AccentPolicy = Accent.ToAccentPolicy(Settings.Accent, Settings.AccentColorSource);
            TaskbarHelper.HookUpdateOperations(Current.SettingsWindow);
            WorkingThread.Start();

            TrayIcon.Visible = true;
            base.Run();
        }

        public new void Shutdown()
        {
            if (paused) WorkingThread.Resume();
            WorkingThread.Abort();
            base.Shutdown();
        }

        #endregion

        [STAThread]
        static void Main()
        {
            Application.Current.Run();
        }
    }
}
