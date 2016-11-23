using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ITCC.Logging.Core.Interfaces;
using ITCC.Logging.Core.Utils;

namespace ITCC.Logging.Core.Loggers
{
    public class PortableBufferedRotatingFileLogger : IFlushableLogReceiver
    {
        #region IFlushableLogReceiver
        public LogLevel Level { get; set; }
        public void WriteEntry(object sender, LogEntryEventArgs args)
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

        #region public
        public const double DefaultFrequency = 10000;

        public PortableBufferedRotatingFileLogger(string filenamePrefix, LogLevel level, int filesCount = 10, long maxFileSize = 10 * 1024 * 1024, double frequency = DefaultFrequency)
        {
            if (filenamePrefix == null)
                throw new ArgumentNullException(nameof(filenamePrefix));

            if (filesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(filesCount), "filesCount < 0");

            if (maxFileSize < 1)
                throw new ArgumentOutOfRangeException(nameof(maxFileSize), "maxFileSize < 1");

            if (frequency < 1)
                throw new ArgumentOutOfRangeException(nameof(frequency), "frequency < 1");

            FilenamePrefix = filenamePrefix;
            Level = level;
            FilesCount = filesCount;
            MaxFileSize = maxFileSize;
            Frequency = Convert.ToInt32(Math.Floor(frequency));
            
            InitTimer();
        }

        public void Start() => _queueTimer.Change(0, Frequency);

        public void Stop() => _queueTimer.Change(Timeout.Infinite, Timeout.Infinite);

        public int FilesCount { get; }

        public long MaxFileSize { get; }

        public string FilenamePrefix { get; }

        public int Frequency { get; }
        #endregion

        #region private
        private void InitTimer()
        {
            _queueTimer = new Timer(QueueTimerOnElapsed, null, 0, Frequency);
        }

        private bool FlushBuffer()
        {
            try
            {
                var currentFileName = MakeFilename(0);
                if (File.Exists(currentFileName))
                {
                    if (new FileInfo(currentFileName).Length > MaxFileSize)
                        Rotate();
                }
                FileUtils.FlushLogQueue(currentFileName, _messageQueue);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException("FILE LOG", LogLevel.Error, ex);
                return false;
            }
        }

        private void Rotate()
        {
            var lastFileName = MakeFilename(FilesCount - 1);
            if (File.Exists(lastFileName))
                File.Delete(lastFileName);
            for (var i = FilesCount - 2; i >= 0; --i)
            {
                var name = MakeFilename(i);
                if (File.Exists(name))
                {
                    var newName = MakeFilename(i + 1);
                    File.Move(name, newName);
                }
            }
        }

        private string MakeFilename(int index) => $"{FilenamePrefix}_{index}.txt";

        private void QueueTimerOnElapsed(object sender) => Task.Run(() => FlushBuffer());

        private Timer _queueTimer;

        private readonly ConcurrentQueue<LogEntryEventArgs> _messageQueue = new ConcurrentQueue<LogEntryEventArgs>();
        #endregion
    }
}
