// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using ITCC.Logging.Core;

namespace ITCC.Logging.Windows.Utils
{
    internal static class FileUtils
    {
        #region public

        public const int BufferSize = 256 * 1024;

        public static void FlushLogQueue(string fileName, ConcurrentQueue<LogEntryEventArgs> argsQueue)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    while (true)
                    {
                        var stringBuilder = new StringBuilder();
                        var count = 0;
                        while (!argsQueue.IsEmpty)
                        {
                            LogEntryEventArgs message;
                            if (argsQueue.TryDequeue(out message))
                            {
                                stringBuilder.AppendLine(message.ToString());
                                count++;
                                if (count >= BufferSize)
                                    break; // Stop dequeuing
                            }
                                
                        }
                        var resultingString = stringBuilder.ToString();
                        if (string.IsNullOrWhiteSpace(resultingString))
                            return;
                        streamWriter.Write(resultingString);
                        streamWriter.Flush();
                        if (count < BufferSize)
                            break;
                    }
                    
                }
            }
        }
        #endregion
    }
}
