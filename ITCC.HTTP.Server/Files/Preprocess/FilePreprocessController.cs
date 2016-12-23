using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server.Files.Preprocess
{
    internal static class FilePreprocessController
    {
        #region public
        public static bool Start(int workerThreads, bool compressFiles)
        {
            var realThreadNumber = workerThreads > 0 ? workerThreads : Environment.ProcessorCount;
            for (var i = 0; i < realThreadNumber; ++i)
            {
                WorkerThreads.Add(new FilePreprocessorThread(TaskQueue, $"FPP-0{i + 1}", compressFiles));
            }
            WorkerThreads.ForEach(t => t.Start());

            return true;
        }

        public static void Stop() => WorkerThreads.ForEach(t => t.Stop());

        public static bool EnqueueFile(string fileName)
        {
            var task = FilePreprocessTaskFactory.BuildTask(fileName);
            if (task == null)
            {
                return false;
            }
            LogDebug($"Preprocess task queued for {fileName} ({task.FileType})");
            TaskQueue.Enqueue(task);
            return true;
        }

        public static bool FileInProgress(string fileName)
        {
            try
            {
                return TaskQueue.Any(t => t.FileName == fileName)
                    || WorkerThreads.Any(t => t.CurrentFile == fileName);
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                // Just in case
                return true;
            }
        }
        #endregion

        #region private

        private static readonly List<FilePreprocessorThread> WorkerThreads = new List<FilePreprocessorThread>();
        private static readonly ConcurrentQueue<BaseFilePreprocessTask> TaskQueue = new ConcurrentQueue<BaseFilePreprocessTask>();

        #endregion

        #region log

        [Conditional("DEBUG")]
        private static void LogDebug(string message) => Logger.LogDebug("FILE PROCESS", message);

        private static void LogMessage(LogLevel level, string message) => Logger.LogEntry("FILE PROCESS", level, message);

        private static void LogException(LogLevel level, Exception exception) => Logger.LogException("FILE PROCESS", level, exception);

        #endregion
    }
}
