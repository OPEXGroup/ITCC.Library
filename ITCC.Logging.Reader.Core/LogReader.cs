using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
                    var readCount = readBuffer.ReadStream(fileStream);
                    LogMessage(LogLevel.Trace, $"Read {readCount} bytes");
                    _fileEnded = !readBuffer.IsFull;
                    if (_fileEnded)
                        LogMessage(LogLevel.Trace, $"Got only {readBuffer.Count} bytes out of {readBuffer.Capacity}");

                    var boundIndex = FindEntryBound(readBuffer.Data, readBuffer.Count);
                    while (boundIndex.Item1 != BoundNotFound)
                    {
                        entryBuffer.CopyFrom(readBuffer, boundIndex.Item1 - 1);
                        var slice = new byte[entryBuffer.Count - 1];
                        Array.Copy(entryBuffer.Data, slice, entryBuffer.Count - 1);
                        LogMessage(LogLevel.Info, $"First symbol: {(char)slice[0]}; Last symbol: {(char)slice[entryBuffer.Count - 2]}");
                        var entry = EntryTokenizer.ParseEntry(slice);
                        if (entry != null)
                        {
                            yield return entry;
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(slice);
                            LogMessage(LogLevel.Warning, $"Failed to parse {str}");
                        }
                        entryBuffer.Flush();

                        readBuffer.TruncateStart(boundIndex.Item1 + boundIndex.Item2);
                        LogMessage(LogLevel.Trace, $"readBuffer starts with {(short) readBuffer.Data[0]}");
                        boundIndex = FindEntryBound(readBuffer.Data, readBuffer.Count);
                    }

                    entryBuffer.CopyFrom(readBuffer);
                    readBuffer.Flush();
                }
                
                LogMessage(LogLevel.Debug, "File ended");
            }
        }

        public string Filename { get; }
        #endregion


        #region private

        private Tuple<int, int> FindEntryBound(byte[] buffer, int bufferSize)
        {
            var result = BoundNotFound;
            for (var i = 0; i < bufferSize - 2; ++i)
            {
                if (buffer[i] == '\r' && buffer[i + 1] == '[')
                {
                    return new Tuple<int, int>(i, 1);
                }
                if (buffer[i] == '\r' && buffer[i + 1] == '\n' && buffer[i + 2] == '[')
                {
                    return new Tuple<int, int>(i, 2);
                }
            }
            return new Tuple<int, int>(BoundNotFound, -1);
        }

        [Conditional("DEBUG")]
        private void LogMessage(LogLevel level, string message) => Logger.LogEntry("LOGREADER", level, message);

        private const int BufferSize = 4096;
        private const int EntryMaxSize = BufferSize * 4;
        private const int BoundNotFound = -1;

        private bool _fileEnded;

        #endregion
    }
}
