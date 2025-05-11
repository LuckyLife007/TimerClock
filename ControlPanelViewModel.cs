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
        private int _timerMinutes = Properties.Settings.Default.LastTimerMinutes;
        private string _timeDisplay = "00:00";
        private bool _disposed;

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

        public ControlPanelViewModel()
        {
            _timerService = new TimerService();
            _clockService = new ClockService();
            _displayManager = new DisplayManager();

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
        }

        private void TimerService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!_displayManager.ShowingClock)
            {
                UpdateTimeDisplay();
            }

            switch (e.PropertyName)
            {
                case nameof(TimerService.IsRunning):
                    OnPropertyChanged(nameof(IsTimerRunning));
                    OnPropertyChanged(nameof(IsPlayPauseEnabled));
                    OnPropertyChanged(nameof(AutoModeEnabled));
                    break;
                case nameof(TimerService.IsPaused):
                    OnPropertyChanged(nameof(IsPaused));
                    OnPropertyChanged(nameof(PlayPauseButtonText));
                    break;
                case nameof(TimerService.IsNegative):
                    OnPropertyChanged(nameof(IsNegative));
                    break;
                case nameof(TimerService.IsWarning):
                    OnPropertyChanged(nameof(IsWarning));
                    break;
            }
        }

        private void ClockService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_displayManager.ShowingClock)
            {
                UpdateTimeDisplay();
            }
        }

        private void DisplayManager_DisplayTypeChanged(object? sender, EventArgs e)
        {
            UpdateTimeDisplay();
            OnPropertyChanged(nameof(ShowingClock));
            UpdateModeBackgrounds();
        }

        private void UpdateModeBackgrounds()
        {
            OnPropertyChanged(nameof(TimerModeBackground));
            OnPropertyChanged(nameof(ClockModeBackground));
            OnPropertyChanged(nameof(AutoModeBackground));
        }

        private void SetMode(string mode)
        {
            _displayManager.DisplayMode = mode;
            UpdateModeBackgrounds();
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


        private void StartTimer()
        {
            if (!IsTimerRunning)
            {
                _timerService.Start(TimerMinutes);
                Properties.Settings.Default.LastTimerMinutes = TimerMinutes;
                Properties.Settings.Default.Save();
                UpdateTimeDisplay();
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
                    _timerService.Dispose();
                    _clockService.Dispose();
                    _displayManager.Dispose();
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