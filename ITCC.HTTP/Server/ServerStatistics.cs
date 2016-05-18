using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Griffin.Net.Protocols.Http;
using ITCC.HTTP.Enums;

namespace ITCC.HTTP.Server
{
    internal class ServerStatistics<TAccount>
        where TAccount : class
    {
        private readonly ConcurrentDictionary<int, int> _responseCodes = new ConcurrentDictionary<int, int>();

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _requestCounters = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();

        private readonly ConcurrentDictionary<string, int> _legacyRequestCounter = new ConcurrentDictionary<string, int>();

        private readonly ConcurrentDictionary<AuthorizationStatus, int> _authentificationResults = new ConcurrentDictionary<AuthorizationStatus, int>();

        private readonly DateTime _startTime = DateTime.Now;

        public string Serialize()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Server started at {_startTime.ToString("s")} ({DateTime.Now.Subtract(_startTime)} ago)");
            builder.AppendLine();

            builder.AppendLine("Legacy request code statistics:");
            var legacyKeys = _legacyRequestCounter.Keys.ToList();
            legacyKeys.Sort();
            legacyKeys.ForEach(k => builder.AppendLine($"\t{k}: {_legacyRequestCounter[k]}"));
            builder.AppendLine();

            builder.AppendLine("Response code statistics:");
            var keys = _responseCodes.Keys.ToList();
            keys.Sort();
            keys.ForEach(k => builder.AppendLine($"\t{k}: {_responseCodes[k]}"));
            builder.AppendLine();

            builder.AppendLine("Authcode statistics:");
            var authKeys = _authentificationResults.Keys.ToList();
            authKeys.Sort();
            authKeys.ForEach(k => builder.AppendLine($"\t{k, 12}: {_authentificationResults[k]}"));
            builder.AppendLine();

            builder.AppendLine("Request statistics:");
            var uris = _requestCounters.Keys.ToList();
            uris.Sort();
            uris.ForEach(u =>
            {
                builder.AppendLine($"\t{u}");
                var methodDict = _requestCounters[u];
                var methodKeys = methodDict.Keys.ToList();
                methodKeys.Sort();
                methodKeys.ForEach(m => builder.AppendLine($"\t\t{m, 10}: { methodDict[m]}"));
            });
            builder.AppendLine();

            return builder.ToString();
        }

        public void AddResponse(HttpResponse response)
        {
            if (!_responseCodes.ContainsKey(response.StatusCode))
            {
                _responseCodes[response.StatusCode] = 1;
            }
            else
            {
                _responseCodes[response.StatusCode]++;
            }
        }

        public void AddRequest(HttpRequest request)
        {
            var uri = request.Uri.LocalPath.TrimEnd('/');
            if (!_requestCounters.ContainsKey(uri))
            {
                _requestCounters[uri] = new ConcurrentDictionary<string, int>();
                _requestCounters[uri].TryAdd(request.HttpMethod, 1);
            }
            else
            {
                if (!_requestCounters[uri].ContainsKey(request.HttpMethod))
                {
                    _requestCounters[uri][request.HttpMethod] = 1;
                }
                else
                {
                    _requestCounters[uri][request.HttpMethod]++;
                }
            }
        }

        public void AddRequestProcessor(RequestProcessor<TAccount> requestProcessor)
        {
            var legacyMethod = requestProcessor.LegacyName;
            if (legacyMethod == null)
                return;
            if (!_legacyRequestCounter.ContainsKey(legacyMethod))
            {
                _legacyRequestCounter[legacyMethod] = 1;
            }
            else
            {
                _legacyRequestCounter[legacyMethod]++;
            }
        }

        public void AddAuthResult(AuthorizationResult<TAccount> authResult)
        {
            if (! _authentificationResults.ContainsKey(authResult.Status))
            {
                _authentificationResults[authResult.Status] = 1;
            }
            else
            {
                _authentificationResults[authResult.Status]++;
            }
        }
    }
}
