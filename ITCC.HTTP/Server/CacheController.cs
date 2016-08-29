using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ITCC.HTTP.Enums;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server
{
    internal static class CacheController<TAccount>
        where TAccount : class, IEquatable<TAccount>
    {
        #region public

        public static async Task<HandlerResult> Process(HttpListenerContext context, TAccount account, RequestProcessor<TAccount> requestProcessor)
        {
            if (requestProcessor.Method != HttpMethod.Get &&
                                    requestProcessor.Method != HttpMethod.Head)
            {
                LogMessage(LogLevel.Debug, "Dropping cache");
                Clear();
                return await requestProcessor.Handler.Invoke(account, context.Request).ConfigureAwait(false);
            }
            else
            {
                HandlerResult cacheResult;
                if (TryGet(account, requestProcessor, out cacheResult))
                {
                    LogMessage(LogLevel.Debug, "Data loaded from cache");
                    return cacheResult;
                }
                LogMessage(LogLevel.Debug, "Cache miss");
                var handleResult = await requestProcessor.Handler.Invoke(account, context.Request).ConfigureAwait(false);
                AddOrUpdate(account, requestProcessor, handleResult);
                return handleResult;
            }
        }

        public static bool AddOrUpdate(TAccount account, RequestProcessor<TAccount> requestProcessor, HandlerResult data)
        {
            try
            {
                if (requestProcessor.CachePolicy == CachePolicy.NoCache)
                    return true;
                var key = BuildAccountProcessorPair(account, requestProcessor);

                CacheDictionary[key] = data;
                return true;
            }
            catch (OutOfMemoryException oom)
            {
                LogException(LogLevel.Warning, oom);
                Clear();
                return false;
            }
        }

        public static bool TryGet(TAccount account, RequestProcessor<TAccount> requestProcessor, out HandlerResult result)
        {
            if (requestProcessor.CachePolicy == CachePolicy.NoCache)
            {
                result = null;
                return false;
            }

            var key = BuildAccountProcessorPair(account, requestProcessor);
            if (! CacheDictionary.ContainsKey(key))
            {
                result = null;
                return false;
            }

            result = CacheDictionary[key];
            return true;
        }

        public static void Clear() => CacheDictionary.Clear();

        #endregion

        #region private

        private static AccountProcessorPair<TAccount> BuildAccountProcessorPair(TAccount account,
            RequestProcessor<TAccount> requestProcessor)
        {
            var key = new AccountProcessorPair<TAccount> { RequestProcessor = requestProcessor };
            if (requestProcessor.CachePolicy == CachePolicy.PrivateCache)
                key.Account = account;
            return key;
        }

        private static readonly ConcurrentDictionary<AccountProcessorPair<TAccount>, HandlerResult> CacheDictionary
            = new ConcurrentDictionary<AccountProcessorPair<TAccount>, HandlerResult>(new CacheEqualityComparer<TAccount>());

        private static void LogMessage(LogLevel level, string message) => Logger.LogEntry("SERVER CACHE", level, message);
        private static void LogException(LogLevel level, Exception ex) => Logger.LogException("SERVER CACHE", level, ex);
        #endregion

    }
}
