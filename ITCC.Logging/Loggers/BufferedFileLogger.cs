using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ITCC.Logging.Loggers
{
    public class BufferedFileLogger : FileLogger
    {
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
            Frequency = frequency;
        }

        public void Start()
        {
            _queueTimer.Enabled = true;
        }

        public void Stop()
        {
            _queueTimer.Enabled = false;
        }

        private void InitTimer(double frequency)
        {
            Frequency = frequency;
            _queueTimer = new Timer(frequency);
            _queueTimer.Elapsed += QueueTimerOnElapsed;
            _queueTimer.AutoReset = true;
            _queueTimer.Enabled = true;
        }

        /// <summary>
        ///     Flushes the whole queue into the file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="elapsedEventArgs"></param>
        private void QueueTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Task.Run(() => FlushBuffer());
        }

        public override void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;
            _messageQueue.Enqueue(args);
        }

        public bool FlushBuffer()
        {
            try
            {
                using (var fileStream = new FileStream(Filename, FileMode.Append, FileAccess.Write))
                {
                    using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        var stringBuilder = new StringBuilder();
                        while (!_messageQueue.IsEmpty)
                        {
                            LogEntryEventArgs message;
                            if (_messageQueue.TryDequeue(out message))
                                stringBuilder.AppendLine(message.ToString());
                        }
                        var resultingString = stringBuilder.ToString();
                        if (string.IsNullOrWhiteSpace(resultingString))
                            return true;
                        streamWriter.WriteLine(stringBuilder.ToString());
                        streamWriter.Flush();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException("FILELOGGING", LogLevel.Error, ex);
                return false;
            }
        }

        public double Frequency { get; private set; }

        private Timer _queueTimer;

        private readonly ConcurrentQueue<LogEntryEventArgs> _messageQueue = new ConcurrentQueue<LogEntryEventArgs>();
    }
}
