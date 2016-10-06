using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Timers;
using ITCC.HTTP.Server.Auth;
using ITCC.HTTP.Server.Enums;

namespace ITCC.HTTP.Server.Service
{
    internal class ServerStatistics<TAccount>
        where TAccount : class
    {
        /// <summary>
        ///     Milliseconds
        /// </summary>
        private const double MemortSamplingPeriod = 100;

        private readonly ConcurrentDictionary<int, int> _responseCodes = new ConcurrentDictionary<int, int>();

        private readonly object _requestMethodLock = new object();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _requestMethodCounters = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();

        private readonly ConcurrentDictionary<string, int> _requestSuccessCounters = new ConcurrentDictionary<string, int>();

        private readonly ConcurrentDictionary<string, int> _requestFailCounters = new ConcurrentDictionary<string, int>();

        private readonly ConcurrentDictionary<string, double> _requestSuccessTimeCounters = new ConcurrentDictionary<string, double>();

        private readonly ConcurrentDictionary<string, double> _requestFailTimeCounters = new ConcurrentDictionary<string, double>();

        private readonly ConcurrentDictionary<SslProtocols, int> _sslProtocolCounter = new ConcurrentDictionary<SslProtocols, int>(); 

        private readonly ConcurrentDictionary<AuthorizationStatus, int> _authentificationResults = new ConcurrentDictionary<AuthorizationStatus, int>();

        private readonly DateTime _startTime = DateTime.Now;

        private readonly Timer _memoryTimer;

        private long _minMemory = long.MaxValue;
        private long _maxMemory;
        private long _currentMemory;
        private double _totalMemory;
        private long _memorySamples;


        private readonly object _memoryLock = new object();

        private readonly object _counterLock = new object();

        /// <summary>
        ///     In milliseconds
        /// </summary>
        private double _minRequestTime = double.PositiveInfinity;

        /// <summary>
        ///     In milliseconds
        /// </summary>
        private double _maxRequestTime = double.NegativeInfinity;

        /// <summary>
        ///     In milliseconds
        /// </summary>
        private double _totalRequestTime;

        private string _slowestRequest;

        private long _requestCount;

        public ServerStatistics()
        {
            _memoryTimer = new Timer(MemortSamplingPeriod);
            _memoryTimer.Elapsed += MemoryTimerOnElapsed;
            _memoryTimer.Start();
        }

        private void MemoryTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (_memoryLock)
            {
                _memorySamples++;
                var currentMemory = GC.GetTotalMemory(false);
                _minMemory = Math.Min(_minMemory, currentMemory);
                _maxMemory = Math.Max(_maxMemory, currentMemory);
                _currentMemory = currentMemory;
                _totalMemory += currentMemory;
            }
        }

        public string Serialize()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Server started at {_startTime:s} ({DateTime.Now.Subtract(_startTime)} ago)");
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

            builder.AppendLine("SSL statistics:");
            var sslKeys = _sslProtocolCounter.Keys.ToList();
            sslKeys.Sort();
            sslKeys.ForEach(k => builder.AppendLine($"\t{k,12}: {_sslProtocolCounter[k]}"));
            builder.AppendLine();

            builder.AppendLine("Request statistics:");
            var uris = _requestMethodCounters.Keys.ToList();
            uris.Sort();
            lock (_requestMethodLock)
            {
                uris.ForEach(u =>
                {
                    var methodDict = _requestMethodCounters[u];
                    double totalSuccessTime;
                    double totalFailTime;
                    if (!_requestSuccessTimeCounters.TryGetValue(u, out totalSuccessTime))
                        totalSuccessTime = 0;
                    if (!_requestFailTimeCounters.TryGetValue(u, out totalFailTime))
                        totalFailTime = 0;
                    double averageSuccessTime;
                    double averageFailTime;
                    if (_requestSuccessCounters.ContainsKey(u))
                    {
                        averageSuccessTime = totalSuccessTime/_requestSuccessCounters[u];
                    }
                    else
                    {
                        averageSuccessTime = 0;
                    }

                    if (_requestFailCounters.ContainsKey(u))
                    {
                        averageFailTime = totalFailTime/_requestFailCounters[u];
                    }
                    else
                    {
                        averageFailTime = 0;
                    }

                    builder.AppendLine($"\t{u}");
                    builder.AppendLine($"\t\tAverage success time (2xx):      {averageSuccessTime,10} ms");
                    builder.AppendLine($"\t\tAverage fail    time (4xx, 5xx): {averageFailTime,10} ms");

                    var methodKeys = methodDict.Keys.ToList();
                    methodKeys.Sort();
                    methodKeys.ForEach(m => builder.AppendLine($"\t\t{m,-10}: {methodDict[m]}"));
                });
            }
            builder.AppendLine();

