using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace TimerClockApp
{
    public partial class DisplayWindow : Window
    {
        private readonly ControlPanelViewModel _viewModel;
        private Storyboard? _warningStoryboard;
        private Storyboard? _negativeStoryboard;

        public DisplayWindow(ControlPanelViewModel viewModel)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;

            // Set initial opacity from settings
            Opacity = Properties.Settings.Default.DisplayOpacity;
            Topmost = Properties.Settings.Default.DisplayTopmost;

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;
            SetupAnimations();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            MaximizeButton.Content = "⧉"; // Set initial content for maximized state
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateAnimationStates();

            // Add mouse hover events for opacity
            MouseEnter += DisplayWindow_MouseEnter;
            MouseLeave += DisplayWindow_MouseLeave;
        }

        private void DisplayWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Opacity = 1.0;
        }

        private void DisplayWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Opacity = Properties.Settings.Default.DisplayOpacity;
        }

        private void SetupAnimations()
        {
            // Setup warning animation (blink + pause)
            _warningStoryboard = new Storyboard();

            var warningAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                AutoReverse = true
            };
            Storyboard.SetTarget(warningAnimation, WarningTimeIndicator);
            Storyboard.SetTargetProperty(warningAnimation, new PropertyPath(UIElement.OpacityProperty));

            var warningPause = new DoubleAnimation
            {
                From = 0,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(3)),
                BeginTime = TimeSpan.FromSeconds(1)  // Start after warningAnimation completes
            };
            Storyboard.SetTarget(warningPause, WarningTimeIndicator);
            Storyboard.SetTargetProperty(warningPause, new PropertyPath(UIElement.OpacityProperty));

            _warningStoryboard.Children.Add(warningAnimation);
            _warningStoryboard.Children.Add(warningPause);
            _warningStoryboard.RepeatBehavior = RepeatBehavior.Forever;

            _warningStoryboard.Begin();

            // Setup negative animation
            _negativeStoryboard = new Storyboard();

            var negativeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget(negativeAnimation, NegativeTimeIndicator);
            Storyboard.SetTargetProperty(negativeAnimation, new PropertyPath(UIElement.OpacityProperty));

            _negativeStoryboard.Children.Add(negativeAnimation);
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
            System.Diagnostics.Debug.WriteLine($"QQQ DisplayWindow property changed: {propertyName}");
            switch (propertyName)
            {
                case nameof(ControlPanelViewModel.ShowingClock):
                    UpdateAnimationStates();
                    break;
                case nameof(ControlPanelViewModel.IsNegative):
                case nameof(ControlPanelViewModel.IsWarning):
                    if (!_viewModel.ShowingClock)
                    {
                        System.Diagnostics.Debug.WriteLine($"QQQ Updating animations - IsWarning: {_viewModel.IsWarning}, ShowingClock: {_viewModel.ShowingClock}");
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
            System.Diagnostics.Debug.WriteLine("QQQ StartNegativeAnimation - Storyboard null? " + (_negativeStoryboard == null));
            if (_negativeStoryboard != null)
            {
                System.Diagnostics.Debug.WriteLine("QQQ Starting negative storyboard");
                _negativeStoryboard.Begin();
                System.Diagnostics.Debug.WriteLine("QQQ Negative storyboard started");
            }
        }

        private void StartWarningAnimation()
        {
            System.Diagnostics.Debug.WriteLine("QQQ StartWarningAnimation - Storyboard null? " + (_warningStoryboard == null));
            if (_warningStoryboard != null)
            {
                System.Diagnostics.Debug.WriteLine("QQQ Starting warning storyboard");
                _warningStoryboard.Begin();
                System.Diagnostics.Debug.WriteLine("QQQ Warning storyboard started");
            }
        }

        private void StopNegativeAnimation()
        {
            _negativeStoryboard?.Stop();
            NegativeTimeIndicator.Opacity = 0;
        }

        private void StopWarningAnimation()
        {
            _warningStoryboard?.Stop();
            WarningTimeIndicator.Opacity = 0;
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

        // DisplayWindow Only: Maximize/Restore
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaximizeButton.Content = "🗖";
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaximizeButton.Content = "⧉"; // Restore icon
            }
        }
    }
}