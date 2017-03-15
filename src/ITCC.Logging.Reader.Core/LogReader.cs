// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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

            var totalRead = 0;
            LogMessage(LogLevel.Debug, $"Reading file {Filename} of size {new FileInfo(Filename).Length}");
            using (var fileStream = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                _fileEnded = false;
                while (!_fileEnded)
                {
                    var offset = 0;
                    var readCount = readBuffer.ReadStream(fileStream);
                    totalRead += readCount;
                    LogMessage(LogLevel.Trace, $"Read {readCount} bytes ({totalRead} total)");
                    _fileEnded = !readBuffer.IsFull;
                    if (_fileEnded)
                        LogMessage(LogLevel.Trace, $"Got only {readBuffer.Count} bytes out of {readBuffer.Capacity}");

                    var boundIndex = FindEntryBound(readBuffer.Data, readBuffer.Count);
                    Console.WriteLine(boundIndex.Item1);
                    while (boundIndex.Item1 != BoundNotFound)
                    {
                        entryBuffer.CopyFrom(readBuffer, boundIndex.Item1);
                        offset += boundIndex.Item1 + boundIndex.Item2;
                        LogMessage(LogLevel.Info, $"Current block offset: {offset}");
                        readBuffer.TruncateStart(boundIndex.Item1 + boundIndex.Item2);
                        var slice = new byte[entryBuffer.Count - 1];
                        Array.Copy(entryBuffer.Data, slice, entryBuffer.Count - 1);
                        entryBuffer.Flush();
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
                        
                        boundIndex = FindEntryBound(readBuffer.Data, readBuffer.Count);
                    }

                    entryBuffer.CopyFrom(readBuffer);
                    LogMessage(LogLevel.Info, $"Data left: {readBuffer.ToUtf8String()}");
                    readBuffer.Flush();
                }
                
                LogMessage(LogLevel.Debug, "File ended");
            }
        }

        public string Filename { get; }
        #endregion


        #region private

        /// <summary>
        ///     Looks for log entry bound (first symbol that is not in entry)
        /// </summary>
        /// <param name="buffer">Buffer for lookup</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <returns>
        ///     Tuple. First element is index and second is line termination symbol count (1 for \r or \n and 2 for \r\n)
        /// </returns>
        private static Tuple<int, int> FindEntryBound(byte[] buffer, int bufferSize)
        {
            for (var i = 0; i < bufferSize - 2; ++i)
            {
                if ((buffer[i] == '\r' || buffer[i] == '\n') && buffer[i + 1] == '[')
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