            if (_requestCount > 0)
            {
                builder.AppendLine("Total performance statistics:");
                builder.AppendLine($"\tRequest count:        {_requestCount}");
                builder.AppendLine($"\tAverage request time: {_totalRequestTime / _requestCount, 10} ms");
                builder.AppendLine($"\tMax     request time: {_maxRequestTime, 10} ms");
                builder.AppendLine($"\tMin     request time: {_minRequestTime, 10} ms");
                builder.AppendLine($"\tTotal   request time: {_totalRequestTime, 10} ms");
                builder.AppendLine($"\tSlowest request:      {_slowestRequest}");
            }

            if (_memorySamples > 0)
            {
                lock (_memoryLock)
                {
                    var averageMemory = _totalMemory/_memorySamples;
                    const int bytesInMegabyte = 1024*1024;
                    builder.AppendLine("Memory statistics:");
                    builder.AppendLine($"\tMin: {(double)_minMemory / bytesInMegabyte, 8:F1} MB");
                    builder.AppendLine($"\tMax: {(double)_maxMemory / bytesInMegabyte,8:F1} MB");
                    builder.AppendLine($"\tAvg: {averageMemory / bytesInMegabyte,8:F1} MB");
                    builder.AppendLine($"\tCur: {(double)_currentMemory / bytesInMegabyte,8:F1} MB");
                }
            }

            return builder.ToString();
        }

        public void AddResponse(HttpListenerResponse response, string uri, double processingTime)
        {
            _responseCodes.AddOrUpdate(response.StatusCode, 1, (key, value) => value + 1);

            lock (_counterLock)
            {
                _minRequestTime = Math.Min(processingTime, _minRequestTime);
                if (processingTime > _maxRequestTime)
                {
                    _maxRequestTime = processingTime;
                    _slowestRequest = uri;
                }
                _totalRequestTime += processingTime;
                _requestCount++;
            }

            if (HasGoodStatusCode(response))
            {
                _requestSuccessTimeCounters.AddOrUpdate(uri, processingTime, (key, value) => value + processingTime);
                _requestSuccessCounters.AddOrUpdate(uri, 1, (key, value) => value + 1);
            }
            else
            {
                _requestFailTimeCounters.AddOrUpdate(uri, processingTime, (key, value) => value + processingTime);
                _requestFailCounters.AddOrUpdate(uri, 1, (key, value) => value + 1);
            }
        }

        public void AddRequest(HttpListenerRequest request)
        {
            var uri = request.Url.LocalPath.TrimEnd('/');
            lock (_requestMethodLock)
            {
                if (!_requestMethodCounters.ContainsKey(uri))
                {
                    _requestMethodCounters[uri] = new ConcurrentDictionary<string, int>();
                    _requestMethodCounters[uri].TryAdd(request.HttpMethod, 1);
                }
                else
                {
                    _requestMethodCounters[uri].AddOrUpdate(request.HttpMethod, 1, (key, value) => value + 1);
                }
            }
        }

        public void AddAuthResult(AuthorizationResult<TAccount> authResult)
        {
            _authentificationResults.AddOrUpdate(authResult.Status, 1, (key, value) => value + 1);
        }

        public void AddSslProtocol(SslProtocols protocol)
        {
            _sslProtocolCounter.AddOrUpdate(protocol, 1, (key, value) => value + 1);
        }

        private bool HasGoodStatusCode(HttpListenerResponse response) => response.StatusCode/100 < 4;
    }
}
