using System;
using System.Collections.Generic;
using System.Text;
using ITCC.HTTP.API.Extensions;
using ITCC.HTTP.API.Testing.Views;
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
            var viewTree = new NodeView
            {
                Name = "Root",
                Children = new List<NodeView>
                {
                    new NodeView
                    {
                        Children = new List<NodeView>
                        {
                            new NodeView()
                        }
                    }
                },
                IsLeaf = true
            };

            var checkResult = viewTree.CheckAsView();
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine();
            logBuilder.AppendLine($"IsCorrect: {checkResult.IsCorrect}");
            logBuilder.AppendLine(Separator);
            logBuilder.AppendLine("Message:");
            logBuilder.AppendLine(checkResult.ErrorDescription);

            var level = checkResult.IsCorrect ? LogLevel.Info : LogLevel.Warning;
            LogMessage(level, logBuilder.ToString());
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

        private const string Separator = "------------------------------------------";
    }
}
