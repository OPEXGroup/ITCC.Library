// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// #define WRITE
// #define READ

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Interfaces;
using ITCC.Logging.Core.Loggers;
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

            SpawnLogThreads();

            Logger.LogEntry("TEST", LogLevel.Info, "Done");
            await Logger.FlushAllAsync();

            Console.ReadLine();
        }

        private static void SpawnLogThreads()
        {
            var threadCount = Environment.ProcessorCount;
            var threads = new List<Thread>();
            for (var i = 0; i < threadCount; ++i)
            {
                threads.Add(new Thread(ThreadFunc));
            }

            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());
        }

        private static void ThreadFunc()
        {
            for (var i = 0; i < 10; ++i)
            {
                for (var j = 0; j < 100 * 1000; ++j)
                {
                    Logger.LogEntry("TEST", LogLevel.Debug, "Message");
                }
                Logger.LogEntry("TEST", LogLevel.Info, $"{i * 10}% done");
            }
                
        }

        private static void InitLoggers()
        {
            Logger.Level = LogLevel.Trace;
            Logger.RegisterReceiver(new ColouredConsoleLogger(LogLevel.Info));
            _fileLogger = new BufferedFileLogger(FileName, LogLevel.Trace, true, 5000, 10000);
            Logger.RegisterReceiver(_fileLogger);
            Console.OutputEncoding = Encoding.UTF8;
        }

        private static ILogReceiver _fileLogger;
        private const string FileName = @"test.log";
    }
}
