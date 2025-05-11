using System;
using System.Diagnostics;

namespace TimerClockApp.Services
{
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
    /// Implements logging functionality using Windows Event Log.
    /// </summary>
    public class EventLogger : ILogger, IDisposable
    {
        private const string LogSource = "TimerClock";
        private const string LogName = "Application";
        private readonly EventLog _eventLog;
        private bool _disposed;

        public EventLogger()
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
                _eventLog = null!;
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
}
