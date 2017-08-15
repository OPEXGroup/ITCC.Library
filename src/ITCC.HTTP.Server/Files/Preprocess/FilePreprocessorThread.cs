// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Concurrent;
using System.Threading;
using ITCC.HTTP.Common;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server.Files.Preprocess
{
    internal class FilePreprocessorThread
    {
        #region public
        public FilePreprocessorThread(ConcurrentQueue<BaseFilePreprocessTask> taskQueue, string name, bool compressFiles)
        {
            _name = name;
            _taskQueue = taskQueue;
            _compressFiles = compressFiles;
        }

        public void Start()
        {
            _thread = new Thread(ThreadFunc) { Name = _name };
            _thread.Start();
        }

        public void Join() => _thread.Join();

        public void Stop(bool hard = false)
        {
            lock (_stopLock)
            {
                _stopRequested = true;
            }

            if (hard)
                _thread.Abort();
        }

        public string CurrentFile
        {
            get
            {
                lock (_fileLock)
                {
                    return _currentFile;
                }
            }
            private set
            {
                lock (_fileLock)
                {
                    _currentFile = value;
                }
            }
        }

        #endregion

        #region private

        private void ThreadFunc()
        {
            LogMessage(LogLevel.Info, "File preprocessor thread started");
            while (true)
            {
                try
                {
                    lock (_stopLock)
                    {
                        if (_stopRequested)
                        {
                            LogMessage(LogLevel.Info, "File preprocessor thread stopped");
                            return;
                        }
                    }

                    if (!_taskQueue.TryDequeue(out BaseFilePreprocessTask task))
                    {
                        Thread.Sleep(Constants.FilesPreprocessorThreadSleepInterval);
                        continue;
                    }

                    CurrentFile = task.FileName;
                    Thread.MemoryBarrier();

                    if (!task.Perform())
                        LogMessage(LogLevel.Warning, $"Task for file {task.FileName} ({task.FileType}) failed");
                    else
                        LogDebug($"Task for file {task.FileName} ({task.FileType}) completed");

                    CurrentFile = null;
                    if (_compressFiles)
                    {
                        if (! task.ZipAllChanged())
                            LogMessage(LogLevel.Warning, $"Compression for file {task.FileName} ({task.FileType}) failed");
                        else
                            LogDebug($"Compression for file {task.FileName} ({task.FileType}) completed");
                    }
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

        private readonly object _fileLock = new object();
        private string _currentFile;

        private bool _stopRequested;
        private readonly object _stopLock = new object();

        private bool _compressFiles;
        
        #endregion

        #region log

        private void LogDebug(string message) => Logger.LogDebug("FILE PROCESS", message);

        private void LogMessage(LogLevel level, string message) => Logger.LogEntry("FILE PROCESS", level, message);

        private void LogException(LogLevel level, Exception exception) => Logger.LogException("FILE PROCESS", level, exception);

        #endregion
    }
}
