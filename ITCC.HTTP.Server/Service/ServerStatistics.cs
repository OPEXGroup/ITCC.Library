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
            _memoryTimer.Stop();
            _threadsTimer.Stop();
            _memoryTimer.Dispose();
            _threadsTimer.Dispose();
            _cpuTimer.Stop();
            _cpuTimer.Dispose();
            _cpuCounter.Dispose();
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

            _memoryTimer = new Timer(MemorySamplingPeriod);
            _memoryTimer.Elapsed += MemoryTimerOnElapsed;
            _memoryTimer.Start();

            _threadsTimer = new Timer(ThreadsSamplingPeriod);
            _threadsTimer.Elapsed += ThreadsTimerOnElapsed;
            _threadsTimer.Start();

            _cpuCounter = new PerformanceCounter
            {
                CategoryName = "Processor",
                CounterName = "% Processor Time",
                InstanceName = GetCurrentProcessInstanceName(),
                ReadOnly = true
            };
            _cpuCounter.InstanceName = "_Total";
            _cpuTimer = new Timer(CpuSamplingPeriod);
            _cpuTimer.Elapsed += CpuTimerOnElapsed;
            _cpuTimer.Start();
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
            SerializeMemoryUsageStatistics(builder);
            SerializeCpuUsageStatistics(builder);
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
                _threadingSamples++;
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

        private void MemoryTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (_memoryLock)
            {
                var currentMemory = GC.GetTotalMemory(false);
                _minMemory = Math.Min(_minMemory, currentMemory);
                _maxMemory = Math.Max(_maxMemory, currentMemory);
                _currentMemory = currentMemory;
                _totalMemory += currentMemory;

                _memorySamples++;

                if (MemoryWarningsEnabled)
                    ProcessPossibleMemoryOverhead();
            }
        }

        private void CpuTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (_cpuLock)
            {
                try
                {
                    var currentCpuUsage = _cpuCounter.NextValue();
                    _minCpuUsage = (int)Math.Min(_minCpuUsage, currentCpuUsage);
                    _maxCpuUsage = (int)Math.Max(_maxCpuUsage, currentCpuUsage);
                    _currentCpu = currentCpuUsage;
                    _totalCpuUsage += currentCpuUsage;

                    _cpuSamples++;
                }
                catch (Exception exception)
                {
                    Logger.LogException("SRV STAT", LogLevel.Warning, exception);
                }
            }
        }

        private void ProcessPossibleMemoryOverhead()
        {
            var memoryValueInMegabytes = (double) _currentMemory/(1024*1024);
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
            if (!_internalErrorCounters.IsEmpty)
            {
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
        }

        private void SerializeGeneralRequestPerformanceStatistics(StringBuilder builder)
        {
            if (_requestCount > 0)
            {
                builder.AppendLine("Request performance statistics:");
                builder.AppendLine($"\tRequest count:        {_requestCount,10}");
                builder.AppendLine($"\tAverage request time: {_totalRequestTime / _requestCount,10:f} ms");
                builder.AppendLine($"\tMax     request time: {_maxRequestTime,10:f} ms");
                builder.AppendLine($"\tMin     request time: {_minRequestTime,10:f} ms");
                builder.AppendLine($"\tTotal   request time: {_totalRequestTime,10:f} ms");
                builder.AppendLine($"\tSlowest request:      {_slowestRequest,10}");
            }
        }

        private void SerializeMemoryUsageStatistics(StringBuilder builder)
        {
            if (_memorySamples > 0)
            {
                lock (_memoryLock)
                {
                    var averageMemory = _totalMemory / _memorySamples;
                    const int bytesInMegabyte = 1024 * 1024;
                    builder.AppendLine();
                    builder.AppendLine("Memory statistics:");
                    builder.AppendLine($"\tMin: {(double)_minMemory / bytesInMegabyte,8:F1} MB");
                    builder.AppendLine($"\tMax: {(double)_maxMemory / bytesInMegabyte,8:F1} MB");
                    builder.AppendLine($"\tAvg: {averageMemory / bytesInMegabyte,8:F1} MB");
                    builder.AppendLine($"\tCur: {(double)_currentMemory / bytesInMegabyte,8:F1} MB");
                }
            }
        }

        private void SerializeThreadPoolUsageStatistics(StringBuilder builder)
        {
            if (_threadingSamples > 0)
            {
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
        }

        private void SerializeCpuUsageStatistics(StringBuilder builder)
        {
            if (_cpuSamples > 0)
            {
                lock (_cpuLock)
                {
                    var averageCpu = _totalCpuUsage / _cpuSamples;
                    builder.AppendLine();
                    builder.AppendLine($"Cpu usage statistics (Total {Environment.ProcessorCount} cores):");
                    builder.AppendLine($"\tMin: {(double)_minCpuUsage,4:F1} %");
                    builder.AppendLine($"\tMax: {(double)_maxCpuUsage,4:F1} %");
                    builder.AppendLine($"\tAvg: {averageCpu,4:F1} %");
                    builder.AppendLine($"\tCur: {(double)_currentCpu,4:F1} %");
                }
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

        private readonly Timer _memoryTimer;

        private long _minMemory = long.MaxValue;
        private long _maxMemory;
        private long _currentMemory;
        private double _totalMemory;
        private long _memorySamples;

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

        private float _currentCpu;
        private float _minCpuUsage;
        private float _maxCpuUsage;
        private double _totalCpuUsage;
        private long _cpuSamples;

        private readonly PerformanceCounter _cpuCounter;

        private readonly Timer _cpuTimer;

        private readonly object _threadsLock = new object();
        private readonly object _memoryLock = new object();
        private readonly object _cpuLock = new object();
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

        #region process utils

        private static string GetCurrentProcessInstanceName()
        {
            var proc = Process.GetCurrentProcess();
            var pid = proc.Id;
            return GetProcessInstanceName(pid);
        }

        private static string GetProcessInstanceName(int pid)
        {
            var counterCategory = new PerformanceCounterCategory("Process");

            var instances = counterCategory.GetInstanceNames();
            foreach (var instance in instances)
            {
                using (var cnt = new PerformanceCounter("Process",
                     "ID Process", instance, true))
                {
                    var val = (int)cnt.RawValue;
                    if (val == pid)
                    {
                        return instance;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
