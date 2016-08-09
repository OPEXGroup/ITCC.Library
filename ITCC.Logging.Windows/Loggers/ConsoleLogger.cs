using System;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Interfaces;

namespace ITCC.Logging.Loggers
{
    /// <summary>
    ///     Simple logger for console
    /// </summary>
    public class ConsoleLogger : ILogReceiver
    {
        #region ILogReceiver
        public LogLevel Level { get; set; }

        public virtual void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;

            Console.WriteLine(args);
        }
        #endregion

        #region public
        public ConsoleLogger()
        {
            Level = Logger.Level;
        }

        public ConsoleLogger(LogLevel level)
        {
            Level = level;
        }
        #endregion
    }
}
