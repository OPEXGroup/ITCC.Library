using System;
using ITCC.HTTP.Server.Enums;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server.Files.Preprocess
{
    internal abstract class BaseFilePreprocessTask
    {
        #region public
        public abstract FileType FileType { get; }

        public abstract string FileName { get; set; }

        public abstract bool Perform();

        #endregion

        #region log

        protected void LogTrace(string message) => Logger.LogTrace($"{FileType.ToString().ToUpper()} TASK", message);

        protected void LogDebug(string message) => Logger.LogDebug($"{FileType.ToString().ToUpper()} TASK", message);

        protected void LogMessage(LogLevel level, string message) => Logger.LogEntry($"{FileType.ToString().ToUpper()} TASK", level, message);

        protected void LogException(LogLevel level, Exception exception) => Logger.LogException($"{FileType.ToString().ToUpper()} TASK", level, exception);

        #endregion
    }
}
