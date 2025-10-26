using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace TimerClockApp
{
    public class ControlPanelViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly TimerService _timerService;
        private readonly ClockService _clockService;
        private readonly DisplayManager _displayManager;
        private readonly ILogger _logger;
        private readonly ISettings _settings;
        private int _timerMinutes;
        private string _timeDisplay = "00:00";
        private bool _disposed;

        private DisplayState _currentState = DisplayState.Normal;

        /// <summary>
        /// Combined visual state used by the UI to decide which animation to play.
        /// </summary>
        public DisplayState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    OnPropertyChanged(nameof(CurrentState));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string TimeDisplay
        {
            get => _timeDisplay;
            private set
            {
                if (_timeDisplay != value)
                {
                    _timeDisplay = value;
                    OnPropertyChanged(nameof(TimeDisplay));
                }
            }
        }

        public int TimerMinutes
        {
            get => _timerMinutes;
            set
            {
                if (value >= 1 && value <= 200 && _timerMinutes != value)
                {
                    _timerMinutes = value;
                    OnPropertyChanged(nameof(TimerMinutes));
                    _logger.LogDebug($"Timer minutes set to: {value}");
                }
            }
        }

        public bool ShowingClock => _displayManager.ShowingClock;
        public bool IsTimerRunning => _timerService.IsRunning;
        public bool IsPaused => _timerService.IsPaused;
        public bool IsNegative => _timerService.IsNegative;
        public bool IsWarning => _timerService.IsWarning;

        public bool IsPlayPauseEnabled => IsTimerRunning;
        public string PlayPauseButtonText => IsPaused ? "Play" : "Pause";

        public bool TimerModeEnabled => true;
        public bool ClockModeEnabled => true;
        public bool AutoModeEnabled => IsTimerRunning;

        public Brush TimerModeBackground => _displayManager.DisplayMode == "Timer" ? new SolidColorBrush(Colors.LightBlue) : new SolidColorBrush(Colors.Transparent);
        public Brush ClockModeBackground => _displayManager.DisplayMode == "Clock" ? new SolidColorBrush(Colors.LightBlue) : new SolidColorBrush(Colors.Transparent);
        public Brush AutoModeBackground => _displayManager.DisplayMode == "Auto" ? new SolidColorBrush(Colors.LightBlue) : new SolidColorBrush(Colors.Transparent);

        public ICommand StartCommand { get; }
        public ICommand PlayPauseCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand TimerModeCommand { get; }
        public ICommand ClockModeCommand { get; }
        public ICommand AutoModeCommand { get; }

        public ControlPanelViewModel(ISettings? settings = null, ILogger? logger = null)
        {
            _logger = logger ?? new Logger();
            _settings = settings ?? new DefaultSettings();
            _timerMinutes = _settings.LastTimerMinutes;

            _timerService = new TimerService(_settings, _logger);
            _clockService = new ClockService(_logger);
            _displayManager = new DisplayManager(_settings, _logger);

            StartCommand = new RelayCommand(StartTimer);
            PlayPauseCommand = new RelayCommand(PlayPauseTimer);
            ResetCommand = new RelayCommand(ResetTimer);
            TimerModeCommand = new RelayCommand(() => SetMode("Timer"));
            ClockModeCommand = new RelayCommand(() => SetMode("Clock"));
            AutoModeCommand = new RelayCommand(() => SetMode("Auto"));

            _timerService.PropertyChanged += TimerService_PropertyChanged;
            _clockService.PropertyChanged += ClockService_PropertyChanged;
            _displayManager.DisplayTypeChanged += DisplayManager_DisplayTypeChanged;

            SetMode("Clock");
            UpdateTimeDisplay();

            _logger.LogInformation("ControlPanelViewModel initialized");
        }

        private void UpdateDisplayState()
        {
            if (!_displayManager.ShowingClock && IsTimerRunning && !IsPaused)
            {
                if (IsNegative)
                {
                    CurrentState = DisplayState.Negative;
                }
                else if (IsWarning)
                {
                    CurrentState = DisplayState.Warning;
                }
                else
                {
                    CurrentState = DisplayState.Normal;
                }
            }
            else
            {
                CurrentState = DisplayState.Normal;
            }
        }

        private void StartTimer()
        {
            if (!IsTimerRunning)
            {
                _settings.LastTimerMinutes = TimerMinutes;
                _timerService.Start(TimerMinutes);
                UpdateTimeDisplay();
                _logger.LogInformation($"Timer started with {TimerMinutes} minutes");
            }
        }

        private void PlayPauseTimer()
        {
            if (IsPaused)
            {
                _timerService.Resume();
            }
            else
            {
                _timerService.Pause();
            }
            UpdateTimeDisplay();
        }

        private void ResetTimer()
        {
            if (_displayManager.DisplayMode == "Auto")
            {
                SetMode("Clock");
            }
            _timerService.Reset(TimerMinutes);
            UpdateTimeDisplay();
            _logger.LogInformation("Timer reset");
        }

        private void UpdateTimeDisplay()
        {
            if (_displayManager.ShowingClock)
            {
                TimeDisplay = _clockService.CurrentTime.ToString("HH':'mm':'ss");
            }
            else
            {
                var timeLeft = _timerService.TimeLeft;
                if (timeLeft < TimeSpan.Zero)
                {
                    TimeDisplay = "-" + $"{(int)Math.Abs(timeLeft.TotalMinutes)}:{Math.Abs(timeLeft.Seconds):D2}";
                }
                else
                {
                    TimeDisplay = $"{(int)timeLeft.TotalMinutes}:{timeLeft.Seconds:D2}";
                }
            }
        }

        private void SetMode(string mode)
        {
            _displayManager.DisplayMode = mode;
            OnPropertyChanged(nameof(TimerModeBackground));
            OnPropertyChanged(nameof(ClockModeBackground));
            OnPropertyChanged(nameof(AutoModeBackground));
            UpdateTimeDisplay();
        }

        private void TimerService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TimerService.TimeLeft):
                case nameof(TimerService.IsNegative):
                    UpdateTimeDisplay();
                    break;
                case nameof(TimerService.IsRunning):
                    OnPropertyChanged(nameof(IsTimerRunning));
                    UpdateDisplayState();
                    OnPropertyChanged(nameof(IsPlayPauseEnabled));
                    OnPropertyChanged(nameof(AutoModeEnabled));
                    break;
                case nameof(TimerService.IsPaused):
                    OnPropertyChanged(nameof(IsPaused));
                    UpdateDisplayState();
                    OnPropertyChanged(nameof(PlayPauseButtonText));
                    break;
                case nameof(TimerService.IsWarning):
                    OnPropertyChanged(nameof(IsWarning));
                    UpdateDisplayState();
                    break;
            }
        }

        private void ClockService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ClockService.CurrentTime) && ShowingClock)
            {
                UpdateTimeDisplay();
            }
        }

        private void DisplayManager_DisplayTypeChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ShowingClock));
            UpdateTimeDisplay();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timerService.PropertyChanged -= TimerService_PropertyChanged;
                    _clockService.PropertyChanged -= ClockService_PropertyChanged;
                    _displayManager.DisplayTypeChanged -= DisplayManager_DisplayTypeChanged;

                    _timerService.Dispose();
                    _clockService.Dispose();
                    _displayManager.Dispose();

                    if (_logger is IDisposable disposableLogger)
                    {
                        disposableLogger.Dispose();
                    }
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