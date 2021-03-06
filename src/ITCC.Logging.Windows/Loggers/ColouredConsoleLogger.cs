﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using ITCC.Logging.Core;

namespace ITCC.Logging.Windows.Loggers
{
    public class ColouredConsoleLogger : ConsoleLogger
    {
        #region ILogReceiver
        public override void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;
            lock (_lockObject)
            {
                switch (args.Level)
                {
                    case LogLevel.Critical:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case LogLevel.Trace:
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;
                    case LogLevel.None:
                    case LogLevel.Debug:
                        // Default color
                        break;
                }
                Console.WriteLine(args);
                Console.ResetColor();
            }
        }
        #endregion

        #region public
        public ColouredConsoleLogger()
        { }

        public ColouredConsoleLogger(LogLevel level) : base(level) { }
        #endregion

        #region private
        private readonly object _lockObject = new object();
        #endregion
    }
}
