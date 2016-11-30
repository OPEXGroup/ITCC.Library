using System;
using System.Threading;
using System.Threading.Tasks;
using ITCC.HTTP.Client.Utils;
using ITCC.Logging.Core;
using ITCC.Logging.Windows.Loggers;
using Newtonsoft.Json;

namespace ITCC.HTTP.Testing
{
    public class TestClass
    {
        public string First { get; set; }
        public string Second { get; set; }
    }

    internal static class Program
    {
        private static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            Thread.CurrentThread.Name = "MAIN";
            if (!InitializeLoggers())
                return;
            Logger.LogEntry("MAIN", LogLevel.Info, "Started");

            var testObject = new TestClass
            {
                First = "First",
                Second = "Second"
            };
            var serialized = JsonConvert.SerializeObject(testObject);
            var dummy = JsonConvert.DeserializeObject<None>(serialized);
            
            Logger.LogEntry("MAIN", LogLevel.Info, "Finished");
            Console.ReadLine();
        }

        private static bool InitializeLoggers()
        {
            Logger.Level = LogLevel.Trace;
            Logger.RegisterReceiver(new ColouredConsoleLogger(), true);

            return true;
        }
    }
}
