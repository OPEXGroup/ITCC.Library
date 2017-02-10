// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
#if TRACE
    #define ITCC_LOG_REQUEST_BODIES
#endif

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ITCC.HTTP.Common.Enums;
using ITCC.HTTP.Server.Auth;
using ITCC.HTTP.Server.Common;
using ITCC.HTTP.Server.Enums;
using ITCC.HTTP.Server.Files;
using ITCC.HTTP.Server.Interfaces;
using ITCC.HTTP.Server.Service;
using ITCC.HTTP.Server.Utils;
using ITCC.HTTP.SslConfigUtil.Core;
using ITCC.HTTP.SslConfigUtil.Core.Enums;
using ITCC.Logging.Core;

// ReSharper disable StaticMemberInGenericType

namespace ITCC.HTTP.Server.Core
{
    /// <summary>
    ///     Represents static (and so, singleton HTTP(S) server
    /// </summary>
    public static class StaticServer<TAccount> where TAccount : class
    {
        #region main
        public static ServerStartStatus Start(HttpServerConfiguration<TAccount> configuration)
        {
            if (_started)
            {
                LogMessage(LogLevel.Warning, "Server is already running");
                return ServerStartStatus.AlreadyStarted;
            }
            lock (OperationLock)
            {
                if (_operationInProgress)
                {
                    LogMessage(LogLevel.Info, "Cannot start now, operation in progress");
                    return ServerStartStatus.AlreadyStarted;
                }
                _operationInProgress = true;
            }
            if (configuration == null || !configuration.IsEnough())
                return ServerStartStatus.BadParameters;

            try
            {
                PrepareStatistics(configuration);

                _optionsController = new OptionsController<TAccount>(InnerRequestProcessors);
                _pingController = new PingController();

                Protocol = configuration.Protocol;
                _authorizer = configuration.Authorizer;
                _authentificationController = new AuthentificationController(configuration.Authentificator);

                ConfigureResponseBuilding(configuration);

                if (configuration.FilesEnabled)
                {
                    if (!StartFileProcessing(configuration))
                        return ServerStartStatus.BadParameters;
                }

                _requestMaxServeTime = configuration.RequestMaxServeTime;

                if (Protocol == Protocol.Https)
                {
                    if (!BindCertificate(configuration))
                    {
                        LogMessage(LogLevel.Warning, "Binding failed");
                        return ServerStartStatus.BindingError;
                    }
                }

                StartListenerThread(configuration);
                StartServices(configuration);
                _started = true;

                return ServerStartStatus.Ok;
            }
            catch (SocketException)
            {
                LogMessage(LogLevel.Critical, $"Error binding to port {configuration.Port}. Is it in use?");
                _fileRequestController.Stop();
                return ServerStartStatus.BindingError;
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Critical, ex);
                _fileRequestController.Stop();
                return ServerStartStatus.UnknownError;
            }
            finally
            {
                lock (OperationLock)
                {
                    _operationInProgress = false;
                }
            }
        }

        private static void PrepareStatistics(HttpServerConfiguration<TAccount> configuration)
        {
            _statisticsController = configuration.StatisticsEnabled
                ? new StatisticsController<TAccount>(new ServerStatistics<TAccount>(
                        configuration.CriticalMemoryValue,
                        configuration.MemoryAlarmStrategy),
                    configuration.StatisticsAuthorizer)
                : new StatisticsController<TAccount>(null, null);
        }

        private static void ConfigureResponseBuilding(HttpServerConfiguration<TAccount> configuration)
        {
            if (configuration.LogBodyReplacePatterns != null)
                ResponseFactory.LogBodyReplacePatterns.AddRange(configuration.LogBodyReplacePatterns);
            if (configuration.LogProhibitedHeaders != null)
                ResponseFactory.LogProhibitedHeaders.AddRange(configuration.LogProhibitedHeaders);

            ResponseFactory.NonSerializableTypes = configuration.NonSerializableTypes;
            ResponseFactory.SetBodyEncoders(configuration.BodyEncoders);
            ResponseFactory.LogResponseBodies = configuration.LogResponseBodies;
            ResponseFactory.ResponseBodyLogLimit = configuration.ResponseBodyLogLimit;
            CommonHelper.SetSerializationLimitations(configuration.RequestBodyLogLimit,
                configuration.LogProhibitedQueryParams,
                configuration.LogProhibitedHeaders,
                configuration.BodyEncoders.First(be => be.IsDefault).Encoding);

            if (configuration.ServerName != null)
            {
                ResponseFactory.SetCommonHeaders(new Dictionary<string, string> {{"Server", configuration.ServerName}});
            }
        }

