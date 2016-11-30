using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ITCC.Logging.Core.Interfaces;
using ITCC.Logging.Core.Utils;

namespace ITCC.Logging.Core.Loggers
{
    /// <summary>
    ///     Used for .Net Core apps. MUST NOT be used for .Net 4.6 apps
    /// </summary>
    public class PortableBufferedFileLogger : PortableFileLogger, IFlushableLogReceiver, IDisposable
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

        public PortableBufferedFileLogger(string filename, bool clearFile = false, double frequency = DefaultFrequency)
            : base(filename, clearFile)
        {
            InitTimer(frequency);
        }

        public PortableBufferedFileLogger(string filename, LogLevel level, bool clearFile = false, double frequency = DefaultFrequency)
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
