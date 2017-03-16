// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Text;
using System.Threading.Tasks;
using ITCC.HTTP.API.Documentation.Core;
using ITCC.HTTP.API.Samples.Testing.Utils;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Loggers;
using ITCC.Logging.Windows.Loggers;

namespace ITCC.HTTP.Api.Documentation.Testing
{
    internal static class Program
    {
        private static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            InitLoggers();
            LogMessage(LogLevel.Info, "Started");
            var generator = new DocGenerator();
            var outputStream = Console.OpenStandardOutput();

            try
            {
                await generator.GenerateApiDocumentationAsync(typeof(TestMarkerType), outputStream);
            }
            catch (Exception exception)
            {
                Logger.LogException("DOC TEST", LogLevel.Warning, exception);
            }
            
            LogMessage(LogLevel.Info, "Done");
            Console.ReadLine();
        }

        private static void InitLoggers()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Logger.Level = LogLevel.Trace;
            Logger.RegisterReceiver(new DebugLogger());
            Logger.RegisterReceiver(new ColouredConsoleLogger());
        }

        private static void LogMessage(LogLevel level, string message)
            => Logger.LogEntry("DOC TEST", level, message);
    }
}