        private static bool StartFileProcessing(HttpServerConfiguration<TAccount> configuration)
        {
            _fileRequestController = new FileRequestController<TAccount>();

            return _fileRequestController.Start(new FileRequestControllerConfiguration<TAccount>
            {
                FilesEnabled = configuration.FilesEnabled,
                FilesBaseUri = configuration.FilesBaseUri,
                ExistingFilesPreprocessingFrequency = configuration.ExistingFilesPreprocessingFrequency,
                FaviconPath = configuration.FaviconPath,
                FilesAuthorizer = configuration.FilesAuthorizer,
                FileSections = configuration.FileSections,
                FilesLocation = configuration.FilesLocation,
                FilesNeedAuthorization = configuration.FilesNeedAuthorization,
                FilesPreprocessingEnabled = configuration.FilesPreprocessingEnabled,
                FilesCompressionEnabled = configuration.FilesCompressionEnabled,
                FilesPreprocessorThreads = configuration.FilesPreprocessorThreads
            },
            _statisticsController.Statistics);
        }

        private static void StartListenerThread(HttpServerConfiguration<TAccount> configuration)
        {
            var protocolString = configuration.Protocol == Protocol.Http ? "http" : "https";
            ServicePointManager.DefaultConnectionLimit = 1000;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"{protocolString}://+:{configuration.Port}/");
            _listener.IgnoreWriteExceptions = true;
            _listener.Start();
            _listenerThread = new Thread(() =>
            {
                LogMessage(LogLevel.Info, "Server listener thread started");
                LogMessage(LogLevel.Info, $"Started listening port {configuration.Port}");
                while (true)
                {
                    try
                    {
                        var context = _listener.GetContext();
                        LogDebug($"Client connected: {context.Request.RemoteEndPoint}");
                        Task.Run(async () => await OnMessageAsync(context));
                    }
                    catch (ThreadAbortException)
                    {
                        LogMessage(LogLevel.Info, "Server listener thread stopped");
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogException(LogLevel.Error, ex);
                    }
                }
            });

