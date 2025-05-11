using System;
using System.ComponentModel;
using System.Windows;
using System.Timers;
using System.Configuration;
using System.Diagnostics;

namespace TimerClockApp
{
    /// <summary>
    /// Interface for accessing application settings
    /// </summary>
    public interface ISettings
    {
        int WarningTime { get; }
        int AutoSwitchInterval { get; }
        int LastTimerMinutes { get; set; }
        double ControlOpacity { get; }
        double DisplayOpacity { get; }
        bool ControlTopmost { get; }
        bool DisplayTopmost { get; }
    }

    /// <summary>
    /// Default settings implementation that uses application settings
    /// </summary>
    public class DefaultSettings : ISettings
    {
        public int WarningTime => Properties.Settings.Default.WarningTime;
        public int AutoSwitchInterval => Properties.Settings.Default.AutoSwitchInterval;
        public int LastTimerMinutes
        {
            get => Properties.Settings.Default.LastTimerMinutes;
            set
            {
                Properties.Settings.Default.LastTimerMinutes = value;
                Properties.Settings.Default.Save();
            }
        }
        public double ControlOpacity => Properties.Settings.Default.ControlOpacity;
        public double DisplayOpacity => Properties.Settings.Default.DisplayOpacity;
        public bool ControlTopmost => Properties.Settings.Default.ControlTopmost;
        public bool DisplayTopmost => Properties.Settings.Default.DisplayTopmost;
    }

