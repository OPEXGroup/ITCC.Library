using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ITCC.HTTP.Common;
using ITCC.HTTP.Enums;
using ITCC.HTTP.Server.Files;
using ITCC.HTTP.Utils;
using ITCC.Logging.Core;
using Newtonsoft.Json;
// ReSharper disable StaticMemberInGenericType

namespace ITCC.HTTP.Server
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

            if (configuration.StatisticsEnabled)
            {
                StatisticsEnabled = true;
                _statistics = new ServerStatistics<TAccount>();
                _statisticsAuthorizer = configuration.StatisticsAuthorizer;
            }
            else
            {
                StatisticsEnabled = false;
                _statistics = null;
                _statisticsAuthorizer = null;
            }



            try
            {
                Protocol = configuration.Protocol;
                _authorizer = configuration.Authorizer;
                _authentificator = configuration.Authentificator;

                if (configuration.LogBodyReplacePatterns != null)
                    ResponseFactory.LogBodyReplacePatterns.AddRange(configuration.LogBodyReplacePatterns);
                if (configuration.LogProhibitedHeaders != null)
                    ResponseFactory.LogProhibitedHeaders.AddRange(configuration.LogProhibitedHeaders);
                _logProhibitedQueryParams = configuration.LogProhibitedQueryParams ?? new List<string>();

                ResponseFactory.SetBodyEncoder(configuration.BodyEncoder);
                _requestEncoding = configuration.BodyEncoder.Encoding;
                _autoGzipCompression = configuration.BodyEncoder.AutoGzipCompression;
                ResponseFactory.LogResponseBodies = configuration.LogResponseBodies;
                ResponseFactory.ResponseBodyLogLimit = configuration.ResponseBodyLogLimit;

                FilesEnabled = configuration.FilesEnabled;
                FilesBaseUri = configuration.FilesBaseUri;

                if (configuration.FilesEnabled)
                {
                    FilesEnabled = true;
                    var fileControllerStartSucceeded = FileRequestController<TAccount>.Start(new FileRequestControllerConfiguration<TAccount>
                    {
                        ExistingFilesPreprocessingFrequency = configuration.ExistingFilesPreprocessingFrequency,
                        FaviconPath = configuration.FaviconPath,
                        FilesAuthorizer = configuration.FilesAuthorizer,
                        FileSections = configuration.FileSections,
                        FilesLocation = configuration.FilesLocation,
                        FilesNeedAuthorization = configuration.FilesNeedAuthorization,
                        FilesPreprocessingEnabled = configuration.FilesPreprocessingEnabled,
                        FilesPreprocessorThreads = configuration.FilesPreprocessorThreads
                    }, _statistics);
                    if (!fileControllerStartSucceeded)
                        return ServerStartStatus.BadParameters;
                }

                _requestMaxServeTime = configuration.RequestMaxServeTime;

                if (configuration.ServerName != null)
                {
                    ResponseFactory.SetCommonHeaders(new Dictionary<string, string>
                    {
                        {"Server", configuration.ServerName}
                    });
                }
                var protocolString = configuration.Protocol == Protocol.Http ? "http" : "https";
                ServicePointManager.DefaultConnectionLimit = 1000;
                _listener = new HttpListener();
                _listener.Prefixes.Add($"{protocolString}://+:{configuration.Port}/");
                _listener.Start();
                _listenerThread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var context = _listener.GetContext();
                            LogMessage(LogLevel.Debug, $"Client connected: {context.Request.RemoteEndPoint}");
                            Task.Run(async () => await OnMessage(context));
                        }
                        catch (ThreadAbortException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
                        }
                    }
                });

                _listenerThread.Start();
                _started = true;
                _serverAddress = $"{protocolString}://{configuration.SubjectName}:{configuration.Port}/";
                ServiceUris.AddRange(configuration.GetReservedUris());
                LogMessage(LogLevel.Info, $"Started listening port {configuration.Port}");
                if (ServiceUris.Any())
                    LogMessage(LogLevel.Debug, $"Reserved sections:\n{string.Join("\n", ServiceUris)}");

                return ServerStartStatus.Ok;
            }
            catch (SocketException)
            {
                LogMessage(LogLevel.Critical, $"Error binding to port {configuration.Port}. Is it in use?");
                return ServerStartStatus.BindingError;
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Critical, ex);
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

        public static void Stop()
        {
            if (!_started)
                return;
            if (FilesEnabled)
                FileRequestController<TAccount>.Stop();
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
            _listener = null;
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

        public static Protocol Protocol { get; private set; }

        #endregion

        #region log

        private static void LogMessage(LogLevel level, string message)
        {
            Logger.LogEntry("HTTP SERVER", level, message);
        }

        private static void LogException(LogLevel level, Exception ex)
        {
            Logger.LogException("HTTP SERVER", level, ex);
        }

        #endregion

        #region handlers

        private static async Task OnMessage(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            // This Stopwatch will be used by different threads, but sequentially (select processor => handle => completion)
            var stopWatch = Stopwatch.StartNew();
            try
            {
                _statistics?.AddRequest(request);
                if (Logger.Level >= LogLevel.Debug)
                    LogMessage(LogLevel.Debug,
                        $"Request from {request.RemoteEndPoint}.\n{SerializeHttpRequest(request)}");

                foreach (var checker in ServiceHandlers.Keys)
                {
                    if (checker.Invoke(request))
                    {
                        await ServiceHandlers[checker].Invoke(context, stopWatch).ConfigureAwait(false);
                        return;
                    }
                }

                var requestProcessorSelectionResult = SelectRequestProcessor(request);
                if (requestProcessorSelectionResult == null)
                {
                    ResponseFactory.BuildResponse(context.Response, HttpStatusCode.NotFound, null);
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
                    ResponseFactory.BuildResponse(response, HttpStatusCode.MethodNotAllowed, null);
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

                _statistics?.AddAuthResult(authResult);
                switch (authResult.Status)
                {
                    case AuthorizationStatus.NotRequired:
                    case AuthorizationStatus.Ok:
                        if (requestProcessor.Handler == null)
                        {
                            LogMessage(LogLevel.Debug,
                                $"{request.HttpMethod} {request.Url.LocalPath} was requested, but no handler is provided");
                            ResponseFactory.BuildResponse(response, HttpStatusCode.NotImplemented, null);
                        }
                        else
                        {
                            var handleResult =
                                await requestProcessor.Handler.Invoke(authResult.Account, request).ConfigureAwait(false);
                            ResponseFactory.BuildResponse(response, handleResult, false, RequestEnablesGzip(request));
                            //if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Head)
                            //{
                            //    var savedBody = response.Body;
                            //    response.Body = null;
                            //    response.ContentLength = Convert.ToInt32(savedBody.Length);
                            //}
                        }
                        break;
                    default:
                        ResponseFactory.BuildResponse(response, authResult, RequestEnablesGzip(request));
                        break;
                }
                OnResponseReady(context, stopWatch);
            }
            catch (Exception exception)
            {
                LogMessage(LogLevel.Warning, $"Error handling client request from {request.RemoteEndPoint}");
                LogException(LogLevel.Warning, exception);
                ResponseFactory.BuildResponse(response, HttpStatusCode.InternalServerError, null);
                OnResponseReady(context, stopWatch);
            }
        }

        private static void OnResponseReady(HttpListenerContext context, Stopwatch requestStopwatch)
        {
            try
            {
                LogMessage(LogLevel.Debug, $"Response for {context.Request.RemoteEndPoint} ready.");
                requestStopwatch.Stop();
                var elapsedMilliseconds = requestStopwatch.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
                _statistics?.AddResponse(context.Response, GetNormalizedRequestUri(context), elapsedMilliseconds);
                if (_requestMaxServeTime > 0 && _requestMaxServeTime < elapsedMilliseconds)
                {
                    LogMessage(LogLevel.Warning, $"Request /{GetNormalizedRequestUri(context)} from {context.Request.RemoteEndPoint} took {elapsedMilliseconds} milliseconds to process!");
                }
                context.Response.Close();
                LogMessage(LogLevel.Debug, $"Response for {context.Request.RemoteEndPoint} queued ({elapsedMilliseconds} milliseconds)");
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

        private static readonly Dictionary<Delegates.ServiceRequestChecker, Delegates.ServiceRequestHandler> ServiceHandlers = new Dictionary<Delegates.ServiceRequestChecker, Delegates.ServiceRequestHandler>
        {
            { IsOptionsRequest, HandleOptions },
            { IsLoginRequest, Authentificate },
            { IsPingRequest, HandlePing },
            { IsStatisticsRequest, HandleStatistics },
            { IsFilesRequest, HandleFileRequest },
            { IsFaviconRequest, HandleFavicon }
        };

        private static readonly List<string> ServiceUris = new List<string>();

        #endregion

        #region auth

        private static bool IsLoginRequest(HttpListenerRequest request)
        {
            if (request == null || _authentificator == null)
            {
                return false;
            }
            if (request.Url.LocalPath.Trim('/').StartsWith("login"))
            {
                return true;
            }
            return request.QueryString["login"] != null && request.QueryString["password"] != null;
        }

        private static async Task Authentificate(HttpListenerContext context, Stopwatch requestStopwatch)
        {
            AuthentificationResult authResult;
            var request = context.Request;
            if (_authentificator != null)
                authResult = await _authentificator.Invoke(request);
            else
                authResult = new AuthentificationResult(null, HttpStatusCode.NotFound);
            if (authResult == null)
                throw new InvalidOperationException("Authentificator fault: null result");
            ResponseFactory.BuildResponse(context.Response, authResult, RequestEnablesGzip(request));
            OnResponseReady(context, requestStopwatch);
        }

        private static Delegates.Authentificator _authentificator;

        private static Delegates.Authorizer<TAccount> _authorizer;

        #endregion

        #region ping
        private static bool IsPingRequest(HttpListenerRequest request)
        {
            if (request == null)
            {
                return false;
            }
            if (request.QueryString["ping"] != null)
            {
                return true;
            }
            return request.Url.LocalPath.Trim('/') == "ping";
        }

        private static Task HandlePing(HttpListenerContext context, Stopwatch requestStopwatch)
        {
            var converter = new PingJsonConverter();
            var responseBody = JsonConvert.SerializeObject(new PingResponse(SerializeHttpRequest(context.Request, true)), Formatting.None, converter);
            ResponseFactory.BuildResponse(context.Response, HttpStatusCode.OK, responseBody, null, true, RequestEnablesGzip(context.Request));
            OnResponseReady(context, requestStopwatch);
            return Task.CompletedTask;
        }

        #endregion

        #region statistics

        public static bool StatisticsEnabled { get; private set; }

        private static ServerStatistics<TAccount> _statistics;

        private static Delegates.StatisticsAuthorizer _statisticsAuthorizer;

        private static bool IsStatisticsRequest(HttpListenerRequest request)
        {
            if (request == null)
            {
                return false;
            }
            return request.Url.LocalPath.Trim('/') == "statistics" && StatisticsEnabled;
        }

        private static async Task HandleStatistics(HttpListenerContext context, Stopwatch requestStopwatch)
        {
            var response = context.Response;
            if (_statisticsAuthorizer != null)
            {
                if (await _statisticsAuthorizer.Invoke(context.Request))
                {
                    var responseBody = _statistics?.Serialize();
                    ResponseFactory.BuildResponse(response, HttpStatusCode.OK, responseBody, null, true);
                    response.ContentType = "text/plain";
                }
                else
                {
                    ResponseFactory.BuildResponse(response, HttpStatusCode.Forbidden, null);
                }
            }
            else
            {
                var responseBody = _statistics?.Serialize();
                ResponseFactory.BuildResponse(response, HttpStatusCode.OK, responseBody, null, true, RequestEnablesGzip(context.Request));
                response.ContentType = "text/plain";
            }

            OnResponseReady(context, requestStopwatch);
        }

        public static string GetStatistics() => _statistics?.Serialize();

        #endregion

        #region options

        private static bool IsOptionsRequest(HttpListenerRequest request)
        {
            if (request == null)
            {
                return false;
            }
            return request.HttpMethod.ToUpper() == "OPTIONS";
        }

        private static Task HandleOptions(HttpListenerContext context, Stopwatch requestStopwatch)
        {
            var allowValues = new List<string>();
            var request = context.Request;
            if (!IsLoginRequest(request) && !IsPingRequest(request) && !IsStatisticsRequest(request))
            {
                foreach (var requestProcessor in InnerRequestProcessors)
                {
                    if (request.Url.LocalPath.Trim('/') == requestProcessor.SubUri)
                    {
                        allowValues.Add(requestProcessor.Method.Method);
                        if (requestProcessor.Method == HttpMethod.Get)
                            allowValues.Add("HEAD");
                    }
                }
            }
            else
            {
                allowValues.Add("GET");
                allowValues.Add("HEAD");
            }


            if (allowValues.Any())
            {
                ResponseFactory.BuildResponse(context.Response, HttpStatusCode.OK, null, null, false, RequestEnablesGzip(request));
                context.Response.AddHeader("Allow", string.Join(", ", allowValues));
            }
            else
            {
                ResponseFactory.BuildResponse(context.Response, HttpStatusCode.NotFound, null);
            }

            OnResponseReady(context, requestStopwatch);
            return Task.CompletedTask;
        }

        #endregion

        #region favicon

        private static bool IsFaviconRequest(HttpListenerRequest request)
        {
            if (request == null)
            {
                return false;
            }
            return request.Url.LocalPath.Trim('/').ToLower() == "favicon.ico";
        }

        private static async Task HandleFavicon(HttpListenerContext context, Stopwatch requestStopwatch)
        {
            await FileRequestController<TAccount>.HandleFavicon(context);
            OnResponseReady(context, requestStopwatch);
        }
        #endregion

        #region files

        public static bool FilesEnabled { get; private set; }

        public static string FilesLocation => FileRequestController<TAccount>.FilesLocation;

        public static string FilesBaseUri { get; private set; }

        public static bool FilesNeedAuthorization => FileRequestController<TAccount>.FilesNeedAuthorization;

        public static List<FileSection> FileSections => FileRequestController<TAccount>.FileSections;

        private static bool IsFilesRequest(HttpListenerRequest request)
        {
            if (request == null || !FilesEnabled)
            {
                return false;
            }
            return request.Url.LocalPath.Trim('/').StartsWith(FilesBaseUri);
        }

        private static async Task HandleFileRequest(HttpListenerContext context, Stopwatch requestStopwatch)
        {
            await FileRequestController<TAccount>.HandleFileRequest(context);
            OnResponseReady(context, requestStopwatch);
        }

        public static bool FileExists(string sectionName, string filename)
        {
            if (!FilesEnabled)
                return false;
            return FileRequestController<TAccount>.FileExists(sectionName, filename);
        }

        public static Stream GetFileStream(string sectionName, string filename)
        {
            if (!FilesEnabled)
                return null;
            return FileRequestController<TAccount>.GetFileStream(sectionName, filename);
        }

        public static async Task<string> GetFileString(string sectionName, string filename)
        {
            if (!FilesEnabled)
                return null;
            return await FileRequestController<TAccount>.GetFileString(sectionName, filename);
        }

        public static async Task<FileOperationStatus> AddFile(string sectionName, string filename, Stream content)
        {
            if (!FilesEnabled)
                return FileOperationStatus.FilesNotEnabled;
            return await FileRequestController<TAccount>.AddFile(sectionName, filename, content);
        }

        public static FileOperationStatus DeleteFile(string sectionName, string filename)
        {
            if (!FilesEnabled)
                return FileOperationStatus.FilesNotEnabled;
            return FileRequestController<TAccount>.DeleteFile(sectionName, filename);
        }

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
        {
            if (requestProcessors == null)
                return true;

            return requestProcessors.All(AddRequestProcessor);
        }

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

            var processorsForUri = InnerRequestProcessors.Where(requestProcessor => localUri == requestProcessor.SubUri.Trim('/')).ToList();
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

        private static bool RequestEnablesGzip(HttpListenerRequest request)
        {
            if (!_autoGzipCompression)
                return false;
            if (request == null)
                return false;
            if (!request.Headers.AllKeys.Contains("Accept-Encoding"))
                return false;
            var parts = request.Headers["Accept-Encoding"].Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Any(p => p == "gzip");
        }

        private static string SerializeHttpRequest(HttpListenerRequest request, bool absolutePath = false)
        {
            if (request == null)
                return null;

            var builder = new StringBuilder();
            var queryString = string.Join("&", request.QueryString.AllKeys.Select(k => $"{k}={QueryParamValueForLog(request, k)}"));
            var separator = string.IsNullOrEmpty(queryString) ? string.Empty : "?";
            var url = absolutePath ? request.Url.ToString() : request.Url.LocalPath;
            builder.AppendLine($"{request.HttpMethod} {url} HTTP/{request.ProtocolVersion}{separator}{queryString}");

            foreach (var key in request.Headers.AllKeys)
            {
                if (ResponseFactory.LogProhibitedHeaders.Contains(key))
                    builder.AppendLine($"{key}: {Constants.RemovedLogString}");
                else
                    builder.AppendLine($"{key}: {request.Headers[key]}");
            }
#if TRACE
            if (!request.HasEntityBody)
                return builder.ToString();

            if (!IsFilesRequest(request))
            {
                using (var reader = new StreamReader(request.InputStream, _requestEncoding, true, 4096, true))
                {
                    var bodyString = reader.ReadToEnd();
                    bodyString = ResponseFactory.LogBodyReplacePatterns.Aggregate(bodyString, (current, replacePattern) => Regex.Replace(current, replacePattern.Item1, replacePattern.Item2));
                    builder.AppendLine(bodyString);
                }
                request.InputStream.Position = 0;
            }
            else
            {
                builder.AppendLine("<Binary content>");
            }
#endif
            return builder.ToString();
        }

        private static string QueryParamValueForLog(HttpListenerRequest request, string paramName) => _logProhibitedQueryParams.Contains(paramName)
            ? Constants.RemovedLogString
            : request.QueryString[paramName];

        public static Dictionary<string, string> StaticRedirectionTable => new Dictionary<string, string>(InnerStaticRedirectionTable);
        public static List<RequestProcessor<TAccount>> RequestProcessors => new List<RequestProcessor<TAccount>>(InnerRequestProcessors);

        private static readonly List<RequestProcessor<TAccount>> InnerRequestProcessors =
            new List<RequestProcessor<TAccount>>();
        private static readonly ConcurrentDictionary<string, string> InnerStaticRedirectionTable = new ConcurrentDictionary<string, string>();
        private static Encoding _requestEncoding = Encoding.UTF8;
        private static bool _autoGzipCompression;
        private static List<string> _logProhibitedQueryParams;

        #endregion
    }
}