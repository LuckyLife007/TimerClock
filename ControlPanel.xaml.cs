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
        private Storyboard? _warningStoryboard;
        private Storyboard? _negativeStoryboard;

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

            // Setup the animations
            SetupAnimations();

            // Register for property changes to update animations
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
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
            Storyboard.SetTarget(warningAnimation, MiniWarningTimeIndicator);
            Storyboard.SetTargetProperty(warningAnimation, new PropertyPath(UIElement.OpacityProperty));

            var warningPause = new DoubleAnimation
            {
                From = 0,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(3)),
                BeginTime = TimeSpan.FromSeconds(1)  // Start after warningAnimation completes
            };
            Storyboard.SetTarget(warningPause, MiniWarningTimeIndicator);
            Storyboard.SetTargetProperty(warningPause, new PropertyPath(UIElement.OpacityProperty));

            _warningStoryboard.Children.Add(warningAnimation);
            _warningStoryboard.Children.Add(warningPause);
            _warningStoryboard.RepeatBehavior = RepeatBehavior.Forever;

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

            Storyboard.SetTarget(negativeAnimation, MiniNegativeTimeIndicator);
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
            MiniNegativeTimeIndicator.Opacity = 0;
        }

        private void StopWarningAnimation()
        {
            _warningStoryboard?.Stop();
            MiniWarningTimeIndicator.Opacity = 0;
        }

        private void StopAllAnimations()
        {
            StopNegativeAnimation();
            StopWarningAnimation();
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