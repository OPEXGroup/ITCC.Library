using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ITCC.Logging.Loggers
{
    public class SystemEventLogger : ILogReceiver, IDisposable
    {
        private readonly EventLog _eventLog;

        private LogLevel _level;

        private readonly Dictionary<LogLevel, EventLogEntryType> _typeConverter = new Dictionary<LogLevel, EventLogEntryType>
        {
            {LogLevel.Info, EventLogEntryType.Information },
            {LogLevel.Warning, EventLogEntryType.Warning },
            {LogLevel.Error, EventLogEntryType.Error },
            {LogLevel.Critical, EventLogEntryType.Error }
        };

        public LogLevel Level
        {
            get
            {
                return _level;
            }
            set
            {
                if (value > LogLevel.Info)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Cannot log message with severity lower than INFO");
                }
                _level = value;
            }
        }

        public void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > _level)
                return;
            
            _eventLog.WriteEntry(args.ToString(), _typeConverter[args.Level]);
        }

        public SystemEventLogger(string source, LogLevel level)
        {
            Level = level;
            _eventLog = new EventLog("Application");
            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, "Application");
                Logger.LogEntry("LOGINTERNAL", LogLevel.Info, $"Log source {source} registered");
            }
            _eventLog.Source = source;
        }

        public void Dispose()
        {
            _eventLog.Dispose();
        }
    }
}
