using System;

namespace ITCC.Logging.Loggers
{
    public class ColouredConsoleLogger : ConsoleLogger
    {
        public ColouredConsoleLogger()
        { }

        public ColouredConsoleLogger(LogLevel level) : base(level) { }

        public override void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;
            lock (LockObject)
            {
                switch (args.Level)
                {
                    case LogLevel.Critical:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case LogLevel.Trace:
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;
                }
                Console.WriteLine(args);
                Console.ResetColor();
            }
        }
    }
}
