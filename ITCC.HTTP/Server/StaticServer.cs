using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using Griffin.Net.Protocols.Http;
using ITCC.HTTP.Common;
using ITCC.HTTP.Enums;
using ITCC.HTTP.Utils;
using ITCC.Logging;
using Newtonsoft.Json;
using HttpListener = Griffin.Net.Protocols.Http.HttpListener;

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

            _listener = new HttpListener(configuration.BufferPoolSize)
            {
                MessageReceived = OnMessage
            };
            _listener.ClientConnected += OnClientConnected;
            _listener.ClientDisconnected += OnClientDisconnected;
            _listener.ListenerError += OnListenerError;

            try
            {
                Protocol = configuration.Protocol;
                SuitableSslProtocols = configuration.SuitableSslProtocols;
                if (configuration.Protocol == Protocol.Https)
                {
                    var certificate = configuration.CertificateProvider.Invoke(configuration.SubjectName,
                        configuration.AllowSelfSignedCertificates);
                    if (certificate == null)
                    {
                        LogMessage(LogLevel.Warning, "Certificate error");
                        return ServerStartStatus.CertificateError;
                    }
                    _listener.ChannelFactory =
                        new SecureTcpChannelFactory(new ServerSideSslStreamBuilder(certificate, SuitableSslProtocols));
                    LogMessage(LogLevel.Info,
                        $"Server certificate {certificate.SubjectName.Decode(X500DistinguishedNameFlags.None)}");
                }
                _authorizer = configuration.Authorizer;
                _authentificator = configuration.Authentificator;
                _fileAuthorizer = configuration.FilesAuthorizer;

                ResponseFactory.SetBodySerializer(configuration.BodySerializer);
                ResponseFactory.SetBodyEncoding(configuration.BodyEncoding);
                ResponseFactory.LogResponseBodies = configuration.LogResponseBodies;
                ResponseFactory.ResponseBodyLogLimit = configuration.ResponseBodyLogLimit;

                FilesEnabled = configuration.FilesEnabled;
                FilesBaseUri = configuration.FilesBaseUri;
                FilesLocation = configuration.FilesLocation;
                FileSections = configuration.FileSections;

                if (FilesEnabled && !IOHelper.HasWriteAccessToDirectory(FilesLocation))
                {
                    LogMessage(LogLevel.Warning, $"Cannot use file folder {FilesLocation} : no write access");
                    return ServerStartStatus.BadParameters;
                }

                FilesNeedAuthorization = configuration.FilesNeedAuthorization;
                _faviconPath = configuration.FaviconPath;

                _requestMaxServeTime = configuration.RequestMaxServeTime;

                _autoGzipCompression = configuration.AutoGzipCompression;

                if (configuration.ServerName != null)
                {
                    ResponseFactory.SetCommonHeaders(new Dictionary<string, string>
                    {
                        {"Server", configuration.ServerName}
                    });
                }
                _listener.Start(IPAddress.Any, configuration.Port);
                _started = true;
                var protocolString = configuration.Protocol == Protocol.Https ? "https" : "http";
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

        private static bool _started;
        private static HttpListener _listener;
        private static bool _operationInProgress;
        private static readonly object OperationLock = new object();
        private static string _serverAddress;

        #endregion

        #region security

        public static Protocol Protocol { get; private set; }

        public static SslProtocols SuitableSslProtocols { get; private set; }

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

        private static async void OnMessage(ITcpChannel channel, object message)
        {
            var request = (HttpRequest)message;
            // This Stopwatch will be used by different threads, but sequentially (select processor => handle => completion)
            var stopWatch = Stopwatch.StartNew();
            try
            {
                _statistics?.AddRequest(request);
                HttpResponse response;
                LogMessage(LogLevel.Debug,
                    $"Request from {channel.RemoteEndpoint}.\n{SerializeHttpRequest(request)}");

                foreach (var checker in ServiceHandlers.Keys)
                {
                    if (checker.Invoke(request))
                    {
                        await ServiceHandlers[checker].Invoke(channel, request, stopWatch).ConfigureAwait(false);
                        return;
                    }
                }

                var requestProcessorSelectionResult = SelectRequestProcessor(request);
                if (requestProcessorSelectionResult == null)
                {
                    response = ResponseFactory.CreateResponse(HttpStatusCode.NotFound, null);
                    OnResponseReady(channel, response, "/" + request.Uri.LocalPath.Trim('/'), stopWatch);
                    return;
                }

                if (requestProcessorSelectionResult.IsRedirect)
                {
                    var redirectLocation = $"{_serverAddress}{requestProcessorSelectionResult.RequestProcessor.SubUri}";
                    response = ResponseFactory.CreateResponse(HttpStatusCode.Found, null,
                        new Dictionary<string, string> { { "Location", redirectLocation } });
                    OnResponseReady(channel, response, "/" + request.Uri.LocalPath.Trim('/'), stopWatch);
                    return;
                }

                if (!requestProcessorSelectionResult.MethodMatches)
                {
                    response = ResponseFactory.CreateResponse(HttpStatusCode.MethodNotAllowed, null);
                    OnResponseReady(channel, response, "/" + request.Uri.LocalPath.Trim('/'), stopWatch);
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
                                $"{request.HttpMethod} {request.Uri.LocalPath} was requested, but no handler is provided");
                            response = ResponseFactory.CreateResponse(HttpStatusCode.NotImplemented, null);
                        }
                        else
                        {
                            var handleResult = await requestProcessor.Handler.Invoke(authResult.Account, request).ConfigureAwait(false);
                            response = ResponseFactory.CreateResponse(handleResult, false, RequestEnablesGzip(request));
                            if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Head)
                            {
                                var savedBody = response.Body;
                                response.Body = null;
                                response.ContentLength = Convert.ToInt32(savedBody.Length);
                            }
                        }
                        break;
                    default:
                        response = ResponseFactory.CreateResponse(authResult, RequestEnablesGzip(request));
                        break;
                }
                OnResponseReady(channel, response, "/" + request.Uri.LocalPath.Trim('/'), stopWatch);
            }
            catch (Exception exception)
            {
                LogMessage(LogLevel.Warning, $"Error handling client request from {channel.RemoteEndpoint}");
                LogException(LogLevel.Warning, exception);
                OnResponseReady(channel,
                    ResponseFactory.CreateResponse(HttpStatusCode.InternalServerError, null),
                    "/" + request.Uri.LocalPath.Trim('/'),
                    stopWatch);
            }
        }

        private static void OnClientConnected(object sender, ClientConnectedEventArgs clientConnectedEventArgs)
        {
            var sslDescription = string.Empty;
            if (Protocol == Protocol.Https)
            {
                var secureChannel = clientConnectedEventArgs.Channel as SecureTcpChannel;
                if (secureChannel != null)
                {
                    var protocol = secureChannel.SslProtocol;
                    sslDescription = $"; SSL version={protocol}";
                    if (StatisticsEnabled)
                        _statistics.AddSslProtocol(protocol);
                }
            }
            else
            {
                if (StatisticsEnabled)
                    _statistics.AddSslProtocol(SslProtocols.None);
            }
            LogMessage(LogLevel.Debug, $"Client connected: {clientConnectedEventArgs.Channel.RemoteEndpoint}{sslDescription}");
        }

        private static void OnClientDisconnected(object sender, ClientDisconnectedEventArgs clientDisconnectedEventArgs)
        {
            LogMessage(LogLevel.Trace, $"Client {clientDisconnectedEventArgs.Channel.RemoteEndpoint} disconnected with status {clientDisconnectedEventArgs.Exception.Message}");
        }

        private static void OnListenerError(object sender, ErrorEventArgs errorEventArgs)
        {
            LogException(LogLevel.Warning, errorEventArgs.GetException());
        }

        private static void OnResponseReady(ITcpChannel channel, HttpResponse response, string uri, Stopwatch requestStopwatch)
        {
            try
            {
#if TRACE
                LogMessage(LogLevel.Trace,
                    $"Response for {channel.RemoteEndpoint} ready. \n{ResponseFactory.SerializeResponse(response)}");
#endif
                requestStopwatch.Stop();
                var elapsedMilliseconds = requestStopwatch.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
                _statistics?.AddResponse(response, uri, elapsedMilliseconds);
                if (_requestMaxServeTime > 0 && _requestMaxServeTime < elapsedMilliseconds)
                {
                    LogMessage(LogLevel.Warning, $"Request /{uri} from {channel.RemoteEndpoint} took {elapsedMilliseconds} milliseconds to process!");
                }
                channel.Send(response);
                LogMessage(LogLevel.Debug, $"Response for {channel.RemoteEndpoint} sent ({elapsedMilliseconds} milliseconds)");
            }
            catch (SocketException)
            {
                LogMessage(LogLevel.Info, "Sudden client disconnect, failed to send response");
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                try
                {
                    channel.Close();
                }
                catch (Exception)
                {
                    // ignore
                }
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

        private static bool IsLoginRequest(HttpRequest request)
        {
            if (request == null || _authentificator == null)
            {
                return false;
            }
            if (request.Uri.LocalPath.Trim('/').StartsWith("login"))
            {
                return true;
            }
            return request.QueryString["login"] != null && request.QueryString["password"] != null;
        }

        private static async Task Authentificate(ITcpChannel channel, HttpRequest request, Stopwatch requestStopwatch)
        {
            AuthentificationResult authResult;
            if (_authentificator != null)
                authResult = await _authentificator.Invoke(request);
            else
                authResult = new AuthentificationResult(null, HttpStatusCode.NotFound);
            if (authResult == null)
                throw new InvalidOperationException("Authentificator fault: null result");
            var response = ResponseFactory.CreateResponse(authResult, RequestEnablesGzip(request));
            OnResponseReady(channel, response, "/login", requestStopwatch);
        }

        private static Delegates.Authentificator _authentificator;

        private static Delegates.Authorizer<TAccount> _authorizer;

        #endregion

        #region ping
        private static bool IsPingRequest(HttpRequest request)
        {
            if (request == null)
            {
                return false;
            }
            if (request.QueryString["ping"] != null)
            {
                return true;
            }
            return request.Uri.LocalPath.Trim('/') == "ping";
        }

        private static Task HandlePing(ITcpChannel channel, HttpRequest request, Stopwatch requestStopwatch)
        {
            var converter = new PingJsonConverter();
            var responseBody = JsonConvert.SerializeObject(new PingResponse(request.ToString()), Formatting.None, converter);
            responseBody = responseBody.Replace(@"\", "");
            var response = ResponseFactory.CreateResponse(HttpStatusCode.OK, responseBody, null, true, RequestEnablesGzip(request));
            OnResponseReady(channel, response, "/ping", requestStopwatch);
            return Task.CompletedTask;
        }

        #endregion

        #region statistics

        public static bool StatisticsEnabled { get; private set; }

        private static ServerStatistics<TAccount> _statistics;

        private static Delegates.StatisticsAuthorizer _statisticsAuthorizer;

        private static bool IsStatisticsRequest(HttpRequest request)
        {
            if (request == null)
            {
                return false;
            }
            return request.Uri.LocalPath.Trim('/') == "statistics" && StatisticsEnabled;
        }

        private static async Task HandleStatistics(ITcpChannel channel, HttpRequest request, Stopwatch requestStopwatch)
        {
            HttpResponse response;
            if (_statisticsAuthorizer != null)
            {
                if (await _statisticsAuthorizer.Invoke(request))
                {
                    var responseBody = _statistics?.Serialize();
                    response = ResponseFactory.CreateResponse(HttpStatusCode.OK, responseBody, null, true);
                    response.ContentType = "text/plain";
                }
                else
                {
                    response = ResponseFactory.CreateResponse(HttpStatusCode.Forbidden, null);
                }
            }
            else
            {
                var responseBody = _statistics?.Serialize();
                response = ResponseFactory.CreateResponse(HttpStatusCode.OK, responseBody, null, true, RequestEnablesGzip(request));
                response.ContentType = "text/plain";
            }

            OnResponseReady(channel, response, "/statistics", requestStopwatch);
        }
        #endregion

        #region options

        private static bool IsOptionsRequest(HttpRequest request)
        {
            if (request == null)
            {
                return false;
            }
            return request.HttpMethod.ToUpper() == "OPTIONS";
        }

        private static Task HandleOptions(ITcpChannel channel, HttpRequest request, Stopwatch requestStopwatch)
        {
            var allowValues = new List<string>();
            if (!IsLoginRequest(request) && !IsPingRequest(request) && !IsStatisticsRequest(request))
            {
                foreach (var requestProcessor in InnerRequestProcessors)
                {
                    if (request.Uri.LocalPath.Trim('/') == requestProcessor.SubUri)
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


            HttpResponse response;
            if (allowValues.Any())
            {
                response = ResponseFactory.CreateResponse(HttpStatusCode.OK, null, null, false, RequestEnablesGzip(request));
                response.AddHeader("Allow", string.Join(", ", allowValues));
            }
            else
            {
                response = ResponseFactory.CreateResponse(HttpStatusCode.NotFound, null);
            }

            OnResponseReady(channel, response, "/" + request.Uri.LocalPath.Trim('/'), requestStopwatch);
            return Task.CompletedTask;
        }

        #endregion

        #region favicon

        private static string _faviconPath;

        private static bool IsFaviconRequest(HttpRequest request)
        {
            if (request == null)
            {
                return false;
            }
            return request.Uri.LocalPath.Trim('/').ToLower() == "favicon.ico";
        }

        private static Task HandleFavicon(ITcpChannel channel, HttpRequest request, Stopwatch requestStopwatch)
        {
            HttpResponse response;
            LogMessage(LogLevel.Trace, $"Favicon requested, path: {_faviconPath}");
            if (string.IsNullOrEmpty(_faviconPath) || !File.Exists(_faviconPath))
            {
                response = ResponseFactory.CreateResponse(HttpStatusCode.NotFound, null);
                OnResponseReady(channel, response, "/favicon.ico", requestStopwatch);
                return Task.CompletedTask;
            }
            response = ResponseFactory.CreateResponse(HttpStatusCode.OK, null);
            var fileStream = new FileStream(_faviconPath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite);
            response.ContentType = "image/x-icon";
            response.Body = fileStream;

            OnResponseReady(channel, response, "/favicon.ico", requestStopwatch);
            return Task.CompletedTask;
        }
        #endregion

        #region files

        private static Delegates.FilesAuthorizer<TAccount> _fileAuthorizer;

        public static bool FilesEnabled { get; private set; }

        public static string FilesLocation { get; private set; }

        public static string FilesBaseUri { get; private set; }

        public static bool FilesNeedAuthorization { get; private set; }

        public static List<FileSection> FileSections { get; private set; }

        private static bool IsFilesRequest(HttpRequest request)
        {
            if (request == null || !FilesEnabled)
            {
                return false;
            }
            return request.Uri.LocalPath.Trim('/').StartsWith(FilesBaseUri);
        }

        private static async Task HandleFileRequest(ITcpChannel channel, HttpRequest request, Stopwatch requestStopwatch)
        {
            HttpResponse response;
            try
            {
                var filename = ExtractFileName(request.Uri.LocalPath);
                var section = ExtractFileSection(request.Uri.LocalPath);
                if (filename == null || section == null)
                {
                    response = ResponseFactory.CreateResponse(HttpStatusCode.BadRequest, null);
                    OnResponseReady(channel, response, "/" + request.Uri.LocalPath.Trim('/'), requestStopwatch);
                    return;
                }
                var filePath = FilesLocation + Path.DirectorySeparatorChar + section.Folder + Path.DirectorySeparatorChar + filename;

                AuthorizationResult<TAccount> authResult;
                if (FilesNeedAuthorization)
                {
                    authResult = await _fileAuthorizer.Invoke(request, section, filename).ConfigureAwait(false);
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
                        if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Get)
                            response = HandleFileGetRequest(filePath);
                        else if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Post)
                        {
                            response = await HandleFilePostRequest(request, section, filePath).ConfigureAwait(false);
                        }
                        else if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Delete)
                        {
                            response = HandleFileDeleteRequest(filePath);
                        }
                        else
                        {
                            response = ResponseFactory.CreateResponse(HttpStatusCode.MethodNotAllowed, null);
                        }
                        break;
                    default:
                        response = ResponseFactory.CreateResponse(authResult);
                        break;
                }
                OnResponseReady(channel, response, "/" + request.Uri.LocalPath.Trim('/'), requestStopwatch);
            }
            catch (Exception exception)
            {
                LogException(LogLevel.Warning, exception);
                response = ResponseFactory.CreateResponse(HttpStatusCode.InternalServerError, null);
                OnResponseReady(channel, response, "/" + request.Uri.LocalPath.Trim('/'), requestStopwatch);
            }
        }

        private static HttpResponse HandleFileGetRequest(string filePath)
        {
            HttpResponse response;
            if (!File.Exists(filePath))
            {
                LogMessage(LogLevel.Debug, $"File {filePath} was requested but not found");
                response = ResponseFactory.CreateResponse(HttpStatusCode.NotFound, null);
                return response;
            }
            response = ResponseFactory.CreateResponse(HttpStatusCode.OK, null);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite);
            response.ContentType = DetermineContentType(filePath);
            response.Body = fileStream;
            return response;
        }

        private static async Task<HttpResponse> HandleFilePostRequest(HttpRequest request, FileSection section, string filePath)
        {
            HttpResponse response;
            if (File.Exists(filePath))
            {
                LogMessage(LogLevel.Debug, $"File {filePath} already exists");
                response = ResponseFactory.CreateResponse(HttpStatusCode.Conflict, null);
                return response;
            }
            var fileContent = request.Body;
            if (section.MaxFileSize > 0 && fileContent.Length > section.MaxFileSize)
            {
                LogMessage(LogLevel.Debug, $"Trying to create file of size {fileContent.Length} in section {section.Name} with max size of {section.MaxFileSize}");
                response = ResponseFactory.CreateResponse(HttpStatusCode.RequestEntityTooLarge, null);
                return response;
            }
            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                await fileContent.CopyToAsync(file).ConfigureAwait(false);
                file.Flush();
                file.Close();
            }
            fileContent.Flush();
            fileContent.Close();
            fileContent.Dispose();
            GC.Collect();
            LogMessage(LogLevel.Trace, $"Total memory: {GC.GetTotalMemory(true)}");
            LogMessage(LogLevel.Debug, $"File {filePath} created");
            response = ResponseFactory.CreateResponse(HttpStatusCode.Created, null);

            return response;
        }

        private static HttpResponse HandleFileDeleteRequest(string filePath)
        {
            HttpResponse response;
            if (!File.Exists(filePath))
            {
                LogMessage(LogLevel.Debug, $"File {filePath} does not exist and cannot be deleted");
                response = ResponseFactory.CreateResponse(HttpStatusCode.NotFound, null);
                return response;
            }

            try
            {
                File.Delete(filePath);
                response = ResponseFactory.CreateResponse(HttpStatusCode.OK, null);
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                response = ResponseFactory.CreateResponse(HttpStatusCode.InternalServerError, null);
            }
            return response;
        }

        private static string ExtractFileName(string localPath)
        {
            if (localPath == null)
                return null;
            try
            {
                var result = localPath.Trim('/');
                var slashIndex = result.LastIndexOf("/", StringComparison.Ordinal);
                result = result.Remove(0, slashIndex + 1);
                LogMessage(LogLevel.Debug, $"File requested: {result}");
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static FileSection ExtractFileSection(string localPath)
        {
            if (localPath == null)
                return null;
            try
            {
                var sectionName = localPath.Trim('/');
                var slashIndex = sectionName.IndexOf("/", StringComparison.Ordinal);
                var lastSlashIndex = sectionName.LastIndexOf("/", StringComparison.Ordinal);
                sectionName = sectionName.Substring(slashIndex + 1, lastSlashIndex - slashIndex - 1);
                LogMessage(LogLevel.Debug, $"Section requested: {sectionName}");
                return FileSections.FirstOrDefault(s => s.Folder == sectionName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string DetermineContentType(string filename)
        {
            if (!filename.Contains("."))
                return "x-application/unknown";

            var lastDotIndex = filename.LastIndexOf(".", StringComparison.Ordinal);
            return MimeTypes.GetTypeByExtenstion(filename.Remove(0, lastDotIndex + 1));
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

        private static RequestProcessorSelectionResult<TAccount> SelectRequestProcessor(HttpRequest request)
        {
            var requestMethod = CommonHelper.HttpMethodToEnum(request.HttpMethod);
            var localUri = request.Uri.LocalPath.Trim('/');
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

        private static bool RequestEnablesGzip(IHttpMessage request)
        {
            if (!_autoGzipCompression)
                return false;
            if (request == null)
                return false;
            if (!request.Headers.Contains("Accept-Encoding"))
                return false;
            var parts = request.Headers["Accept-Encoding"].Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Any(p => p == "gzip");
        }

        private static string SerializeHttpRequest(HttpRequest request)
        {
            if (request == null)
                return null;

            var builder = new StringBuilder();
            builder.AppendLine($"{request.StatusLine}");

            foreach (var header in request.Headers)
            {
                builder.AppendLine($"{header.Key}: {header.Value}");
            }
#if TRACE
            if (request.Body == null)
                return builder.ToString();
            using (var reader = new StreamReader(request.Body))
            {
                builder.AppendLine(reader.ReadToEnd());
            }
            request.Body.Position = 0;
#endif

            return builder.ToString();
        }

        public static Dictionary<string, string> StaticRedirectionTable => new Dictionary<string, string>(InnerStaticRedirectionTable);
        public static List<RequestProcessor<TAccount>> RequestProcessors => new List<RequestProcessor<TAccount>>(InnerRequestProcessors);

        private static readonly List<RequestProcessor<TAccount>> InnerRequestProcessors =
            new List<RequestProcessor<TAccount>>();
        private static readonly ConcurrentDictionary<string, string> InnerStaticRedirectionTable = new ConcurrentDictionary<string, string>();
        private static bool _autoGzipCompression;

        #endregion
    }
}