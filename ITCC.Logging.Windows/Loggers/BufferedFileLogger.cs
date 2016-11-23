using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Interfaces;
using ITCC.Logging.Windows.Utils;

namespace ITCC.Logging.Windows.Loggers
{
    public class BufferedFileLogger : FileLogger, IFlushableLogReceiver, IDisposable
    {
        #region IFlushableLogReceiver
        public override void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;
            _messageQueue.Enqueue(args);
        }

        public Task Flush()
        {
            FlushBuffer();
            return Task.FromResult(0);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Stop();
            _queueTimer.Dispose();
        }

        #endregion

        #region public
        /// <summary>
        ///     Default logger frequency in milliseconds
        /// </summary>
        public const double DefaultFrequency = 10000;

        public BufferedFileLogger(string filename, bool clearFile = false, double frequency = DefaultFrequency)
            : base(filename, clearFile)
        {
            InitTimer(frequency);
        }

        public BufferedFileLogger(string filename, LogLevel level, bool clearFile = false, double frequency = DefaultFrequency)
            : base(filename, level, clearFile)
        {
            InitTimer(frequency);
        }

        public void Start() => _queueTimer.Change(0, Frequency);

        public void Stop() => _queueTimer.Change(Timeout.Infinite, Timeout.Infinite);

        private bool FlushBuffer()
        {
            try
            {
                FileUtils.FlushLogQueue(Filename, _messageQueue);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException("FILELOGGING", LogLevel.Error, ex);
                return false;
            }
        }

        public int Frequency { get; private set; }
        #endregion

        #region private
        private void InitTimer(double frequency)
        {
            Frequency = Convert.ToInt32(Math.Floor(frequency));
            _queueTimer = new Timer(QueueTimerOnElapsed, null, 0, Frequency);
        }

        private void QueueTimerOnElapsed(object sender)
        {
            Task.Run(() => FlushBuffer());
        }

        private Timer _queueTimer;

        private readonly ConcurrentQueue<LogEntryEventArgs> _messageQueue = new ConcurrentQueue<LogEntryEventArgs>();
        #endregion
    }
}
