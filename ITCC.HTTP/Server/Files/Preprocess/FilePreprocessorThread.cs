using System;
using System.Collections.Concurrent;
using System.Threading;
using ITCC.Logging;

namespace ITCC.HTTP.Server.Files.Preprocess
{
    internal class FilePreprocessorThread
    {
        #region public
        public FilePreprocessorThread(ConcurrentQueue<BaseFilePreprocessTask> taskQueue, string name)
        {
            _name = name;
            _taskQueue = taskQueue;
        }

        public void Start()
        {
            _thread = new Thread(ThreadFunc) { Name = _name };
            _thread.Start();
        }

        public void Join() => _thread.Join();

        public void Stop() => _thread.Abort();

        #endregion

        #region private

        private void ThreadFunc()
        {
            LogMessage(LogLevel.Info, "File preprocessor thread started");
            while (true)
            {
                try
                {
                    BaseFilePreprocessTask task;
                    if (!_taskQueue.TryDequeue(out task))
                    {
                        Thread.Sleep(Constants.FilesPreprocessorThreadSleepInterval);
                        continue;
                    }

                    if (!task.Perform())
                        LogMessage(LogLevel.Warning, $"Task for file {task.FileName} ({task.FileType}) failed");
                    else
                        LogMessage(LogLevel.Debug, $"Task for file {task.FileName} ({task.FileType}) completed");
                }
                catch (ThreadAbortException)
                {
                    LogMessage(LogLevel.Info, "File preprocessor thread stopped");
                    return;
                }
                catch (Exception ex)
                {
                    LogException(LogLevel.Warning, ex);
                }
            }
        }

        private readonly string _name;
        private Thread _thread;
        private readonly ConcurrentQueue<BaseFilePreprocessTask> _taskQueue;
        #endregion

        #region log

        private void LogMessage(LogLevel level, string message) => Logger.LogEntry("FILE PROCESS", level, message);

        private void LogException(LogLevel level, Exception exception) => Logger.LogException("FILE PROCESS", level, exception);

        #endregion
    }
}
