using System.Windows;

namespace TimerClockApp
{
    public partial class SettingsPanel : Window
    {
        public SettingsPanel()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            ControlOpacitySlider.Value = Properties.Settings.Default.ControlOpacity * 100;
            DisplayOpacitySlider.Value = Properties.Settings.Default.DisplayOpacity * 100;
            WarningTimeTextBox.Text = Properties.Settings.Default.WarningTime.ToString();
            AutoSwitchIntervalTextBox.Text = Properties.Settings.Default.AutoSwitchInterval.ToString();
            ControlTopmostCheckbox.IsChecked = Properties.Settings.Default.ControlTopmost;
            DisplayTopmostCheckbox.IsChecked = Properties.Settings.Default.DisplayTopmost;

            // Force owner to 100% opacity while settings are open
            if (Owner != null)
                Owner.Opacity = 1.0;
        }

        private void WarningTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(WarningTimeTextBox.Text, out int value))
                value = 30; // Default

            WarningTimeTextBox.Text = Math.Clamp(value, 10, 300).ToString();
        }

        private void AutoSwitchIntervalTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(AutoSwitchIntervalTextBox.Text, out int value))
                value = 10; // Default
            AutoSwitchIntervalTextBox.Text = Math.Clamp(value, 5, 60).ToString();
        }

        private void ControlOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Preview Control Panel opacity
            if (Owner != null)
                Owner.Opacity = ControlOpacitySlider.Value / 100.0;
        }

        private void DisplayOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Preview Display Window opacity
            var displayWindow = Application.Current.Windows.OfType<DisplayWindow>().FirstOrDefault();
            if (displayWindow != null)
                displayWindow.Opacity = DisplayOpacitySlider.Value / 100.0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save settings
            Properties.Settings.Default.ControlOpacity = ControlOpacitySlider.Value / 100.0;
            Properties.Settings.Default.DisplayOpacity = DisplayOpacitySlider.Value / 100.0;
            Properties.Settings.Default.WarningTime = int.Parse(WarningTimeTextBox.Text);
            Properties.Settings.Default.AutoSwitchInterval = int.Parse(AutoSwitchIntervalTextBox.Text);
            Properties.Settings.Default.ControlTopmost = ControlTopmostCheckbox.IsChecked ?? false;
            Properties.Settings.Default.DisplayTopmost = DisplayTopmostCheckbox.IsChecked ?? false;
            Properties.Settings.Default.Save();

            // Update the DisplayWindow's Topmost property immediately
            var displayWindow = Application.Current.Windows.OfType<DisplayWindow>().FirstOrDefault();
            if (displayWindow != null)
            {
                displayWindow.Topmost = Properties.Settings.Default.DisplayTopmost;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}