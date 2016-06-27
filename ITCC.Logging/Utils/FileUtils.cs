using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace ITCC.Logging.Utils
{
    internal static class FileUtils
    {
        #region public
        public static void FlushLogQueue(string fileName, ConcurrentQueue<LogEntryEventArgs> argsQueue)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    var stringBuilder = new StringBuilder();
                    while (!argsQueue.IsEmpty)
                    {
                        LogEntryEventArgs message;
                        if (argsQueue.TryDequeue(out message))
                            stringBuilder.AppendLine(message.ToString());
                    }
                    var resultingString = stringBuilder.ToString();
                    if (string.IsNullOrWhiteSpace(resultingString))
                        return;
                    streamWriter.WriteLine(stringBuilder.ToString());
                    streamWriter.Flush();
                }
            }
        }
        #endregion
    }
}
