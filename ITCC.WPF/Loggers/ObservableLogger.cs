// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Timers;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Interfaces;
using ITCC.WPF.Common;
using ITCC.WPF.Utils;
using ITCC.WPF.ViewModels;

namespace ITCC.WPF.Loggers
{
    public class ObservableLogger : IFlushableLogReceiver, IDisposable
    {
        public const double FlushInterval = 5;
        public const int BufferSize = 100;

        #region ILogReceiver
        public LogLevel Level { get; set; }

        public void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;
            
            _eventQueue.Enqueue(args);
            if (_eventQueue.Count >= BufferSize)
                Flush();
        }

        public Task Flush()
        {
            _uiThreadRunner.Invoke(() => {
                LogEntryEventArgs args;
                while (_eventQueue.TryDequeue(out args))
                {
                    LogEntryCollection.AddLast(new LogEntryEventArgsViewModel(args));
                }
            });

            return Task.FromResult(0);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _flushTimer.Stop();
            _flushTimer.Dispose();
        }

        #endregion

        #region public
        public ObservableLogger(int capacity, Delegates.UiThreadRunner uiThreadRunner)
        {
            Level = Logger.Level;
            LogEntryCollection = new ObservableRingBuffer<LogEntryEventArgsViewModel>(capacity);
            _uiThreadRunner = uiThreadRunner;
            InitTimer();
        }

        public ObservableLogger(LogLevel level, int capacity, Delegates.UiThreadRunner uiThreadRunner)
        {
            Level = level;
            LogEntryCollection = new ObservableRingBuffer<LogEntryEventArgsViewModel>(capacity);
            _uiThreadRunner = uiThreadRunner;
            InitTimer();
        }

        public ObservableRingBuffer<LogEntryEventArgsViewModel> LogEntryCollection { get; }

        #endregion

        #region private

        private void InitTimer()
        {
            _flushTimer = new Timer(FlushInterval);
            _flushTimer.Elapsed += (sender, args) => Flush();
            _flushTimer.Start();
        }

        private readonly ConcurrentQueue<LogEntryEventArgs> _eventQueue = new ConcurrentQueue<LogEntryEventArgs>();
        private readonly Delegates.UiThreadRunner _uiThreadRunner;
        private Timer _flushTimer;

        #endregion
    }
}