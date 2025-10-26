using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Linq;

namespace TimerClockApp
{
    public partial class ControlPanel : Window
    {
        private readonly ControlPanelViewModel _viewModel;
        private readonly DisplayWindow _displayWindow;

        public ControlPanel()
        {
            InitializeComponent();
            _viewModel = new ControlPanelViewModel();
            DataContext = _viewModel;

            // Set initial opacity from settings
            Opacity = Properties.Settings.Default.ControlOpacity;
            Topmost = Properties.Settings.Default.ControlTopmost;

            _displayWindow = new DisplayWindow(_viewModel);
            _displayWindow.Closed += DisplayWindow_Closed;
            this.Closed += ControlPanel_Closed;
            _displayWindow.Show();

        }




        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsPanel = new SettingsPanel
            {
                Owner = this
            };

            if (settingsPanel.ShowDialog() == true)
            {
                // Update settings immediately after save
                Opacity = Properties.Settings.Default.ControlOpacity;
                Topmost = Properties.Settings.Default.ControlTopmost;
            }
        }

        private void ControlPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            Opacity = 1.0;
        }

        private void ControlPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            Opacity = Properties.Settings.Default.ControlOpacity;
        }

        private void DisplayWindow_Closed(object? sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ControlPanel_Closed(object? sender, EventArgs e)
        {
            _viewModel.Dispose();
            Application.Current.Shutdown();
        }

        // Draggable Icon Logic
        private void DragHandle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // Minimize
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Close
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TimerInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out _))
            {
                e.Handled = true;
                return;
            }

            var textBox = sender as TextBox;
            string proposedText = textBox?.Text.Insert(textBox.CaretIndex, e.Text) ?? e.Text;

            if (int.TryParse(proposedText, out int value))
            {
                e.Handled = value < 1 || value > 200;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // No quick timer buttons to initialize
        }
    }
}