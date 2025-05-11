using System;
using System.ComponentModel;
using System.Windows;
using System.Timers;

namespace TimerClockApp
{
    public class TimerService : INotifyPropertyChanged, IDisposable
    {
        private readonly System.Timers.Timer _timer;
        private TimeSpan _timeLeft;
        private bool _isRunning;
        private bool _isPaused;
        private bool _isNegative;
        private bool _isWarning;
        private bool _disposed;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            private set
            {
                _isPaused = value;
                OnPropertyChanged(nameof(IsPaused));
            }
        }

        public bool IsNegative
        {
            get => _isNegative;
            private set
            {
                if (_isNegative != value)
                {
                    _isNegative = value;
                    OnPropertyChanged(nameof(IsNegative));
                }
            }
        }

        public bool IsWarning
        {
            get => _isWarning;
            private set
            {
                if (_isWarning != value)
                {
                    _isWarning = value;
                    OnPropertyChanged(nameof(IsWarning));
                }
            }
        }

        public TimeSpan TimeLeft
        {
            get => _timeLeft;
            private set
            {
                _timeLeft = value;
                IsNegative = _timeLeft < TimeSpan.Zero;

                // Use warning time from settings, defaulting to 30 if not set
                int warningTime = Properties.Settings.Default.WarningTime > 0
                    ? Properties.Settings.Default.WarningTime
                    : 30;

                bool shouldWarn = !IsNegative && _timeLeft.TotalSeconds <= warningTime;
                System.Diagnostics.Debug.WriteLine($"QQQ TimeLeft: {_timeLeft.TotalSeconds:F1}s, WarningTime: {warningTime}s, ShouldWarn: {shouldWarn}");

                IsWarning = shouldWarn;
                OnPropertyChanged(nameof(TimeLeft));
            }
        }

        public TimerService()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            TimeLeft = TimeSpan.Zero;
        }

        public void Start(int minutes)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TimeLeft = TimeSpan.FromMinutes(minutes);
                _timer.Start();
                IsRunning = true;
                IsPaused = false;
            });
        }

        public void Pause()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (IsRunning)
                {
                    _timer.Stop();
                    IsPaused = true;
                }
            });
        }

        public void Resume()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (IsRunning && IsPaused)
                {
                    _timer.Start();
                    IsPaused = false;
                }
            });
        }

        public void Stop()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _timer.Stop();
                IsRunning = false;
                IsPaused = false;
            });
        }

        public void Reset(int minutes)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Stop();
                TimeLeft = TimeSpan.FromMinutes(minutes);
            });
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!IsPaused)
                {
                    TimeLeft = TimeLeft.Subtract(TimeSpan.FromSeconds(1));
                }
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timer.Elapsed -= Timer_Elapsed;
                    _timer.Stop();
                    _timer.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class ClockService : INotifyPropertyChanged, IDisposable
    {
        private readonly System.Timers.Timer _timer;
        private DateTime _currentTime;
        private bool _disposed;

        public event PropertyChangedEventHandler? PropertyChanged;

        public DateTime CurrentTime
        {
            get => _currentTime;
            private set
            {
                _currentTime = value;
                OnPropertyChanged(nameof(CurrentTime));
            }
        }

        public ClockService()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            CurrentTime = DateTime.Now;
            _timer.Start();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentTime = DateTime.Now;
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timer.Elapsed -= Timer_Elapsed;
                    _timer.Stop();
                    _timer.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class DisplayManager : INotifyPropertyChanged, IDisposable
    {
        private readonly System.Timers.Timer _autoModeTimer;
        private string _displayMode = "Timer";
        private bool _showingClock;
        private bool _disposed;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? DisplayTypeChanged;

        public bool ShowingClock
        {
            get => _showingClock;
            private set
            {
                if (_showingClock != value)
                {
                    _showingClock = value;
                    OnPropertyChanged(nameof(ShowingClock));
                    DisplayTypeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string DisplayMode
        {
            get => _displayMode;
            set
            {
                if (_displayMode != value)
                {
                    _displayMode = value;
                    OnPropertyChanged(nameof(DisplayMode));
                    HandleModeChange();
                }
            }
        }

        public DisplayManager()
        {
            _autoModeTimer = new System.Timers.Timer();
            _autoModeTimer.Elapsed += AutoModeTimer_Elapsed;
        }

        public void HandleModeChange()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (DisplayMode == "Auto")
                {
                    _autoModeTimer.Interval = Properties.Settings.Default.AutoSwitchInterval * 1000;
                    _autoModeTimer.Start();
                }
                else
                {
                    _autoModeTimer.Stop();
                    ShowingClock = DisplayMode == "Clock";
                }
            });
        }

        private void AutoModeTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (DisplayMode == "Auto")
                {
                    ShowingClock = !ShowingClock;
                }
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _autoModeTimer.Elapsed -= AutoModeTimer_Elapsed;
                    _autoModeTimer.Stop();
                    _autoModeTimer.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}