#define DEBUG

// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Diagnostics;
using ITCC.Logging.Core.Interfaces;

namespace ITCC.Logging.Core.Loggers
{
    /// <summary>
    ///     Simple logger for debug logging (via System.Diagnostics.Debug)
    /// </summary>
    public class DebugLogger : ILogReceiver
    {
        #region ILogReceiver
        public LogLevel Level { get; set; }

        public virtual void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;

            Debug.WriteLine(args);
        }
        #endregion

        #region public
        public DebugLogger()
        {
            Level = Logger.Level;
        }

        public DebugLogger(LogLevel level)
        {
            Level = level;
        }
        #endregion
    }
}