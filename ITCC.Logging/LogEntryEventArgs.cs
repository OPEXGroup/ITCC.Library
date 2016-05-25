using System;
using System.Threading;

namespace ITCC.Logging
{
    public class LogEntryEventArgs : EventArgs
    {
        private string _representation;

        /// <summary>
        ///     Format string for message
        /// </summary>
        public readonly string Format;

        /// <summary>
        ///     Objects used to create final string
        /// </summary>
        public readonly object[] FormatParams;

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
        public readonly int ThreadId = Thread.CurrentThread.ManagedThreadId;

        /// <summary>
        ///     Event thread human-readable name
        /// </summary>
        public readonly string ThreadName = Thread.CurrentThread.Name;

        /// <summary>
        ///     Event time
        /// </summary>
        public DateTime Time = DateTime.Now;

        /// <summary>
        ///     Occured exception
        /// </summary>
        public Exception Exception;

        public LogEntryEventArgs(object scope, LogLevel level, string message, Exception exception = null)
        {
            Scope = scope;
            Level = level;
            Format = message;
            Exception = exception;
        }

        public string Representation => _representation ?? ToString();

        public string Message => FormatParams != null ? string.Format(Format, FormatParams) : Format;

        public override string ToString()
        {
            try
            {
                var threadInfo = ThreadName == null ? $"{ThreadId,-6}" : $"{ThreadName,-6}";
                var result = $"[{DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss.fff tt")}] [{LogLevelRepresentation(Level), 5}] [THR {threadInfo}] [{Scope, 12}] {Message}";
                _representation = result;
                return result;
            }
            catch (Exception)
            {
                _representation = "ERROR CREATING LOG ENTRY";
                return _representation;
            }
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