using System;
using System.Text;
using ITCC.HTTP.Server.Testing.Enums;
using ITCC.HTTP.Server.Testing.Utils;
using ITCC.Logging.Core;
using ITCC.Logging.Windows.Loggers;

namespace ITCC.HTTP.Server.Testing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (!Configuration.ReadAppConfig())
                return;

            InitializeLoggers();

            if (!ServerController.Start())
            {
                Logger.LogEntry("MAIN", LogLevel.Critical, "Failed to start server");
                return;
            }

            Logger.LogEntry("MAIN", LogLevel.Info,
                $"Files: {Configuration.Protocol.ToString().ToLowerInvariant()}://localhost/files/Test/filename");

            Console.ReadLine();
            ServerController.Stop();
        }

        private static void InitializeLoggers()
        {
            Logger.Level = Configuration.LogLevel;

            if (Configuration.LoggerMode.HasFlag(LoggerMode.Console))
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = Encoding.Unicode;
                Console.Title = @"ITCC Test Server 2016";
                var consoleLogger = new ColouredConsoleLogger();
                Logger.RegisterReceiver(consoleLogger, true);
            }

            if (Configuration.LoggerMode.HasFlag(LoggerMode.File))
            {
                var logFileName = Configuration.LogDirectory + @"\" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt";
                var fileLogger = new BufferedFileLogger(logFileName);
                Logger.RegisterReceiver(fileLogger, true);
            }

            Logger.LogEntry("MAIN", LogLevel.Info, $"Loggers initialized: {Configuration.LoggerMode}");
        }
    }
}
