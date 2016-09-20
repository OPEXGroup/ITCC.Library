using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCC.Logging.Core;
using ITCC.Logging.Windows.Loggers;

namespace ITCC.Logging.Testing
{
    internal class Program
    {
        private static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            InitLoggers();

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
            await Task.Delay(1);
            Console.ReadLine();
        }

        private static void InitLoggers()
        {
            Logger.Level = LogLevel.Info;
            Logger.RegisterReceiver(new ColouredConsoleLogger(LogLevel.Debug));
            Logger.RegisterReceiver(new BufferedFileLogger(@"C:\Test\test.log", LogLevel.Trace, true, 1000));
            Console.OutputEncoding = Encoding.UTF8;
        }

        
    }
}
