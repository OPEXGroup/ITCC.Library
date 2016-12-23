using System;
using ITCC.HTTP.Server.Enums;
using ITCC.Logging.Core;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace ITCC.HTTP.Server.Files.Preprocess
{
    internal abstract class BaseFilePreprocessTask
    {
        #region public
        public abstract FileType FileType { get; }

        public abstract string FileName { get; set; }

        /// <summary>
        ///     Get all files after type-specific conversions, including original one
        /// </summary>
        /// <returns></returns>
        public abstract List<string> GetAllFiles();

        public virtual bool Perform() => true;

        public bool ZipAllChanged()
        {
            try
            {
                GetAllFiles().ForEach(Compress);
                return true;
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                return false;
            }
        }

        #endregion

        #region zip
        private void Compress(string filename)
        {
            LogDebug($"Compressing {filename}");
            var gzipName = GzipName(filename);

            using (var originalFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var compressedFileStream = File.Create(gzipName))
                {
                    using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                    {
                        // This is CPU-bound, do synchronously
                        originalFileStream.CopyTo(compressionStream);
                    }
                }
            }
            LogDebug($"Compressed {filename}");
        }

        private static string GzipName(string originalName) => originalName + ".gz";

        #endregion

        #region log

        protected void LogTrace(string message) => Logger.LogTrace($"{FileType.ToString().ToUpper()} TASK", message);

        protected void LogDebug(string message) => Logger.LogDebug($"{FileType.ToString().ToUpper()} TASK", message);

        protected void LogMessage(LogLevel level, string message) => Logger.LogEntry($"{FileType.ToString().ToUpper()} TASK", level, message);

        protected void LogException(LogLevel level, Exception exception) => Logger.LogException($"{FileType.ToString().ToUpper()} TASK", level, exception);

        #endregion
    }
}
