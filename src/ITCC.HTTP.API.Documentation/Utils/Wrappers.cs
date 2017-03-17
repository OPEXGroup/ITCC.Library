// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Text;
using System.Threading.Tasks;
using ITCC.Logging.Core;

namespace ITCC.HTTP.API.Documentation.Utils
{
    internal static class Wrappers
    {
        public static bool DoSafe(Func<bool> method)
        {
            try
            {
                return method.Invoke();
            }
            catch (Exception exception)
            {
                LogExceptionDebug(exception);
                return false;
            }
        }

        public static async Task<bool> DoSafeAsync(Func<Task<bool>> asyncMethod)
        {
            try
            {
                return await asyncMethod.Invoke();
            }
            catch (Exception exception)
            {
                LogExceptionDebug(exception);
                return false;
            }
        }

        public static void AppendPaddedLines(StringBuilder builder, params string[] lines)
        {
            if (lines.Length == 0)
                return;

            builder.AppendLine();
            foreach (var line in lines)
            {
                builder.AppendLine(line);
                builder.AppendLine();
            }
        }

        private static void LogExceptionDebug(Exception exception) => Logger.LogExceptionDebug(LogScope, exception);
        private const string LogScope = "DOC GENER";
    }
}
