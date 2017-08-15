// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;
using ITCC.HTTP.Server.Auth;
using ITCC.HTTP.Server.Enums;
using ITCC.HTTP.Server.Interfaces;
using ITCC.HTTP.Server.Utils;
using ITCC.Logging.Core;
using Timer = System.Timers.Timer;

namespace ITCC.HTTP.Server.Service
{
    internal class ServerStatistics<TAccount> : IDisposable
        where TAccount : class
    {
        #region IDisposable

        public void Dispose()
        {
            _processMemoryTimer.Stop();
            _threadsTimer.Stop();
            _processMemoryTimer.Dispose();
            _threadsTimer.Dispose();
            _totalCpuTimer.Stop();
            _totalCpuTimer.Dispose();
            _totalCpuCounter.Dispose();
        }

        #endregion

        #region public 
        public ServerStatistics(int criticalMemoryValue, MemoryAlarmStrategy memoryAlarmStrategy)
        {
            _criticalMemoryValue = criticalMemoryValue;
            _counterProvider = CounterProviderFactory.BuildCounterProvider(memoryAlarmStrategy, StartMemoryWarningCount);
            _memoryIntervalSamplesLeft = _counterProvider.GetNextCount();
            _memorySampleCount = _memoryIntervalSamplesLeft;
            _memoryOverheadOccured = false;

            _processMemoryTimer = new Timer(MemorySamplingPeriod);
            _processMemoryTimer.Elapsed += ProcessMemoryTimerOnElapsed;
            _processMemoryTimer.Start();

            _threadsTimer = new Timer(ThreadsSamplingPeriod);
            _threadsTimer.Elapsed += ThreadsTimerOnElapsed;
            _threadsTimer.Start();

            _totalCpuCounter = new PerformanceCounter
            {
                CategoryName = "Processor",
                CounterName = "% Processor Time",
                InstanceName = "_Total",
                ReadOnly = true
            };
            _totalCpuTimer = new Timer(CpuSamplingPeriod);
            _totalCpuTimer.Elapsed += TotalCpuTimerOnElapsed;
            _totalCpuTimer.Start();
        }

        public string Serialize()
        {
            var builder = new StringBuilder();
            SerilalizeStatisticsHeader(builder);
            SerializeResponseCodeStatistics(builder);
            SerializeAuthCodeStatistics(builder);
            SerializeInternalErrorStatistics(builder);
            SerializeDetailedRequestPerformanceStatistics(builder);
            SerializeGeneralRequestPerformanceStatistics(builder);
            SerializeProcessMemoryUsageStatistics(builder);
            SerializeTotalCpuUsageStatistics(builder);
            SerializeThreadPoolUsageStatistics(builder);
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
                return;
            }
            _requestFailTimeCounters.AddOrUpdate(uri, processingTime, (key, value) => value + processingTime);
            _requestFailCounters.AddOrUpdate(uri, 1, (key, value) => value + 1);
            if (!IsInternalServerError(response))
                return;
                
            lock (_counterLock)
            {
                var code = response.StatusCode;
                if (!_internalErrorCounters.ContainsKey(uri))
                {
                    _internalErrorCounters[uri] = new ConcurrentDictionary<int, int>();
                    _internalErrorCounters[uri].TryAdd(code, 1);
                }
                else
                {
                    _internalErrorCounters[uri].AddOrUpdate(code, 1, (key, value) => value + 1);
                }
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
        #endregion

        #region private
        private static bool HasGoodStatusCode(HttpListenerResponse response) => response.StatusCode / 100 < 4;
        private static bool IsInternalServerError(HttpListenerResponse response) => response.StatusCode >= 500;

        private void ThreadsTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (_threadsLock)
            {
                int totalWorkerThreads;
                int totalIocpThreads;
                int availableWorkerThreads;
                int availableIocpThreads;
                Thread.MemoryBarrier();
                ThreadPool.GetMaxThreads(out totalWorkerThreads, out totalIocpThreads);
                ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableIocpThreads);
                Thread.MemoryBarrier();

                _currentWorkerThreads = totalWorkerThreads - availableWorkerThreads;
                _currentIocpThreads = totalIocpThreads - availableIocpThreads;

                _totalWorkerThreads += _currentWorkerThreads;
                _totalIocpThreads += _currentIocpThreads;

                _maxWorkerThreads = Math.Max(_maxWorkerThreads, _currentWorkerThreads);
                _maxIocpThreads = Math.Max(_maxIocpThreads, _currentIocpThreads);

                _threadingSamples++;
            }
        }

