// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ITCC.HTTP.API.Extensions;
using ITCC.HTTP.API.Testing.Enums;
using ITCC.HTTP.API.Testing.Views;
using ITCC.HTTP.API.Utils;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Loggers;
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

            var apiErrorView = ApiErrorViewFactory.ForeignKeyError(typeof(List<string>), "jljfslkdf");
            using (var stringWriter = new StringWriter())
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    NewLineOnAttributes = true
                };
                using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    var xmlSerializer = new XmlSerializer(apiErrorView.GetType());
                    xmlSerializer.Serialize(xmlWriter, apiErrorView);
                }
                LogMessage(LogLevel.Info, stringWriter.ToString());

                LogMessage(LogLevel.Info, EnumInfoProvider.GetEnumElementByName<SimpleEnum>("1")?.ToString());
                var str = EnumInfoProvider.GetElementName(FlagsEnum.Second | FlagsEnum.Third);
                LogMessage(LogLevel.Info, EnumInfoProvider.GetEnumElementByName<FlagsEnum>(str).ToString());
            }
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
