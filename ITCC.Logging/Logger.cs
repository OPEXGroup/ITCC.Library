using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCC.Logging.Interfaces;

namespace ITCC.Logging
{
    /// <summary>
    ///     Logger based on events and subscribers
    /// </summary>
    public class Logger
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

        public static bool AddBannedScopeRange(IEnumerable scopeRange)
        {
            return scopeRange.Cast<object>().All(AddBannedScope);
        }

        /// <summary>
        ///     Creates log notification
        /// </summary>
        /// <param name="scope">Message context</param>
        /// <param name="level">Severity level</param>
        /// <param name="message">Notification message</param>
        public static void LogEntry(object scope, LogLevel level, string message)
        {
            if (level > Level || BannedScopes.Contains(scope.ToString()))
                return;
            var eventArgs = new LogEntryEventArgs(scope, level, message);
            LogEntryEvent?.Invoke(scope, eventArgs);
        }

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
            if (level > Level || BannedScopes.Contains(scope.ToString()))
                return;
            var description = new StringBuilder($"EXCEPTION ({exception.GetType().Name})\n");
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

        public void FlushAll()
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
                    MutableReceivers.ForEach(r => r.Level = value);
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

        private static readonly List<ILogReceiver> Receivers = new List<ILogReceiver>();

        private static LogLevel _logLevel;

        private static readonly List<ILogReceiver> MutableReceivers = new List<ILogReceiver>(); 

        private static readonly object LockObject = new object();
        #endregion
    }
}
