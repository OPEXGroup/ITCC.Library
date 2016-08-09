using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server.Files.Preprocess
{
    internal static class FilePreprocessController
    {
        #region public
        public static bool Start(int workerThreads)
        {
            var realThreadNumber = workerThreads > 0 ? workerThreads : Environment.ProcessorCount;
            for (var i = 0; i < realThreadNumber; ++i)
            {
                WorkerThreads.Add(new FilePreprocessorThread(TaskQueue, $"FPP-0{i + 1}"));
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
            LogMessage(LogLevel.Debug, $"Preprocess task queued for {fileName} ({task.FileType})");
            TaskQueue.Enqueue(task);
            return true;
        }

        public static bool FileInProgress(string fileName)
        {
            try
            {
                return TaskQueue.Any(t => t.FileName == fileName);
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

        private static void LogMessage(LogLevel level, string message) => Logger.LogEntry("FILE PROCESS", level, message);

        private static void LogException(LogLevel level, Exception exception) => Logger.LogException("FILE PROCESS", level, exception);

        #endregion
    }
}
