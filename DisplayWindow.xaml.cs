using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace TimerClockApp
{
    /// <summary>
    /// Interaction logic for DisplayWindow.xaml
    /// </summary>
    public partial class DisplayWindow : Window
    {
        private readonly ControlPanelViewModel _viewModel;

        public DisplayWindow(ControlPanelViewModel viewModel)
        {
            InitializeComponent();

            // Set initial opacity from settings
            Opacity = Properties.Settings.Default.DisplayOpacity;
            Topmost = Properties.Settings.Default.DisplayTopmost;

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;
        }

        private void DisplayWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            Opacity = 1.0;
        }

        private void DisplayWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            Opacity = Properties.Settings.Default.DisplayOpacity;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Add mouse hover events for opacity
            MouseEnter += DisplayWindow_MouseEnter;
            MouseLeave += DisplayWindow_MouseLeave;
        }

        // Draggable Icon Logic
        private void DragHandle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // Simple size tracking - just store the normal size
        private Size _normalSize = new(800, 450); // Default normal size from XAML

        private void ShrinkButton_Click(object sender, RoutedEventArgs e)
        {
            GoToMinimumSize();
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

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            GoToMaximizedSize();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreToNormalSize();
        }

        private bool IsMinimumSize()
        {
            const double tolerance = 5.0;
            return Math.Abs(Width - MinWidth) <= tolerance &&
                   Math.Abs(Height - MinHeight) <= tolerance;
        }

        private bool IsMaximized()
        {
            return WindowState == WindowState.Maximized;
        }

        private bool IsNormalSize()
        {
            return WindowState == WindowState.Normal && !IsMinimumSize();
        }

        private void GoToMinimumSize()
        {
            // Store current size if we're in normal state
            if (IsNormalSize())
            {
                _normalSize = new Size(Width, Height);
            }

            WindowState = WindowState.Normal;
            Width = MinWidth;
            Height = MinHeight;
        }

        private void GoToMaximizedSize()
        {
            // Store current size if we're in normal state
            if (IsNormalSize())
            {
                _normalSize = new Size(Width, Height);
            }

            WindowState = WindowState.Maximized;
        }

        private void RestoreToNormalSize()
        {
            WindowState = WindowState.Normal;
            Width = _normalSize.Width;
            Height = _normalSize.Height;
        }



        private void DragHandle_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Prevent default double-click behavior
            e.Handled = true;
        }

    }
}
