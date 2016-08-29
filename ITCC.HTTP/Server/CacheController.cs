using System;
using System.Collections.Concurrent;
using ITCC.HTTP.Enums;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server
{
    internal static class CacheController<TAccount>
        where TAccount : class, IEquatable<TAccount>
    {
        #region public

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
                Logger.LogException("SERVERCACHE", LogLevel.Warning, oom);
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
        #endregion

    }
}
