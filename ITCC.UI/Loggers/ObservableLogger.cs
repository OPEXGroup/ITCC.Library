using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Timers;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Interfaces;
using ITCC.UI.Utils;
using ITCC.UI.ViewModels;
using static ITCC.UI.Common.Delegates;

namespace ITCC.UI.Loggers
{
    public class ObservableLogger : IFlushableLogReceiver
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
            
            return Task.CompletedTask;
        }

        #endregion

        #region public
        public ObservableLogger(int capacity, UiThreadRunner uiThreadRunner)
        {
            Level = Logger.Level;
            LogEntryCollection = new ObservableRingBuffer<LogEntryEventArgsViewModel>(capacity);
            _uiThreadRunner = uiThreadRunner;
            InitTimer();
        }

        public ObservableLogger(LogLevel level, int capacity, UiThreadRunner uiThreadRunner)
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
        private readonly UiThreadRunner _uiThreadRunner;
        private Timer _flushTimer;

        #endregion
    }
}