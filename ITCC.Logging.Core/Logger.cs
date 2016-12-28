// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ITCC.Logging.Core.Interfaces;

namespace ITCC.Logging.Core
{
    /// <summary>
    ///     Logger based on events and subscribers
    /// </summary>
    public static class Logger
    {
        #region public
        /// <summary>
        ///     Actual logging event
        /// </summary>
        private static event EventHandler<LogEntryEventArgs> LogEntryEvent;

        /// <summary>
        ///     Registers receivers for log notifications
        /// </summary>
        /// <param name="receiver">Log receiver (instance)</param>
        /// <param name="mutableReceiver">If so, then receiver's LogLevel is always coherent with Logger's</param>
        /// <returns>Operation status</returns>
        public static bool RegisterReceiver(ILogReceiver receiver, bool mutableReceiver = false)
        {
            if (receiver == null)
                return false;
            lock (LockObject)
            {
                LogEntryEvent += receiver.WriteEntry;
                if (mutableReceiver)
                    MutableReceivers.Add(receiver);
                Receivers.Add(receiver);
            }

            return true;
        }

        /// <summary>
        ///     Unregisters receivers for log notifications
        /// </summary>
        /// <param name="receiver">Log receiver (instance)</param>
        /// <returns>Operation status</returns>
        public static bool UnregisterReceiver(ILogReceiver receiver)
        {
            if (receiver == null)
                return false;

            try
            {
                lock (LockObject)
                {
                    LogEntryEvent -= receiver.WriteEntry;
                    Receivers.Remove(receiver);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool AddBannedScope(object scope)
        {
            lock (LockObject)
            {
                if (scope == null)
                    return false;
                var scopeName = scope.ToString();
                if (BannedScopes.Contains(scopeName))
                    return false;
                BannedScopes.Add(scopeName);
                return true;
            }
        }

        public static bool AddBannedScopeRange(IEnumerable scopeRange) => scopeRange.Cast<object>().All(AddBannedScope);

        /// <summary>
        ///     Logs message in DEBUG level if DEBUG is defined
        /// </summary>
        /// <param name="scope">Message scope</param>
        /// <param name="message">Message text</param>
        [Conditional("DEBUG")]
        public static void LogDebug(object scope, string message) => LogEntry(scope, LogLevel.Debug, message);

        /// <summary>
        ///     Logs message in TRACE level if both DEBUG and TRACE are defined
        /// </summary>
        /// <param name="scope">Message scope</param>
        /// <param name="message">Message text</param>
        [Conditional("DEBUG")]
        public static void LogTrace(object scope, string message) => LogTraceInner(scope, message);

        /// <summary>
        ///     Creates log notification
        /// </summary>
        /// <param name="scope">Message context</param>
        /// <param name="level">Severity level</param>
        /// <param name="message">Notification message</param>
        public static void LogEntry(object scope, LogLevel level, string message)
        {
            if (level > Level || level == LogLevel.None || BannedScopes.Contains(scope.ToString()))
                return;
            var eventArgs = new LogEntryEventArgs(scope, level, message);
            LogEntryEvent?.Invoke(scope, eventArgs);
        }

        /// <summary>
        ///     Logs exception in TRACE level if both DEBUG and TRACE are defined
        /// </summary>
        /// <param name="scope">Exception scope</param>
        /// <param name="exception">Exception instance</param>
        [Conditional("DEBUG")]
        public static void LogExceptionTrace(object scope, Exception exception)
            => LogExceptionTraceInner(scope, exception);

        /// <summary>
        ///     Logs message in DEBUG level if DEBUG is defined
        /// </summary>
        /// <param name="scope">Exception scope</param>
        /// <param name="exception">Exception instance</param>
        [Conditional("DEBUG")]
        public static void LogExceptionDebug(object scope, Exception exception)
            => LogException(scope, LogLevel.Debug, exception);

        /// <summary>
        ///     Logs exception
        /// </summary>
        /// <param name="scope">Exception occurance scope</param>
        /// <param name="level">Severity level</param>
        /// <param name="exception">Exception object</param>
        public static void LogException(object scope, LogLevel level, Exception exception)
        {
            if (exception == null)
                return;
            if (level > Level || level == LogLevel.None || BannedScopes.Contains(scope.ToString()))
                return;
            var description = new StringBuilder($"EXCEPTION ({exception.GetType().GetTypeInfo().Name})\n");
            try
            {
                var tempException = exception;
                while (tempException != null)
                {
                    description.AppendLine(tempException.Message);
                    tempException = tempException.InnerException;
                }
                description.AppendLine(exception.StackTrace);
            }
            catch (Exception ex)
            {
                LogEntry(scope, level, $"Error logging exception: {ex.Message}");
            }

            var eventArgs = new LogEntryEventArgs(scope, level, description.ToString(), exception);
            LogEntryEvent?.Invoke(scope, eventArgs);
        }

        /// <summary>
        ///     Synchronously flushes all logger's output buffers (which implement <see cref="IFlushableLogReceiver"/>)
        /// </summary>
        public static void FlushAll()
        {
            lock (LockObject)
            {
                var flushTasks = Receivers.OfType<IFlushableLogReceiver>().Select(r => r.Flush()).ToArray();
                Task.WaitAll(flushTasks);
            }
        }

        /// <summary>
        ///     Logging level
        /// </summary>
        public static LogLevel Level
        {
            get
            {
                return _logLevel;
            }
            set
            {
                lock (LockObject)
                {
                    _logLevel = value;
                    foreach (var mutableReceiver in MutableReceivers)
                    {
                        mutableReceiver.Level = _logLevel;
                    }
                }
            }
        }
       
        public static List<string> BannedScopes { get; } = new List<string>();

        #endregion

        #region private
        static Logger()
        {
            _logLevel = LogLevel.Info;
        }

        /// <summary>
        ///     Used to acquire AND condition
        /// </summary>
        /// <param name="scope">Message scope</param>
        /// <param name="message">Message text</param>
        [Conditional("TRACE")]
        private static void LogTraceInner(object scope, string message) => LogEntry(scope, LogLevel.Trace, message);

        /// <summary>
        ///     Used to acquire AND condition
        /// </summary>
        /// <param name="scope">Exception scope</param>
        /// <param name="exception">Exception instance</param>
        [Conditional("TRACE")]
        private static void LogExceptionTraceInner(object scope, Exception exception) => LogException(scope, LogLevel.Trace, exception);

        private static readonly List<ILogReceiver> Receivers = new List<ILogReceiver>();

        private static LogLevel _logLevel;

        private static readonly List<ILogReceiver> MutableReceivers = new List<ILogReceiver>(); 

        private static readonly object LockObject = new object();
        #endregion
    }
}
