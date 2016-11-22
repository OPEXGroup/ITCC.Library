using System;
using ITCC.Logging.Core;
using ITCC.Logging.Windows.Loggers;

namespace ITCC.HTTP.API.Testing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            InitLoggers();

            LogMessage(LogLevel.Info, "Started");

            try
            {
                PerformTest();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            LogMessage(LogLevel.Info, "Done");
        }

        private static void PerformTest()
        {
            
        }

        private static void InitLoggers()
        {
            Logger.Level = LogLevel.Trace;
            Logger.RegisterReceiver(new ColouredConsoleLogger());
        }

        private static void LogMessage(LogLevel level, string message)
            => Logger.LogEntry("TEST", level, message);

        private static void LogException(Exception exception)
            => Logger.LogException("TEST", LogLevel.Warning, exception);
    }
}