            _listenerThread.Start();
            _serverAddress = $"{protocolString}://{configuration.SubjectName}:{configuration.Port}/";
        }

        private static void StartServices(HttpServerConfiguration<TAccount> configuration)
        {
            ServiceUris.AddRange(configuration.GetReservedUris());
            if (ServiceUris.Any())
            {
                LogDebug($"Reserved sections:\n{string.Join("\n", ServiceUris)}");
            }

            ServiceRequestProcessors.Add(_optionsController);
            if (configuration.FilesEnabled)
                ServiceRequestProcessors.Add(_fileRequestController);
            if (configuration.StatisticsEnabled)
                ServiceRequestProcessors.Add(_statisticsController);
            ServiceRequestProcessors.Add(_pingController);
            ServiceRequestProcessors.Add(_authentificationController);
        }

        public static void Stop()
        {
            if (!_started)
                return;
            if (FilesEnabled)
                _fileRequestController.Stop();
            lock (OperationLock)
            {
                if (_operationInProgress)
                {
                    LogMessage(LogLevel.Info, "Cannot stop now, operation in progress");
                    return;
                }
                _operationInProgress = true;
            }

            LogMessage(LogLevel.Info, "Shutting down");
            try
            {
                _listenerThread.Abort();
                _listener.Stop();
                _started = false;
                CleanUp();
                LogMessage(LogLevel.Info, "Stopped");
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
            }
            finally
            {
                lock (OperationLock)
                {
                    _operationInProgress = false;
                }
            }
        }

        private static void CleanUp()
        {
            InnerRequestProcessors.Clear();
            InnerStaticRedirectionTable.Clear();
            _listener = null;
            _fileRequestController.Dispose();
            _statisticsController.Dispose();
            ServiceUris.Clear();
        }

        private static string GetNormalizedRequestUri(HttpListenerContext context) => "/" + context.Request.Url.LocalPath.Trim('/');

        private static bool _started;
        private static Thread _listenerThread;
        private static HttpListener _listener;
        private static bool _operationInProgress;
        private static readonly object OperationLock = new object();
        private static string _serverAddress;

        #endregion

        #region security

        private static bool BindCertificate(HttpServerConfiguration<TAccount> config)
        {
            AssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Port = config.Port.ToString();
            IpAddress = "0.0.0.0";

            BindingParams bindingParams;
            switch (config.CertificateBindType)
            {
                case BindType.CertificateThumbprint:
                    bindingParams = new CertificateThumbprintBindingParams(config.CertificateThumbprint);
                    break;
                case BindType.SubjectName:
                    bindingParams = new CertificateSubjectnameParams(config.SubjectName, config.AllowGeneratedCertificates);
                    break;
                case BindType.FromFile:
                    bindingParams = new CertificateFileBindingParams(config.CertificateFilename, null);
                    break;
                default:
                    return false;
            }

            var result = Binder.Bind(AssemblyPath, IpAddress, Port, config.CertificateBindType, bindingParams);
            if (result.Status != BindingStatus.Ok)
            {
                LogMessage(LogLevel.Warning, result.Reason ?? "Binding failed");
                return false;
            }
            return true;
        }

        public static Protocol Protocol { get; private set; }

        private static string IpAddress { get; set; }
        private static string Port { get; set; }
        private static string AssemblyPath { get; set; }

        #endregion

        #region log

        [Conditional("DEBUG")]
        private static void LogDebug(string message) => Logger.LogDebug("HTTP SERVER", message);

        private static void LogMessage(LogLevel level, string message) => Logger.LogEntry("HTTP SERVER", level, message);

        private static void LogException(LogLevel level, Exception ex) => Logger.LogException("HTTP SERVER", level, ex);

        #endregion

        #region handlers

        private static async Task OnMessageAsync(HttpListenerContext context)
        {
            var request = context.Request;
            // This Stopwatch will be used by different threads, but sequentially (select processor => handle => completion)
            var stopWatch = Stopwatch.StartNew();
            try
            {
                _statisticsController.Statistics?.AddRequest(request);
                var isBinaryRequest = _fileRequestController != null && _fileRequestController.RequestIsSuitable(request);
                LogDebug($"Request from {request.RemoteEndPoint}.\n{CommonHelper.SerializeHttpRequest(context, false, ! isBinaryRequest)}");

                var serviceProcessor =
                    ServiceRequestProcessors.FirstOrDefault(sp => sp.RequestIsSuitable(context.Request));
                if (serviceProcessor != null)
                {
                    LogDebug($"Service {serviceProcessor.Name} requested");
                    await serviceProcessor.HandleRequestAsync(context);
                    OnResponseReady(context, stopWatch);
                    return;
                }

                var requestProcessorSelectionResult = SelectRequestProcessor(request);
                if (requestProcessorSelectionResult == null)
                {
                    ResponseFactory.BuildResponse(context, HttpStatusCode.NotFound, null);
                    OnResponseReady(context, stopWatch);
                    return;
                }

                if (requestProcessorSelectionResult.IsRedirect)
                {
                    var redirectLocation = $"{_serverAddress}{requestProcessorSelectionResult.RequestProcessor.SubUri}";
                    context.Response.Redirect(redirectLocation);
                    OnResponseReady(context, stopWatch);
                    return;
                }

                if (!requestProcessorSelectionResult.MethodMatches)
                {
                    ResponseFactory.BuildResponse(context, HttpStatusCode.MethodNotAllowed, null);
                    OnResponseReady(context, stopWatch);
                    return;
                }

                var requestProcessor = requestProcessorSelectionResult.RequestProcessor;

                AuthorizationResult<TAccount> authResult;
                if (requestProcessor.AuthorizationRequired && _authorizer != null)
                {
                    authResult = await _authorizer.Invoke(request, requestProcessor);
                }
                else
                {
                    authResult = new AuthorizationResult<TAccount>(null, AuthorizationStatus.NotRequired);
                }

                _statisticsController.Statistics?.AddAuthResult(authResult);
                switch (authResult.Status)
                {
                    case AuthorizationStatus.NotRequired:
                    case AuthorizationStatus.Ok:
                        if (requestProcessor.Handler == null)
                        {
                            LogDebug($"{request.HttpMethod} {request.Url.LocalPath} was requested, but no handler is provided");
                            ResponseFactory.BuildResponse(context, HttpStatusCode.NotImplemented, null);
                        }
                        else
                        {
                            var handleResult =
                                await requestProcessor.Handler.Invoke(authResult.Account, request).ConfigureAwait(false);
                            ResponseFactory.BuildResponse(context, handleResult);
                        }
                        break;
                    default:
                        ResponseFactory.BuildResponse(context, authResult);
                        break;
                }
                OnResponseReady(context, stopWatch);
            }
            catch (Exception exception)
            {
                LogMessage(LogLevel.Warning, $"Error handling client request from {request.RemoteEndPoint}");
                LogException(LogLevel.Warning, exception);
                ResponseFactory.BuildResponse(context, HttpStatusCode.InternalServerError, null);
                OnResponseReady(context, stopWatch);
            }
        }

        private static void OnResponseReady(HttpListenerContext context, Stopwatch requestStopwatch)
        {
            try
            {
                LogDebug($"Response for {context.Request.RemoteEndPoint} ready.");
                requestStopwatch.Stop();
                var elapsedMilliseconds = requestStopwatch.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
                _statisticsController.Statistics?.AddResponse(context.Response, GetNormalizedRequestUri(context), elapsedMilliseconds);
                if (_requestMaxServeTime > 0 && _requestMaxServeTime < elapsedMilliseconds)
                {
                    LogMessage(LogLevel.Warning, $"Request /{GetNormalizedRequestUri(context)} from {context.Request.RemoteEndPoint} took {elapsedMilliseconds} milliseconds to process!");
                }
                context.Response.Close();
                LogDebug($"Response for {context.Request.RemoteEndPoint} queued ({elapsedMilliseconds} milliseconds)");
            }
            catch (HttpListenerException)
            {
                LogMessage(LogLevel.Info, "Sudden client disconnect, failed to send response");
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
            }
        }

        private static double _requestMaxServeTime;
        #endregion

        #region service

        private static FileRequestController<TAccount> _fileRequestController;
        private static OptionsController<TAccount> _optionsController;
        private static PingController _pingController;
        private static AuthentificationController _authentificationController;
        private static StatisticsController<TAccount> _statisticsController;

        private static readonly List<IServiceController> ServiceRequestProcessors = new List<IServiceController>();

        private static readonly List<string> ServiceUris = new List<string>();

        #endregion

        #region statistics

        public static bool StatisticsEnabled => _statisticsController.StatisticsEnabled;

        public static string GetStatistics() => _statisticsController.Statistics?.Serialize();

        #endregion

        #region files

        public static bool FilesEnabled => _fileRequestController.FilesEnabled;

        public static string FilesLocation => _fileRequestController.FilesLocation;

        public static string FilesBaseUri => _fileRequestController.FilesBaseUri;

        public static bool FilesNeedAuthorization => _fileRequestController.FilesNeedAuthorization;

        public static List<FileSection> FileSections => _fileRequestController.FileSections;

        public static bool FileExists(string sectionName, string filename)
            => FilesEnabled && _fileRequestController.FileExists(sectionName, filename);

        public static Stream GetFileStream(string sectionName, string filename)
            => !FilesEnabled ? null : _fileRequestController.GetFileStream(sectionName, filename);

        public static async Task<string> GetFileStringAsync(string sectionName, string filename)
        {
            if (!FilesEnabled)
                return null;
            return await _fileRequestController.GetFileStringAsync(sectionName, filename);
        }

        public static async Task<FileOperationStatus> AddFileAsync(string sectionName, string filename, Stream content)
        {
            if (!FilesEnabled)
                return FileOperationStatus.FilesNotEnabled;
            return await _fileRequestController.AddFileAsync(sectionName, filename, content);
        }

        public static FileOperationStatus DeleteFile(string sectionName, string filename)
            => !FilesEnabled ? FileOperationStatus.FilesNotEnabled : _fileRequestController.DeleteFile(sectionName, filename);

        #endregion

        #region requests

        private static bool RequestProcessorDuplicates(RequestProcessor<TAccount> first,
            RequestProcessor<TAccount> second)
        {
            if (first == null || second == null)
                return false;

            return first.SubUri == second.SubUri && first.Method == second.Method;
        }

        public static bool AddRequestProcessor(RequestProcessor<TAccount> requestProcessor)
        {
            if (CheckRequestProcessor(requestProcessor) != true)
                return false;

            requestProcessor.SubUri = requestProcessor.SubUri.Trim('/');
            var duplicate = InnerRequestProcessors.FirstOrDefault(rp => RequestProcessorDuplicates(rp, requestProcessor));
            if (duplicate != null)
                InnerRequestProcessors.Remove(duplicate);

            InnerRequestProcessors.Add(requestProcessor);
            return true;
        }

        public static bool AddRequestProcessorRange(
            IEnumerable<RequestProcessor<TAccount>> requestProcessors)
            => requestProcessors != null && requestProcessors.All(AddRequestProcessor);

        public static bool AddStaticRedirect(string fromUri, string toUri)
        {
            var preprocessed = PreprocessRedirect(fromUri, toUri);
            if (!preprocessed.Item3)
                return false;
            return InnerStaticRedirectionTable.TryAdd(preprocessed.Item1, preprocessed.Item2);
        }

        public static bool AddStaticRedirectRange(IDictionary<string, string> uriTable)
        {
            return uriTable.All(up => AddStaticRedirect(up.Key, up.Value));
        }

        private static bool CheckRequestProcessor(RequestProcessor<TAccount> requestProcessor)
        {
            if (requestProcessor.SubUri == null)
                return false;

            if (requestProcessor.Method == HttpMethod.Options || requestProcessor.Method == HttpMethod.Head)
            {
                LogMessage(LogLevel.Warning, $"Cannot explicitely register {requestProcessor.Method.Method.ToUpperInvariant()} processor");
                return false;
            }

            var serviceDuplicate = ServiceUris.FirstOrDefault(su => requestProcessor.SubUri.Trim('/').StartsWith(su));
            if (serviceDuplicate != null)
            {
                LogMessage(LogLevel.Warning, $"Failed to register uri '{requestProcessor.SubUri}'. Section '{serviceDuplicate}' is reserved.");
                return false;
            }

            return true;
        }

        private static Tuple<string, string, bool> PreprocessRedirect(string fromUri, string toUri)
        {
            if (fromUri == null || toUri == null)
                return new Tuple<string, string, bool>(null, null, false);

            if (InnerStaticRedirectionTable.ContainsKey(fromUri))
                return new Tuple<string, string, bool>(null, null, false);

            return new Tuple<string, string, bool>(fromUri.Trim('/'), toUri.Trim('/'), true);
        }

        private static RequestProcessorSelectionResult<TAccount> SelectRequestProcessor(HttpListenerRequest request)
        {
            var requestMethod = CommonHelper.HttpMethodToEnum(request.HttpMethod);
            var localUri = request.Url.LocalPath.Trim('/');
            if (requestMethod == HttpMethod.Get || requestMethod == HttpMethod.Head)
            {
                string redirectionTarget;
                if (InnerStaticRedirectionTable.TryGetValue(localUri, out redirectionTarget))
                {
                    var correspondedProcessor =
                        InnerRequestProcessors.FirstOrDefault(
                            rp => rp.Method == HttpMethod.Get && rp.SubUri.Trim('/') == redirectionTarget);
                    if (correspondedProcessor != null)
                    {
                        return new RequestProcessorSelectionResult<TAccount>
                        {
                            RequestProcessor = correspondedProcessor,
                            MethodMatches = true,
                            IsRedirect = true
                        };
                    }
                }
            }

            var processorsForUri = InnerRequestProcessors.Where(requestProcessor => CommonHelper.UriMatchesString(request.Url, requestProcessor.SubUri)).ToList();
            if (processorsForUri.Count == 0)
                return null;
            var suitableProcessor = processorsForUri.FirstOrDefault(rp => rp.Method == requestMethod || requestMethod == HttpMethod.Head && rp.Method == HttpMethod.Get);
            return new RequestProcessorSelectionResult<TAccount>
            {
                RequestProcessor = suitableProcessor,
                MethodMatches = suitableProcessor != null,
                IsRedirect = false
            };
        }

        private static Delegates.Authorizer<TAccount> _authorizer;

        public static Dictionary<string, string> StaticRedirectionTable => new Dictionary<string, string>(InnerStaticRedirectionTable);
        public static List<RequestProcessor<TAccount>> RequestProcessors => new List<RequestProcessor<TAccount>>(InnerRequestProcessors);

        private static readonly List<RequestProcessor<TAccount>> InnerRequestProcessors =
            new List<RequestProcessor<TAccount>>();
        private static readonly ConcurrentDictionary<string, string> InnerStaticRedirectionTable = new ConcurrentDictionary<string, string>();

        #endregion
    }
}