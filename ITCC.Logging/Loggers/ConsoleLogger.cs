using System;

namespace ITCC.Logging.Loggers
{
    /// <summary>
    ///     Simple logger for console
    /// </summary>
    public class ConsoleLogger : ILogReceiver
    {
        public LogLevel Level { get; set; }

        public ConsoleLogger()
        {
            Level = Logger.Level;
        }

        public ConsoleLogger(LogLevel level)
        {
            Level = level;
        }

        public virtual void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;

            Console.WriteLine(args);
        }
    }
}
