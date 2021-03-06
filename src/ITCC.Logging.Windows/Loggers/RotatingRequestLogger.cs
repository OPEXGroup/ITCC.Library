﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using System.Linq;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Interfaces;
using ITCC.Logging.Windows.Utils;

namespace ITCC.Logging.Windows.Loggers
{
    /// <summary>
    ///     Logger than cat return last log entries by request
    /// </summary>
    public class RotatingRequestLogger : ILogReceiver
    {
        #region ILogReceiver
        public LogLevel Level { get; set; }
        public void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (Level < args.Level)
                return;

            _innerQueue.Enqueue(args);
        }
        #endregion

        #region public

        public RotatingRequestLogger(int capacity)
        {
            _innerQueue = new ConcurrentBoundedQueue<LogEntryEventArgs>(capacity);
            Level = Logger.Level;
        }

        public RotatingRequestLogger(int capacity, LogLevel level)
        {
            _innerQueue = new ConcurrentBoundedQueue<LogEntryEventArgs>(capacity);
            Level = level;
        }

        public List<LogEntryEventArgs> GetEntries() => _innerQueue.ToList();

        public List<LogEntryEventArgs> GetEntries(int count) => GetEntries().Take(count).ToList();

        public void Flush() => _innerQueue.Flush();

        #endregion

        #region private

        private readonly ConcurrentBoundedQueue<LogEntryEventArgs> _innerQueue;

        #endregion
    }
}
