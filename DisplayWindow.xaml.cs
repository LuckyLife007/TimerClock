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
        private Storyboard? _warningStoryboard;
        private Storyboard? _negativeStoryboard;

        public DisplayWindow(ControlPanelViewModel viewModel)
        {
            InitializeComponent();

            // Set initial opacity from settings
            Opacity = Properties.Settings.Default.DisplayOpacity;
            Topmost = Properties.Settings.Default.DisplayTopmost;

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;
            SetupAnimations();
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
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateAnimationStates();

            // Add mouse hover events for opacity
            MouseEnter += DisplayWindow_MouseEnter;
            MouseLeave += DisplayWindow_MouseLeave;
        }

        private void UpdateIndicatorOpacity(Border? indicator, double opacity)
        {
            if (indicator != null)
                indicator.Opacity = opacity;
        }

        private void SetupAnimations()
        {
            // Setup warning animation (blink + pause)
            _warningStoryboard = new Storyboard();

            var warningIndicator = this.FindName("WarningTimeIndicator") as FrameworkElement;
            if (warningIndicator != null)
            {
                var warningAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    AutoReverse = true
                };
                Storyboard.SetTarget(warningAnimation, warningIndicator);
                Storyboard.SetTargetProperty(warningAnimation, new PropertyPath(UIElement.OpacityProperty));

                var warningPause = new DoubleAnimation
                {
                    From = 0,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(3)),
                    BeginTime = TimeSpan.FromSeconds(1)  // Start after warningAnimation completes
                };
                Storyboard.SetTarget(warningPause, warningIndicator);
                Storyboard.SetTargetProperty(warningPause, new PropertyPath(UIElement.OpacityProperty));

                _warningStoryboard.Children.Add(warningAnimation);
                _warningStoryboard.Children.Add(warningPause);
                _warningStoryboard.RepeatBehavior = RepeatBehavior.Forever;
            }

            // Setup negative animation
            _negativeStoryboard = new Storyboard();

            if (this.FindName("NegativeTimeIndicator") is FrameworkElement negativeIndicator)
            {
                var negativeAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                Storyboard.SetTarget(negativeAnimation, negativeIndicator);
                Storyboard.SetTargetProperty(negativeAnimation, new PropertyPath(UIElement.OpacityProperty));

                _negativeStoryboard.Children.Add(negativeAnimation);
            }

            _warningStoryboard?.Begin();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => HandlePropertyChange(e.PropertyName));
                return;
            }
            HandlePropertyChange(e.PropertyName);
        }

        private void HandlePropertyChange(string? propertyName)
        {
            switch (propertyName)
            {
                case nameof(ControlPanelViewModel.ShowingClock):
                    UpdateAnimationStates();
                    break;
                case nameof(ControlPanelViewModel.IsNegative):
                case nameof(ControlPanelViewModel.IsWarning):
                    if (!_viewModel.ShowingClock)
                    {
                        UpdateAnimationStates();
                    }
                    break;
            }
        }

        private void UpdateAnimationStates()
        {
            if (_viewModel.ShowingClock || !(_viewModel.IsTimerRunning && !_viewModel.IsPaused))
            {
                StopAllAnimations();
                return;
            }

            if (_viewModel.IsNegative)
            {
                StartNegativeAnimation();
                StopWarningAnimation();
            }
            else if (_viewModel.IsWarning)
            {
                StopNegativeAnimation();
                StartWarningAnimation();
            }
            else
            {
                StopAllAnimations();
            }
        }

        private void StartNegativeAnimation()
        {
            if (_negativeStoryboard != null)
            {
                _negativeStoryboard.Begin();
            }
        }

        private void StartWarningAnimation()
        {
            if (_warningStoryboard != null)
            {
                _warningStoryboard.Begin();
            }
        }

        private void StopNegativeAnimation()
        {
            _negativeStoryboard?.Stop();
            UpdateIndicatorOpacity(this.FindName("NegativeTimeIndicator") as Border, 0);
        }

        private void StopWarningAnimation()
        {
            _warningStoryboard?.Stop();
            UpdateIndicatorOpacity(this.FindName("WarningTimeIndicator") as Border, 0);
        }

        private void StopAllAnimations()
        {
            StopNegativeAnimation();
            StopWarningAnimation();
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
