// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Text;
using System.Threading.Tasks;
using ITCC.HTTP.Api.Documentation.Testing.Utils;
using ITCC.HTTP.API.Documentation.Core;
using ITCC.Logging.Core;
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
            await generator.GenerateApiDocumentationAsync(typeof(TestMarkerType), outputStream);
            LogMessage(LogLevel.Info, "Done");
        }

        private static void InitLoggers()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Logger.Level = LogLevel.Trace;
            Logger.RegisterReceiver(new ColouredConsoleLogger());
        }

        private static void LogMessage(LogLevel level, string message)
            => Logger.LogEntry("DOC TEST", level, message);
    }
}