        private void ProcessMemoryTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (_processMemoryLock)
            {
                var currentMemory = GC.GetTotalMemory(false);
                _processMinMemory = Math.Min(_processMinMemory, currentMemory);
                _processMaxMemory = Math.Max(_processMaxMemory, currentMemory);
                _processCurrentMemory = currentMemory;
                _processSumMemory += currentMemory;

                _processMemorySamples++;

                if (MemoryWarningsEnabled)
                    ProcessPossibleMemoryOverhead();
            }
        }

        private void TotalCpuTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (_totalCpuLock)
            {
                try
                {
                    var currentCpuUsage = _totalCpuCounter.NextValue();
                    _totalMinCpuUsage = (int)Math.Min(_totalMinCpuUsage, currentCpuUsage);
                    _totalMaxCpuUsage = (int)Math.Max(_totalMaxCpuUsage, currentCpuUsage);
                    _totalCurrentCpuUsage = currentCpuUsage;
                    _totalSumCpuUsage += currentCpuUsage;

                    _totalCpuSamples++;
                }
                catch (Exception exception)
                {
                    Logger.LogException("SRV STAT", LogLevel.Warning, exception);
                }
            }
        }

        private void ProcessPossibleMemoryOverhead()
        {
            var memoryValueInMegabytes = (double) _processCurrentMemory/(1024*1024);
            if (memoryValueInMegabytes > _criticalMemoryValue)
            {
                _memoryOverheadOccured = true;
                _memoryOverheadValue = memoryValueInMegabytes;
                _memoryOverheadCount++;
            }

            _memoryIntervalSamplesLeft--;
            if (_memoryIntervalSamplesLeft > 0)
                return;

            if (_memoryOverheadOccured)
            {
                var builder = new StringBuilder();
                builder.AppendLine($"SERVER MEMORY OVERHEAD (Max {_criticalMemoryValue} MBs allowed)");
                builder.AppendLine($"Took {_memorySampleCount} samples during last {_memorySampleCount} seconds");
                var overheadSamplesPercentage = (double) _memoryOverheadCount*100/_memorySampleCount;
                builder.AppendLine($"Total {_memoryOverheadCount} overhead samples ({overheadSamplesPercentage:F1}%)");
                var overheadValuePercentage = _memoryOverheadValue * 100 / _criticalMemoryValue;
                builder.AppendLine($"Max memory value {_memoryOverheadValue:F1} MBs ({overheadValuePercentage:F1}%)");
                Logger.LogEntry("SERVER STAT", LogLevel.Warning, builder.ToString());
            }
            else
            {
                _counterProvider.Reset();
                _memoryOverheadOccured = false;
                _memoryOverheadCount = 0;
            }

            _memoryIntervalSamplesLeft = _counterProvider.GetNextCount();
            _memorySampleCount = _memoryIntervalSamplesLeft;
            _memoryOverheadCount = 0;
        }

        private void SerilalizeStatisticsHeader(StringBuilder builder)
        {
            builder.AppendLine($"Server started at {_startTime:s} ({DateTime.Now.Subtract(_startTime)} ago)");
            builder.AppendLine();
        }

        private void SerializeResponseCodeStatistics(StringBuilder builder)
        {
            builder.AppendLine("Response code statistics:");
            var keys = _responseCodes.Keys.ToList();
            keys.Sort();
            keys.ForEach(k => builder.AppendLine($"\t{k}: {_responseCodes[k]}"));
            builder.AppendLine();
        }

        private void SerializeAuthCodeStatistics(StringBuilder builder)
        {
            builder.AppendLine("Authcode statistics:");
            var authKeys = _authentificationResults.Keys.ToList();
            authKeys.Sort();
            authKeys.ForEach(k => builder.AppendLine($"\t{k,12}: {_authentificationResults[k]}"));
            builder.AppendLine();
        }

        private void SerializeDetailedRequestPerformanceStatistics(StringBuilder builder)
        {
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
                        averageSuccessTime = totalSuccessTime / _requestSuccessCounters[u];
                    }
                    else
                    {
                        averageSuccessTime = 0;
                    }

                    if (_requestFailCounters.ContainsKey(u))
                    {
                        averageFailTime = totalFailTime / _requestFailCounters[u];
                    }
                    else
                    {
                        averageFailTime = 0;
                    }

                    builder.AppendLine($"\t{u}");
                    builder.AppendLine($"\t\tAverage success time (2xx):      {averageSuccessTime,10:f} ms");
                    builder.AppendLine($"\t\tAverage fail    time (4xx, 5xx): {averageFailTime,10:f} ms");

                    var methodKeys = methodDict.Keys.ToList();
                    methodKeys.Sort();
                    methodKeys.ForEach(m => builder.AppendLine($"\t\t{m,-10}: {methodDict[m]}"));
                });
                builder.AppendLine();
            }
        }

        private void SerializeInternalErrorStatistics(StringBuilder builder)
        {
            if (_internalErrorCounters.IsEmpty)
                return;

            builder.AppendLine("Internal error statistics:");
            var uris = _internalErrorCounters.Keys.ToList();
            uris.Sort();
            lock (_counterLock)
            {
                uris.ForEach(u =>
                {
                    var codeDict = _internalErrorCounters[u];
                    builder.AppendLine($"\t{u}");
                    var codes = codeDict.Keys.ToList();
                    codes.Sort();
                    codes.ForEach(c => builder.AppendLine($"\t\t{c,-4}: {codeDict[c]}"));
                });
            }
            builder.AppendLine();
        }

        private void SerializeGeneralRequestPerformanceStatistics(StringBuilder builder)
        {
            if (_requestCount <= 0)
                return;

            builder.AppendLine("Request performance statistics:");
            builder.AppendLine($"\tRequest count:        {_requestCount,10}");
            builder.AppendLine($"\tAverage request time: {_totalRequestTime / _requestCount,10:f} ms");
            builder.AppendLine($"\tMax     request time: {_maxRequestTime,10:f} ms");
            builder.AppendLine($"\tMin     request time: {_minRequestTime,10:f} ms");
            builder.AppendLine($"\tTotal   request time: {_totalRequestTime,10:f} ms");
            builder.AppendLine($"\tSlowest request:      {_slowestRequest,10}");
        }

        private void SerializeProcessMemoryUsageStatistics(StringBuilder builder)
        {
            if (_processMemorySamples <= 0)
                return;

            lock (_processMemoryLock)
            {
                var averageMemory = _processSumMemory / _processMemorySamples;
                const int bytesInMegabyte = 1024 * 1024;
                builder.AppendLine();
                builder.AppendLine("Process memory statistics:");
                builder.AppendLine($"\tMin: {(double)_processMinMemory / bytesInMegabyte,8:F1} MB");
                builder.AppendLine($"\tMax: {(double)_processMaxMemory / bytesInMegabyte,8:F1} MB");
                builder.AppendLine($"\tAvg: {averageMemory / bytesInMegabyte,8:F1} MB");
                builder.AppendLine($"\tCur: {(double)_processCurrentMemory / bytesInMegabyte,8:F1} MB");
            }
        }

        private void SerializeThreadPoolUsageStatistics(StringBuilder builder)
        {
            if (_threadingSamples <= 0)
                return;

            lock (_threadsLock)
            {
                builder.AppendLine();
                builder.AppendLine("Thread statistics:");
                builder.AppendLine("\tWorker threads:");
                var avgWorkerThreads = (double)_totalWorkerThreads / _threadingSamples;
                builder.AppendLine($"\t\tMax: {_maxWorkerThreads,5}");
                builder.AppendLine($"\t\tAvg: {avgWorkerThreads,5:F1}");
                builder.AppendLine($"\t\tCur: {_currentWorkerThreads,5}");
                builder.AppendLine("\tIOCP threads:");
                var ageIocpThreads = (double)_totalIocpThreads / _threadingSamples;
                builder.AppendLine($"\t\tMax: {_maxIocpThreads,5}");
                builder.AppendLine($"\t\tAvg: {ageIocpThreads,5:F1}");
                builder.AppendLine($"\t\tCur: {_currentIocpThreads,5}");
            }
        }

        private void SerializeTotalCpuUsageStatistics(StringBuilder builder)
        {
            if (_totalCpuSamples <= 0)
                return;

            lock (_totalCpuLock)
            {
                var averageCpu = _totalSumCpuUsage / _totalCpuSamples;
                builder.AppendLine();
                builder.AppendLine($"Total CPU usage statistics ({Environment.ProcessorCount} cores):");
                builder.AppendLine($"\tMin: {(double)_totalMinCpuUsage,4:F1} %");
                builder.AppendLine($"\tMax: {(double)_totalMaxCpuUsage,4:F1} %");
                builder.AppendLine($"\tAvg: {averageCpu,4:F1} %");
                builder.AppendLine($"\tCur: {(double)_totalCurrentCpuUsage,4:F1} %");
            }
        }
        #endregion

        #region fields
        /// <summary>
        ///     Milliseconds
        /// </summary>
        private const double MemorySamplingPeriod = 1000;

        private const long StartMemoryWarningCount = 60;

        /// <summary>
        ///     Milliseconds
        /// </summary>
        private const double ThreadsSamplingPeriod = 1000;

        /// <summary>
        ///     Milliseconds
        /// </summary>
        private const double CpuSamplingPeriod = 1000;

        private readonly ConcurrentDictionary<int, int> _responseCodes = new ConcurrentDictionary<int, int>();

        private readonly object _requestMethodLock = new object();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _requestMethodCounters = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, int>> _internalErrorCounters = new ConcurrentDictionary<string, ConcurrentDictionary<int, int>>();

        private readonly ConcurrentDictionary<string, int> _requestSuccessCounters = new ConcurrentDictionary<string, int>();

        private readonly ConcurrentDictionary<string, int> _requestFailCounters = new ConcurrentDictionary<string, int>();

        private readonly ConcurrentDictionary<string, double> _requestSuccessTimeCounters = new ConcurrentDictionary<string, double>();

        private readonly ConcurrentDictionary<string, double> _requestFailTimeCounters = new ConcurrentDictionary<string, double>();

        private readonly ConcurrentDictionary<AuthorizationStatus, int> _authentificationResults = new ConcurrentDictionary<AuthorizationStatus, int>();

        private readonly DateTime _startTime = DateTime.Now;

        private readonly Timer _processMemoryTimer;

        private long _processMinMemory = long.MaxValue;
        private long _processMaxMemory;
        private long _processCurrentMemory;
        private double _processSumMemory;
        private long _processMemorySamples;

        private readonly int _criticalMemoryValue;
        private readonly IIntervalCounterProvider _counterProvider;
        private bool MemoryWarningsEnabled => _criticalMemoryValue > 0;
        private bool _memoryOverheadOccured;
        /// <summary>
        ///     MBs
        /// </summary>
        private double _memoryOverheadValue;

        private long _memorySampleCount;
        private long _memoryOverheadCount;

        private long _memoryIntervalSamplesLeft;

        private readonly Timer _threadsTimer;

        private int _maxWorkerThreads;
        private int _currentWorkerThreads;
        private long _totalWorkerThreads;
        private int _maxIocpThreads;
        private int _currentIocpThreads;
        private long _totalIocpThreads;
        private long _threadingSamples;

        private float _totalCurrentCpuUsage;
        private float _totalMinCpuUsage;
        private float _totalMaxCpuUsage;
        private double _totalSumCpuUsage;
        private long _totalCpuSamples;
        private readonly PerformanceCounter _totalCpuCounter;
        private readonly Timer _totalCpuTimer;

        private readonly object _threadsLock = new object();
        private readonly object _processMemoryLock = new object();
        private readonly object _totalCpuLock = new object();
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
        #endregion
    }
}
