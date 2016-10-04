using System;

namespace ITCC.Logging.Core
{
    public class LogEntryEventArgs : EventArgs
    {
        private string _representation;

        /// <summary>
        ///     Format string for message
        /// </summary>
        public readonly string Message;

        /// <summary>
        ///     Current message level
        /// </summary>
        public readonly LogLevel Level;

        /// <summary>
        ///     Scope of event occurance
        /// </summary>
        public readonly object Scope;

        /// <summary>
        ///     Event thread
        /// </summary>
        public readonly int ThreadId = Environment.CurrentManagedThreadId;

        /// <summary>
        ///     Event time
        /// </summary>
        public DateTime Time = DateTime.Now;

        /// <summary>
        ///     Occured exception
        /// </summary>
        public Exception Exception;

        public LogEntryEventArgs()
        {
            Level = LogLevel.Info;
        }

        public LogEntryEventArgs(object scope, LogLevel level, string message, Exception exception = null)
        {
            Scope = scope;
            Level = level;
            Message = message;
            Exception = exception;
        }

        public static LogEntryEventArgs CreateFromRawData(DateTime time, LogLevel level, int threadId, object scope,
            string message) => new LogEntryEventArgs(time, level, threadId, scope, message);

        public string Representation => _representation ?? ToString();

        public override string ToString()
        {
            try
            {
                var result = $"[{Time.ToString("dd.MM.yyyy HH:mm:ss.fff")}] [{LogLevelRepresentation(Level), 5}] [THR {ThreadId,-6}] [{Scope, 12}] {Message}";
                _representation = result;
                return result;
            }
            catch (Exception)
            {
                _representation = "ERROR CREATING LOG ENTRY";
                return _representation;
            }
        }

        private LogEntryEventArgs(DateTime time, LogLevel level, int threadId, object scope,
            string message)
        {
            ThreadId = threadId;
            Time = time;
            Level = level;
            Scope = scope;
            Message = message;
        }

        private string LogLevelRepresentation(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Critical:
                    return "CRIT";
                case LogLevel.Error:
                    return "ERROR";
                case LogLevel.Warning:
                    return "WARN";
                case LogLevel.Info:
                    return "INFO";
                case LogLevel.Debug:
                    return "DEBUG";
                case LogLevel.Trace:
                    return "TRACE";
                default:
                    return "???";
            }
        }
    }
}