// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Interfaces;
using ITCC.UI.Common;
using ITCC.UI.Utils;
using ITCC.UI.ViewModels;

namespace ITCC.UI.Loggers
{
    public class ObservableLogger : IFlushableLogReceiver, IDisposable
    {
        public const int FlushInterval = 5;
        public const int BufferSize = 100;

        #region ILogReceiver
        public LogLevel Level { get; set; }

        public async void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;
            
            _eventQueue.Enqueue(args);
            if (_eventQueue.Count >= BufferSize)
                await FlushAsync();
        }

        public Task FlushAsync()
        {
            Action flushAction = () =>
            {
                LogEntryEventArgs args;
                while (_eventQueue.TryDequeue(out args))
                {
                    LogEntryCollection.AddLast(new LogEntryEventArgsViewModel(args));
                }
            };

            return _asyncUiThreadRunner.Invoke(flushAction);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _flushTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _flushTimer.Dispose();
        }

        #endregion

        #region public
        public ObservableLogger(int capacity, Delegates.AsyncUiThreadRunner asyncUiThreadRunner)
        {
            Level = Logger.Level;
            LogEntryCollection = new ObservableRingBuffer<LogEntryEventArgsViewModel>(capacity);
            _asyncUiThreadRunner = asyncUiThreadRunner;
            InitTimer();
        }

        public ObservableLogger(LogLevel level, int capacity, Delegates.AsyncUiThreadRunner asyncUiThreadRunner)
        {
            Level = level;
            LogEntryCollection = new ObservableRingBuffer<LogEntryEventArgsViewModel>(capacity);
            _asyncUiThreadRunner = asyncUiThreadRunner;
            InitTimer();
        }

        public ObservableRingBuffer<LogEntryEventArgsViewModel> LogEntryCollection { get; }

        #endregion

        #region private

        private void InitTimer()
        {
            _flushTimer = new Timer(async sender => await FlushAsync(), null, FlushInterval, FlushInterval);
        }

        private readonly ConcurrentQueue<LogEntryEventArgs> _eventQueue = new ConcurrentQueue<LogEntryEventArgs>();
        private readonly Delegates.AsyncUiThreadRunner _asyncUiThreadRunner;
        private Timer _flushTimer;

        #endregion
    }
}