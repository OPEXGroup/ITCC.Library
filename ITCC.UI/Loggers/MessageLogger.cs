using System.Collections.Generic;
using System.Windows;
using ITCC.Logging;
using ITCC.Logging.Interfaces;

namespace ITCC.UI.Loggers
{
    public class MessageLogger : ILogReceiver
    {
        public MessageLogger()
        {
            Level = Logger.Level;
        }

        public MessageLogger(LogLevel level)
        {
            Level = level;
        }

        public LogLevel Level { get; set; }

        public void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;
            MessageBox.Show(args.Message, args.Level.ToString(), MessageBoxButton.OK, MessageBoxImages[args.Level]);
        }

        private static readonly Dictionary<LogLevel, MessageBoxImage> MessageBoxImages = new Dictionary
            <LogLevel, MessageBoxImage>
        {
            {LogLevel.Trace, MessageBoxImage.None},
            {LogLevel.Debug, MessageBoxImage.None},
            {LogLevel.Info, MessageBoxImage.Information},
            {LogLevel.Warning, MessageBoxImage.Warning},
            {LogLevel.Error, MessageBoxImage.Error},
            {LogLevel.Critical, MessageBoxImage.Stop}
        };
    }
}