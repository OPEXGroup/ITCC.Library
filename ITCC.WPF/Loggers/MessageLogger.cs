// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using System.Windows;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Interfaces;
using ITCC.WPF.Utils;

namespace ITCC.WPF.Loggers
{
    public class MessageLogger : ILogReceiver
    {
        #region ILogReceiver
        public LogLevel Level { get; set; }

        public void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;
            MessageBox.Show(args.Message, EnumHelper.LogLevelName(args.Level), MessageBoxButton.OK, MessageBoxImages[args.Level]);
        }
        #endregion

        #region public
        public MessageLogger()
        {
            Level = Logger.Level;
        }

        public MessageLogger(LogLevel level)
        {
            Level = level;
        }
        #endregion

        #region private
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
        #endregion
    }
}