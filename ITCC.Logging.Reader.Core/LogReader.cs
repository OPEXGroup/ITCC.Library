using System;
using System.Collections.Generic;
using System.IO;
using ITCC.Logging.Core;
using ITCC.Logging.Reader.Core.Utils;

namespace ITCC.Logging.Reader.Core
{
    /// <summary>
    ///     Reader should be created one-per-file
    /// </summary>
    public class LogReader
    {
        #region public

        /// <summary>
        ///     Files of that size can be stored in memory
        /// </summary>
        public const int InMemoryThreshold = 10 * 1024 * 1024;

        public LogReader(string filename)
        {
            Filename = filename;
        }

        public IEnumerable<LogEntryEventArgs> ReadAsEnumerable()
        {
            var readBuffer = new ByteBuffer(BufferSize);
            var entryBuffer = new ByteBuffer(EntryMaxSize);

            LogMessage(LogLevel.Debug, $"Reading file {Filename} of size {new FileInfo(Filename).Length}");
            using (var fileStream = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                _fileEnded = false;
                while (!_fileEnded)
                {
                    readBuffer.ReadStream(fileStream);
                    _fileEnded = !readBuffer.IsFull;
                    if (_fileEnded)
                        LogMessage(LogLevel.Trace, $"Got only {readBuffer.Count} bytes out of {readBuffer.Capacity}");

                    var boundIndex = FindEntryBound(readBuffer.Data, readBuffer.Count);
                    while (boundIndex != BoundNotFound)
                    {
                        entryBuffer.CopyFrom(readBuffer, boundIndex);
                        var entry = EntryTokenizer.ParseEntry(entryBuffer.Data);
                        yield return entry;
                        entryBuffer.Flush();

                        readBuffer.TruncateStart(boundIndex);
                        boundIndex = FindEntryBound(readBuffer.Data, readBuffer.Count);
                    }
                }
                LogMessage(LogLevel.Debug, "File ended");
            }
        }

        public string Filename { get; }
        #endregion


        #region private

        private int FindEntryBound(byte[] buffer, int bufferSize)
        {
            for (var i = 0; i < bufferSize - 1; ++i)
            {
                if (buffer[i] == '\n' && buffer[i + 1] == '[')
                    return i;
            }
            return BoundNotFound;
        }

        private void LogMessage(LogLevel level, string message) => Logger.LogEntry("LOGREADER", level, message);

        private const int BufferSize = 4096;
        private const int EntryMaxSize = BufferSize * 4;
        private const int BoundNotFound = -1;

        private bool _fileEnded;

        #endregion
    }
}