    /// <summary>
    /// Provides logging functionality for the application.
    /// </summary>
    public interface ILogger
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception? exception = null);
        void LogDebug(string message);
    }

    /// <summary>
    /// Implements logging functionality using Windows Event Log and Debug output.
    /// </summary>
    public class Logger : ILogger, IDisposable
    {
        private const string LogSource = "TimerClock";
        private const string LogName = "Application";
        private readonly EventLog? _eventLog;
        private bool _disposed;

        public Logger()
        {
            try
            {
                if (!EventLog.SourceExists(LogSource))
                {
                    EventLog.CreateEventSource(LogSource, LogName);
                }

                _eventLog = new EventLog(LogName)
                {
                    Source = LogSource
                };
            }
            catch (Exception ex)
            {
                // Fall back to debug output if event log creation fails
                Debug.WriteLine($"Failed to create event log: {ex.Message}");
                _eventLog = null;
            }
        }

        public void LogInformation(string message)
        {
            if (_eventLog != null)
            {
                _eventLog.WriteEntry(message, EventLogEntryType.Information);
            }
            Debug.WriteLine($"[INFO] {message}");
        }

        public void LogWarning(string message)
        {
            if (_eventLog != null)
            {
                _eventLog.WriteEntry(message, EventLogEntryType.Warning);
            }
            Debug.WriteLine($"[WARN] {message}");
        }

        public void LogError(string message, Exception? exception = null)
        {
            var logMessage = exception != null
                ? $"{message}\nException: {exception.Message}\nStack Trace: {exception.StackTrace}"
                : message;

            if (_eventLog != null)
            {
                _eventLog.WriteEntry(logMessage, EventLogEntryType.Error);
            }
            Debug.WriteLine($"[ERROR] {logMessage}");
        }

        public void LogDebug(string message)
        {
#if DEBUG
            Debug.WriteLine($"[DEBUG] {message}");
#endif
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _eventLog != null)
                {
                    _eventLog.Dispose();
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

    /// <summary>
    /// Helper class for handling dispatcher operations in both test and runtime environments
    /// </summary>
    internal static class DispatcherHelper
    {
        public static void InvokeOnDispatcher(Action action)
        {
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }

    /// <summary>
    /// Manages the countdown timer functionality, including state management and time tracking.
    /// This service handles timer operations, warning states, and property change notifications.
    /// </summary>
    public class TimerService : INotifyPropertyChanged, IDisposable
    {
        private readonly System.Timers.Timer _timer;
        private readonly ISettings _settings;
        private readonly ILogger _logger;
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
                    if (_isWarning)
                    {
                        _logger.LogWarning($"Timer warning threshold reached: {TimeLeft.TotalSeconds:F1} seconds remaining");
                    }
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

                // Use warning time from settings
                int warningTime = _settings.WarningTime;
                bool shouldWarn = !IsNegative && _timeLeft.TotalSeconds <= warningTime;
                IsWarning = shouldWarn;

                OnPropertyChanged(nameof(TimeLeft));
                _logger.LogDebug($"Timer updated: {TimeLeft.TotalSeconds:F1} seconds remaining");
            }
        }

        public TimerService(ISettings? settings = null, ILogger? logger = null)
        {
            _settings = settings ?? new DefaultSettings();
            _logger = logger ?? new Logger();
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            TimeLeft = TimeSpan.Zero;
            _logger.LogInformation("TimerService initialized");
        }

        public void Start(int minutes)
        {
            DispatcherHelper.InvokeOnDispatcher(() =>
            {
                TimeLeft = TimeSpan.FromMinutes(minutes);
                _timer.Start();
                IsRunning = true;
                IsPaused = false;
                _logger.LogInformation($"Timer started: {minutes} minutes");
            });
        }

        public void Pause()
        {
            DispatcherHelper.InvokeOnDispatcher(() =>
            {
                if (IsRunning)
                {
                    _timer.Stop();
                    IsPaused = true;
                    _logger.LogInformation("Timer paused");
                }
            });
        }

        public void Resume()
        {
            DispatcherHelper.InvokeOnDispatcher(() =>
            {
                if (IsRunning && IsPaused)
                {
                    _timer.Start();
                    IsPaused = false;
                    _logger.LogInformation("Timer resumed");
                }
            });
        }

        public void Stop()
        {
            DispatcherHelper.InvokeOnDispatcher(() =>
            {
                _timer.Stop();
                IsRunning = false;
                IsPaused = false;
                _logger.LogInformation("Timer stopped");
            });
        }

        public void Reset(int minutes)
        {
            DispatcherHelper.InvokeOnDispatcher(() =>
            {
                Stop();
                TimeLeft = TimeSpan.FromMinutes(minutes);
                _logger.LogInformation($"Timer reset to {minutes} minutes");
            });
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            DispatcherHelper.InvokeOnDispatcher(() =>
            {
                if (!IsPaused)
                {
                    TimeLeft = TimeLeft.Subtract(TimeSpan.FromSeconds(1));
                    if (IsNegative)
                    {
                        _logger.LogWarning($"Timer expired: {Math.Abs(TimeLeft.TotalSeconds):F1} seconds overtime");
                    }
                }
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            DispatcherHelper.InvokeOnDispatcher(() =>
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

    public class ClockService : INotifyPropertyChanged, IDisposable
    {
        private readonly System.Timers.Timer _timer;
        private readonly ILogger _logger;
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

        public ClockService(ILogger? logger = null)
        {
            _logger = logger ?? new Logger();
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            CurrentTime = DateTime.Now;
            _timer.Start();
            _logger.LogInformation("ClockService initialized");
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            DispatcherHelper.InvokeOnDispatcher(() =>
            {
                CurrentTime = DateTime.Now;
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            DispatcherHelper.InvokeOnDispatcher(() =>
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

    public class DisplayManager : INotifyPropertyChanged, IDisposable
    {
        private readonly System.Timers.Timer _autoModeTimer;
        private readonly ISettings _settings;
        private readonly ILogger _logger;
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
                    _logger.LogDebug($"Display changed to: {(_showingClock ? "Clock" : "Timer")}");
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
                    _logger.LogInformation($"Display mode changed to: {value}");
                }
            }
        }

        public DisplayManager(ISettings? settings = null, ILogger? logger = null)
        {
            _settings = settings ?? new DefaultSettings();
            _logger = logger ?? new Logger();
            _autoModeTimer = new System.Timers.Timer();
            _autoModeTimer.Elapsed += AutoModeTimer_Elapsed;
            _logger.LogInformation("DisplayManager initialized");
        }

        public void HandleModeChange()
        {
            DispatcherHelper.InvokeOnDispatcher(() =>
            {
                if (DisplayMode == "Auto")
                {
                    _autoModeTimer.Interval = _settings.AutoSwitchInterval * 1000;
                    _autoModeTimer.Start();
                    _logger.LogInformation($"Auto mode started with interval: {_settings.AutoSwitchInterval} seconds");
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
            DispatcherHelper.InvokeOnDispatcher(() =>
            {
                if (DisplayMode == "Auto")
                {
                    ShowingClock = !ShowingClock;
                }
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            DispatcherHelper.InvokeOnDispatcher(() =>
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