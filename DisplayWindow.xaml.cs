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

        // Manual drag state
        private bool _isDragging;
        private Point _dragOffset; // cursor offset from window top-left in logical pixels at drag start
        private const double TopSnapThreshold = 10.0; // px from top of screen to trigger maximize

        // Returns cursor position in logical screen coordinates (DPI-aware via WPF)
        private Point CursorScreenPos() =>
            new(Left + Mouse.GetPosition(this).X, Top + Mouse.GetPosition(this).Y);

        private void DragHandle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            // If maximized, restore first and re-anchor window under cursor
            if (IsMaximized())
            {
                var cursor = CursorScreenPos();
                RestoreToNormalSize();
                Left = cursor.X - _normalSize.Width / 2;
                Top = cursor.Y - 17; // mid-height of drag bar
            }

            // Record cursor offset from window origin (after any restore/reposition)
            _dragOffset = Mouse.GetPosition(this);
            _isDragging = true;
            CaptureMouse(); // capture on the Window, not the Button
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_isDragging) return;
            var cursor = CursorScreenPos();
            Left = cursor.X - _dragOffset.X;
            Top = cursor.Y - _dragOffset.Y;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!_isDragging) return;
            _isDragging = false;
            ReleaseMouseCapture();
            if (Top <= TopSnapThreshold)
                GoToMaximizedSize();
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            _isDragging = false;
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
