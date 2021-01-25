using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace WTBTransparencyTool
{
    public class SettingsWindow: Window
    {
        #region Constants

        const double WindowWidth = 440;
        const double WindowHeight = 240;
        const double AllMargins = 10;
        const double CheckBoxWidth = WindowWidth - 2 * AllMargins;
        const double TextBlockWidth = 120;
        const double ComboBoxWidth = WindowWidth - 4 * AllMargins - TextBlockWidth;
        const double TextBoxWidth = (WindowWidth - 4 * AllMargins - TextBlockWidth) / 2;
        const double UIElementHeight = 20;
        const string StartupLaunchingCheckBoxText = "Launch application at system startup (all users)";
        const string MultipleMointorsCheckBoxText = "Apply accent on multiple monitors";

        #endregion

        #region Components & Flags

        Canvas content;
        CheckBox startupLaunchingCheckBox;
        CheckBox multipleMonitorsCheckBox;
        TextBlock accentModeTextBlock;
        ComboBox accentModeComboBox;
        TextBlock accentColorSourceTextBlock;
        ComboBox accentColorSourceComboBox;
        TextBlock accentColorTextBlock;
        TextBox accentColorTextBox;
        TextBlock transparencyTextBlock;
        TextBox transparencyTextBox;

        bool raiseEvents;
        bool textChanged;

        #endregion

        #region Events

        public event EventHandler StartupLaunchingChanged;
        public event EventHandler MultipleMonitorsChanged;
        public event EventHandler AccentModeChanged;
        public event EventHandler AccentColorSourceChanged;
        public event EventHandler AccentColorChanged;

        #endregion

        #region Constructor

        public SettingsWindow()
        {
            content = new Canvas();
            content.Width = WindowWidth;
            content.Height = WindowHeight;
            content.Focusable = true;
            content.Background = new SolidColorBrush { Color = SystemColors.WindowColor };
            content.MouseDown += delegate { content.Focus(); };

            startupLaunchingCheckBox = new CheckBox();
            startupLaunchingCheckBox.Width = CheckBoxWidth;
            startupLaunchingCheckBox.Height = UIElementHeight;
            startupLaunchingCheckBox.Content = StartupLaunchingCheckBoxText;
            startupLaunchingCheckBox.Checked += delegate { if (raiseEvents) { StartupLaunchingChanged.Invoke(this, new BooleanValueChangedEventArgs(true)); } };
            startupLaunchingCheckBox.Unchecked += delegate { if (raiseEvents) StartupLaunchingChanged.Invoke(this, new BooleanValueChangedEventArgs(false)); };
            Canvas.SetLeft(startupLaunchingCheckBox, AllMargins);
            Canvas.SetTop(startupLaunchingCheckBox, AllMargins);
            content.Children.Add(startupLaunchingCheckBox);

            multipleMonitorsCheckBox = new CheckBox();
            multipleMonitorsCheckBox.Width = CheckBoxWidth;
            multipleMonitorsCheckBox.Height = UIElementHeight;
            multipleMonitorsCheckBox.Content = MultipleMointorsCheckBoxText;
            multipleMonitorsCheckBox.Checked += delegate { if (raiseEvents) MultipleMonitorsChanged.Invoke(this, new BooleanValueChangedEventArgs(true)); };
            multipleMonitorsCheckBox.Unchecked += delegate { if (raiseEvents) MultipleMonitorsChanged.Invoke(this, new BooleanValueChangedEventArgs(false)); };
            Canvas.SetLeft(multipleMonitorsCheckBox, AllMargins);
            Canvas.SetTop(multipleMonitorsCheckBox, AllMargins + UIElementHeight + AllMargins);
            content.Children.Add(multipleMonitorsCheckBox);

            accentModeTextBlock = new TextBlock();
            accentModeTextBlock.Width = TextBlockWidth;
            accentModeTextBlock.Height = UIElementHeight;
            accentModeTextBlock.Text = "Accent Mode";
            Canvas.SetLeft(accentModeTextBlock, AllMargins);
            Canvas.SetTop(accentModeTextBlock, AllMargins + 2 * (UIElementHeight + AllMargins));
            content.Children.Add(accentModeTextBlock);

            accentModeComboBox = new ComboBox();
            accentModeComboBox.Width = ComboBoxWidth;
            accentModeComboBox.Height = UIElementHeight;
            accentModeComboBox.ItemsSource = new ArraySegment<AccentMode>((AccentMode[]) Enum.GetValues(typeof(AccentMode)), 1, 3);
            accentModeComboBox.SelectionChanged += delegate {
                AccentMode accentMode = (AccentMode) accentModeComboBox.SelectedItem;
                transparencyTextBox.IsEnabled = accentMode != AccentMode.Solid;
                if (raiseEvents)
                    AccentModeChanged.Invoke(this, new AccentModeChangedEventArgs(accentMode));
            };
            Canvas.SetLeft(accentModeComboBox, AllMargins + TextBlockWidth + AllMargins);
            Canvas.SetTop(accentModeComboBox, AllMargins + 2 * (UIElementHeight + AllMargins));
            content.Children.Add(accentModeComboBox);

            accentColorSourceTextBlock = new TextBlock();
            accentColorSourceTextBlock.Width = TextBlockWidth;
            accentColorSourceTextBlock.Height = UIElementHeight;
            accentColorSourceTextBlock.Text = "Accent Color Source";
            Canvas.SetLeft(accentColorSourceTextBlock, AllMargins);
            Canvas.SetTop(accentColorSourceTextBlock, AllMargins + 3 * (UIElementHeight + AllMargins));
            content.Children.Add(accentColorSourceTextBlock);

            accentColorSourceComboBox = new ComboBox();
            accentColorSourceComboBox.Width = ComboBoxWidth;
            accentColorSourceComboBox.Height = UIElementHeight;
            accentColorSourceComboBox.ItemsSource = Enum.GetValues(typeof(AccentColorSource));
            accentColorSourceComboBox.SelectionChanged += delegate {
                AccentColorSource accentColorSource = (AccentColorSource) accentColorSourceComboBox.SelectedItem;
                accentColorTextBlock.IsEnabled = accentColorSource != AccentColorSource.System;
                accentColorTextBox.IsEnabled = accentColorSource != AccentColorSource.System;
                if (raiseEvents)
                    AccentColorSourceChanged.Invoke(this, new AccentColorSourceChangedEventArgs(accentColorSource));
            };
            Canvas.SetLeft(accentColorSourceComboBox, AllMargins + TextBlockWidth + AllMargins);
            Canvas.SetTop(accentColorSourceComboBox, AllMargins + 3 * (UIElementHeight + AllMargins));
            content.Children.Add(accentColorSourceComboBox);

            accentColorTextBlock = new TextBlock();
            accentColorTextBlock.Width = TextBlockWidth;
            accentColorTextBlock.Height = UIElementHeight;
            accentColorTextBlock.Text = "Accent Color";
            Canvas.SetLeft(accentColorTextBlock, AllMargins);
            Canvas.SetTop(accentColorTextBlock, AllMargins + 4 * (UIElementHeight + AllMargins));
            content.Children.Add(accentColorTextBlock);

            accentColorTextBox = new TextBox();
            accentColorTextBox.Width = TextBoxWidth;
            accentColorTextBox.Height = UIElementHeight;
            accentColorTextBox.TextChanged += delegate { textChanged = true; };
            accentColorTextBox.KeyDown += (object sender, KeyEventArgs keyEventArgs) => {
                if (keyEventArgs.Key == Key.Enter && textChanged)
                {
                    textChanged = false;
                    Color color = Utilities.ConvertToColor(accentColorTextBox.Text);
                    color.A = Utilities.ConvertToAlphaByte(byte.Parse(transparencyTextBox.Text));
                    accentColorTextBox.Text = Utilities.ConvertToString(color);
                    if (raiseEvents) AccentColorChanged.Invoke(this, new ColorChangedEventArgs(color));
                }
            };
            accentColorTextBox.LostFocus += delegate {
                if (textChanged)
                {
                    textChanged = false;
                    Color color = Utilities.ConvertToColor(accentColorTextBox.Text);
                    color.A = Utilities.ConvertToAlphaByte(byte.Parse(transparencyTextBox.Text));
                    accentColorTextBox.Text = Utilities.ConvertToString(color);
                    if (raiseEvents) AccentColorChanged.Invoke(this, new ColorChangedEventArgs(color));
                }
            };
            Canvas.SetLeft(accentColorTextBox, AllMargins + TextBlockWidth + AllMargins);
            Canvas.SetTop(accentColorTextBox, AllMargins + 4 * (UIElementHeight + AllMargins));
            content.Children.Add((accentColorTextBox));

            transparencyTextBlock = new TextBlock();
            transparencyTextBlock.Width = TextBlockWidth;
            transparencyTextBlock.Height = UIElementHeight;
            transparencyTextBlock.Text = "Transparency";
            Canvas.SetLeft(transparencyTextBlock, AllMargins);
            Canvas.SetTop(transparencyTextBlock, AllMargins + 5 * (UIElementHeight + AllMargins));
            content.Children.Add(transparencyTextBlock);

            transparencyTextBox = new TextBox();
            transparencyTextBox.Width = TextBoxWidth;
            transparencyTextBox.Height = UIElementHeight;
            transparencyTextBox.TextChanged += delegate { textChanged = true; };
            transparencyTextBox.KeyDown += (object sender, KeyEventArgs keyEventArgs) => {
                if (keyEventArgs.Key == Key.Enter && textChanged)
                {
                    textChanged = false;
                    byte transparency = Utilities.ConvertToTransparencyByte(transparencyTextBox.Text);
                    transparencyTextBox.Text = transparency.ToString();
                    Color color = Utilities.ConvertToColor(accentColorTextBox.Text);
                    color.A = Utilities.ConvertToAlphaByte(transparency);
                    if (raiseEvents) AccentColorChanged.Invoke(this, new ColorChangedEventArgs(color));
                }
            };
            transparencyTextBox.LostFocus += delegate {
                if (textChanged)
                {
                    textChanged = false;
                    byte transparency = Utilities.ConvertToTransparencyByte(transparencyTextBox.Text);
                    transparencyTextBox.Text = transparency.ToString();
                    Color color = Utilities.ConvertToColor(accentColorTextBox.Text);
                    color.A = Utilities.ConvertToAlphaByte(transparency);
                    if (raiseEvents) AccentColorChanged.Invoke(this, new ColorChangedEventArgs(color));
                }
            };
            Canvas.SetLeft(transparencyTextBox, AllMargins + TextBlockWidth + AllMargins);
            Canvas.SetTop(transparencyTextBox, AllMargins + 5 * (UIElementHeight + AllMargins));
            content.Children.Add(transparencyTextBox);

            this.Title = Application.Name + " Settings";
            this.Width = WindowWidth;
            this.Height = WindowHeight;
            this.Visibility = Visibility.Collapsed;
            this.ResizeMode = ResizeMode.CanMinimize;
            this.Closing += (object sender, CancelEventArgs cancelEventArgs) => { cancelEventArgs.Cancel = true; this.Visibility = Visibility.Collapsed; };
            this.Content = content;
            raiseEvents = false;
        }

        #endregion

        #region Methods

        public void Load(Settings settings)
        {
            startupLaunchingCheckBox.IsChecked = settings.IsStartupLaunching;
            multipleMonitorsCheckBox.IsChecked = settings.ApplyOnMultipleMonitors;
            accentModeComboBox.SelectedValue = settings.Accent.Mode;
            accentColorSourceComboBox.SelectedValue = settings.AccentColorSource;
            accentColorTextBox.Text = Utilities.ConvertToString(settings.Accent.Color);
            transparencyTextBox.Text =  Utilities.ConvertToTransparencyByte(settings.Accent.Color.A).ToString();

            Show();
            this.Visibility = Visibility.Collapsed;
            raiseEvents = true;
        }

        public void Open()
        {
            this.Visibility = Visibility.Visible;
        }

        #endregion
    }
}
