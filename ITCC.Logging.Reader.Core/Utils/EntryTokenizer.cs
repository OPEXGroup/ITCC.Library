
using System;
using System.Text;
using ITCC.Logging.Core;

namespace ITCC.Logging.Reader.Core.Utils
{
    public class EntryTokenizer
    {
        #region public
        /// <summary>
        ///     We parse line that:
        ///         1) Starts with [
        ///         2) DOES NOT contain '\n' at the end
        /// </summary>
        /// <param name="segment">Line bytes</param>
        /// <returns>Parsed entry or null in case of error</returns>
        public static LogEntryEventArgs ParseEntry(byte[] segment)
        {
            var tokenizer = new EntryTokenizer(segment);
            return tokenizer.ParseEntry();
        }
        #endregion

        #region private

        private EntryTokenizer(byte[] segment)
        {
            _segment = Encoding.UTF8.GetString(segment).ToCharArray();
            _position = 0;
        }

        private LogEntryEventArgs ParseEntry()
        {
            var dateString = ParseNextSegment();
            DateTime date;
            LogMessage(LogLevel.Trace, $"Date string: {dateString}");
            if (!DateTime.TryParse(dateString, out date))
            {
                return null;
            }
            if (!CheckNextSegmentExists())
                return null;

            var levelString = ParseNextSegment();
            var level = ParseLogLevel(levelString);
            if (level == LogLevel.None)
            {
                return null;
            }
            if (!CheckNextSegmentExists())
                return null;

            Skip(ThreadMark);
            var threadString = ParseNextSegment();
            int threadId;
            if (!int.TryParse(threadString, out threadId))
            {
                return null;
            }
            if (!CheckNextSegmentExists())
                return null;

            var scope = ParseNextSegment(false);

            var message = ReadToEnd();

            return LogEntryEventArgs.CreateFromRawData(date, level, threadId, scope, message);
        }

        private string ParseNextSegment(bool leftAligned = true)
        {
            var newPosition = _position;
            int actualStart;
            int actualEnd;
            if (leftAligned)
            {
                actualStart = newPosition;
                var possibleEnd = -1;
                var wasWhiteSpace = false;
                while (_segment[newPosition] != PartEnd)
                {
                    if (_segment[newPosition] == WhiteSpace)
                    {
                        if (!wasWhiteSpace)
                            possibleEnd = newPosition - 1;
                        wasWhiteSpace = true;
                    }
                    else
                    {
                        possibleEnd = newPosition;
                        wasWhiteSpace = false;
                    }

                    newPosition++;
                }

                actualEnd = wasWhiteSpace ? possibleEnd : newPosition - 1;
            }
            else
            {
                while (_segment[newPosition] == WhiteSpace)
                {
                    newPosition++;
                }
                actualStart = newPosition;
                while (_segment[newPosition] != PartEnd)
                {
                    newPosition++;
                }
                actualEnd = newPosition - 1;
            }

            // Skip "] [" and go to next segment
            _position = newPosition + 3;

            var actualLength = actualEnd - actualStart + 1;
            var buffer = new char[actualLength];
            Array.Copy(_segment, buffer, actualLength);
            return buffer.ToString();
        }

        private bool CheckNextSegmentExists()
        {
            var exists = _segment[_position - 1] == PartStart;
            if (! exists)
                LogMessage(LogLevel.Trace, "Next segment does not exist");
            return exists;
        } 

        private string ReadToEnd()
        {
            // Now we have no [
            _position--;
            var builder = new StringBuilder();
            builder.Append(_segment, _position, _segment.Length - 1);
            return builder.ToString();
        }

        private  void Skip(string mark)
        {
            var bytes = mark.ToCharArray();
            var i = 0;
            while (_segment[_position + i] == bytes[i])
            {
                i++;
            }
            _position += i;
        }

        private static LogLevel ParseLogLevel(string representation)
        {
            switch (representation)
            {
                case "TRACE":
                    return LogLevel.Trace;
                case "DEBUG":
                    return LogLevel.Debug;
                case "INFO":
                    return LogLevel.Info;
                case "WARN":
                    return LogLevel.Warning;
                case "ERROR":
                    return LogLevel.Error;
                case "CRIT":
                    return LogLevel.Critical;
                default:
                    return LogLevel.None;
            }
        }

        private void LogMessage(LogLevel level, string message) => Logger.LogEntry("TOKENIZER", level, message);

        private const string ThreadMark = @"THR ";
        private const char PartStart = '[';
        private const char PartEnd = ']';
        private const char WhiteSpace = ' ';

        private int _position;
        private readonly char[] _segment;

        #endregion
    }
}
