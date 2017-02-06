// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// #define WRITE
// #define READ

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
        private const int ReportsPerThread = 10;
        private const int MessagesPerReport = 100 * 1000;
        private static int _threadCount;

        private static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            InitLoggers();
            _threadCount = Environment.ProcessorCount * 2;

            var sw = Stopwatch.StartNew();

            SpawnLogThreads();
            sw.Stop();

            var messagesCount = _threadCount * ReportsPerThread * MessagesPerReport;
            var avgMessageTime = (double) sw.ElapsedMilliseconds / messagesCount;
            Logger.LogEntry("TEST", LogLevel.Info, $"Done {messagesCount} in {sw.ElapsedMilliseconds}ms (avg {avgMessageTime})");
            await Logger.FlushAllAsync();

            Console.ReadLine();
        }

        private static void SpawnLogThreads()
        {
            var threads = new List<Thread>();
            for (var i = 0; i < _threadCount; ++i)
            {
                threads.Add(new Thread(ThreadFunc));
            }

            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());
        }

        private static void ThreadFunc()
        {
            for (var i = 0; i < ReportsPerThread; ++i)
            {
                for (var j = 0; j < 1 * MessagesPerReport; ++j)
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
