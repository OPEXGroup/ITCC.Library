// #define WRITE
#define READ

using System;
using System.Text;
using System.Threading.Tasks;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Interfaces;
using ITCC.Logging.Reader.Core;
using ITCC.Logging.Windows.Loggers;

namespace ITCC.Logging.Testing
{
    internal class Program
    {
        private static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            InitLoggers();

#if WRITE
            const int outterCycleSize = 100;
            const int innerCycleSize = 1000;

            var random = new Random();

            for (var i = 0; i < outterCycleSize; ++i)
            {
                var willBeDone = (i + 1) * (innerCycleSize + 1);
                for (var j = 0; j < innerCycleSize; ++j)
                {
                    Logger.LogEntry("TEST", LogLevel.Trace, $"Message with random {random.Next()}");
                }
                Logger.LogEntry("TEST", LogLevel.Debug, $"Done {willBeDone}");
            }
            Logger.LogEntry("MAIN", LogLevel.Info, "Done");
            Logger.FlushAll();
            _fileLogger.Level = LogLevel.None;
#endif

#if READ
            try
            {
                var reader = new LogReader(FileName);
                foreach (var entry in reader.ReadAsEnumerable())
                {
                    if (entry == null)
                    {
                        Logger.LogEntry("MAIN", LogLevel.Debug, "Failed to parse");
                        continue;
                    }

                    PrintEntry(entry);
                    //Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("MAIN", LogLevel.Warning, ex);
                throw;
            }
#endif

            await Task.Delay(1);
            Console.ReadLine();
        }

        private static void PrintEntry(LogEntryEventArgs args)
        {
            Action<string> printAction = Console.WriteLine;

            printAction($"DATE:           {args.Time}");
            printAction($"LEVEL:          {args.Level}");
            printAction($"THREAD:         {args.ThreadId}");
            printAction($"SCOPE:          {args.Scope}");
            printAction($"MESSAGE:        {args.Message}");
        }

        private static void InitLoggers()
        {
            Logger.Level = LogLevel.Trace;
            Logger.RegisterReceiver(new ColouredConsoleLogger(LogLevel.Trace));
#if WRITE
            _fileLogger = new BufferedFileLogger(FileName, LogLevel.Trace, true, 1000);
#endif
            Logger.RegisterReceiver(_fileLogger);
            Console.OutputEncoding = Encoding.UTF8;
        }

        private static ILogReceiver _fileLogger;
        private const string FileName = @"C:\Test\test.log";
    }
}
