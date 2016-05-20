using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
        #region start

        /// <summary>
        ///     Starts server for incoming connection listening
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ServerStartStatus Start(HttpServerConfiguration<TAccount> configuration)
        {
            if (configuration == null || !configuration.IsEnough())
                return ServerStartStatus.BadParameters;

            if (configuration.StatisticsEnabled)
            {
                StatisticsEnabled = true;
                _statistics = new ServerStatistics<TAccount>();
                _statisticsAuthorizer = configuration.StatisticsAuthorizer;
            }

            var listener = new HttpListener(configuration.BufferPoolSize)
            {
                MessageReceived = OnMessage
            };
            listener.ClientConnected += OnClientConnected;
            listener.ClientDisconnected += OnClientDisconnected;

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
                    listener.ChannelFactory = new SecureTcpChannelFactory(new ServerSideSslStreamBuilder(certificate, SuitableSslProtocols));
                    LogMessage(LogLevel.Info, $"Server certificate {certificate.SubjectName.Decode(X500DistinguishedNameFlags.None)}");
                }
                _authorizer = configuration.Authorizer;
                _authentificator = configuration.Authentificator;
                _fileAuthorizer = configuration.FilesAuthorizer;

                ResponseFactory.SetBodySerializer(configuration.BodySerializer);

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

                if (configuration.ServerName != null)
                {
                    ResponseFactory.SetCommonHeaders(new Dictionary<string, string>
                    {
                        {"Server", configuration.ServerName}
                    });
                }
                listener.Start(IPAddress.Any, configuration.Port);
                LogMessage(LogLevel.Info, $"Started listening port {configuration.Port}");

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
        }

        #endregion

        #region security

        public static Protocol Protocol { get; private set; }

        public static SslProtocols SuitableSslProtocols { get; private set; }

        #endregion

        #region log

        /// <summary>
        ///     Simple server logger
        /// </summary>
        /// <param name="level">Message level</param>
        /// <param name="message">Message text</param>
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

#pragma warning disable AsyncFixer03 // Avoid fire & forget async void methods
        /// <summary>
        ///     Event handler for received HTTP message
        /// </summary>
        /// <param name="channel">Client TCP channel</param>
        /// <param name="message">Receiver request</param>
        private static async void OnMessage(ITcpChannel channel, object message)
#pragma warning restore AsyncFixer03 // Avoid fire & forget async void methods
        {
            try
            {
                var request = (HttpRequest) message;
                _statistics?.AddRequest(request);
                HttpResponse response;
                LogMessage(LogLevel.Debug,
                    $"Request from {channel.RemoteEndpoint}. Method: {request.HttpMethod}, URI: {request.Uri.LocalPath}");

                foreach (var checker in ServiceHandlers.Keys)
                {
                    if (checker.Invoke(request))
                    {
                        await Task.Run(() => ServiceHandlers[checker].Invoke(channel, request));
                        return;
                    }
                }

                var requestProcessor = SelectRequestProcessor(request);
                if (requestProcessor == null)
                {
                    response = ResponseFactory.CreateResponse(HttpStatusCode.NotFound, null);
                    OnResponseReady(channel, response);
                    return;
                }
                _statistics?.AddRequestProcessor(requestProcessor);

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
                            var handleResult = await requestProcessor.Handler.Invoke(authResult.Account, request);
                            if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Head)
                                handleResult.Body = null;
                            response = ResponseFactory.CreateResponse(handleResult.Status, handleResult.Body);
                        }
                        break;
                    case AuthorizationStatus.Unauthorized:
                        response = ResponseFactory.CreateResponse(HttpStatusCode.Unauthorized, null);
                        break;
                    case AuthorizationStatus.Forbidden:
                        response = ResponseFactory.CreateResponse(HttpStatusCode.Forbidden, null);
                        break;
                    case AuthorizationStatus.TooManyRequests:
                        response = ResponseFactory.CreateResponse((HttpStatusCode)429, null);
                        response.AddHeader("Retry-After", authResult.Userdata.ToString());
                        break;
                    default:
                        response = ResponseFactory.CreateResponse(HttpStatusCode.InternalServerError, null);
                        break;
                }
                OnResponseReady(channel, response);
            }
            catch (Exception exception)
            {
                LogMessage(LogLevel.Warning, $"Error handling client request from {channel.RemoteEndpoint}");
                LogException(LogLevel.Warning, exception);
                OnResponseReady(channel, ResponseFactory.CreateResponse(HttpStatusCode.InternalServerError, null));
            }
        }

        /// <summary>
        ///     Client connection handler
        /// </summary>
        /// <param name="sender">Event context</param>
        /// <param name="clientConnectedEventArgs">Event params</param>
        private static void OnClientConnected(object sender, ClientConnectedEventArgs clientConnectedEventArgs)
        {
            LogMessage(LogLevel.Trace, $"Client connected: {clientConnectedEventArgs.Channel.RemoteEndpoint}");
        }

        /// <summary>
        ///     Client disconnection handler
        /// </summary>
        /// <param name="sender">Event context</param>
        /// <param name="clientDisconnectedEventArgs">Event params</param>
        private static void OnClientDisconnected(object sender, ClientDisconnectedEventArgs clientDisconnectedEventArgs)
        {
            LogMessage(LogLevel.Trace, $"Client {clientDisconnectedEventArgs.Channel.RemoteEndpoint} disconnected with status {clientDisconnectedEventArgs.Exception.Message}");
        }

        /// <summary>
        ///     Method invoked when response is prepared
        /// </summary>
        /// <param name="channel">Client TCP channel</param>
        /// <param name="response">Server HTTP response</param>
        private static void OnResponseReady(ITcpChannel channel, HttpResponse response)
        {
            try
            {
                LogMessage(LogLevel.Trace,
                    $"Response for {channel.RemoteEndpoint} ready. \n{ResponseFactory.SerializeResponse(response)}");
                _statistics?.AddResponse(response);
                channel.Send(response);
                LogMessage(LogLevel.Trace, $"Response for {channel.RemoteEndpoint} sent");
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

        #endregion

        #region auth

        private static bool IsLoginRequest(HttpRequest request)
        {
            if (request == null)
            {
                return false;
            }
            if (request.Uri.LocalPath.Trim('/').StartsWith("login"))
            {
                return true;
            }
            return request.QueryString["login"] != null && request.QueryString["password"] != null;
        }

#pragma warning disable AsyncFixer03 // Avoid fire & forget async void methods
        private static async void Authentificate(ITcpChannel channel, HttpRequest request)
#pragma warning restore AsyncFixer03 // Avoid fire & forget async void methods
        {
            AuthentificationResult authResult;
            if (_authentificator != null)
                authResult = await _authentificator.Invoke(request);
            else
                authResult = new AuthentificationResult(null, HttpStatusCode.NotFound);
            if (authResult == null)
                throw new InvalidOperationException("Authentificator fault: null result");
            var response = ResponseFactory.CreateResponse(authResult.Status, authResult.AccountView);
            if (authResult.Status == (HttpStatusCode) 429)
            {
                response.AddHeader("Retry-After", authResult.Userdata.ToString());
            }
            OnResponseReady(channel, response);
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

        private static void HandlePing(ITcpChannel channel, HttpRequest request)
        {
            var converter = new PingJsonConverter();
            var responseBody = JsonConvert.SerializeObject(new PingResponse(request.ToString()), Formatting.None, converter);
            responseBody = responseBody.Replace(@"\", "");
            var response = ResponseFactory.CreateResponse(HttpStatusCode.OK, responseBody, true);
            OnResponseReady(channel, response);
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

        private static async void HandleStatistics(ITcpChannel channel, HttpRequest request)
        {
            HttpResponse response;
            if (_statisticsAuthorizer != null && await _statisticsAuthorizer.Invoke(request))
            {
                var responseBody = _statistics?.Serialize();
                response = ResponseFactory.CreateResponse(HttpStatusCode.OK, responseBody, true);
                response.ContentType = "text/plain";
            }
            else
            {
                response = ResponseFactory.CreateResponse(HttpStatusCode.Forbidden, null);
            }
            
            
            OnResponseReady(channel, response);
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

        private static void HandleOptions(ITcpChannel channel, HttpRequest request)
        {
            var allowValues = new List<string>();
            if (!IsLoginRequest(request) && !IsPingRequest(request) && !IsStatisticsRequest(request))
            {
                foreach (var requestProcessor in RequestProcessors)
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
                response = ResponseFactory.CreateResponse(HttpStatusCode.OK, null);
                response.AddHeader("Allow", string.Join(", ", allowValues));
            }
            else
            {
                response = ResponseFactory.CreateResponse(HttpStatusCode.NotFound, null);
            }
            
            OnResponseReady(channel, response);
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

        private static void HandleFavicon(ITcpChannel channel, HttpRequest request)
        {
            HttpResponse response;
            LogMessage(LogLevel.Trace, $"Favicon requested, path: {_faviconPath}");
            if (string.IsNullOrEmpty(_faviconPath) || !File.Exists(_faviconPath))
            {
                response = ResponseFactory.CreateResponse(HttpStatusCode.NotFound, null);
                OnResponseReady(channel, response);
                return;
            }
            response = ResponseFactory.CreateResponse(HttpStatusCode.OK, null);
            var fileStream = new FileStream(_faviconPath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite);
            response.ContentType = "image/x-icon";
            response.Body = fileStream;

            OnResponseReady(channel, response);
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

#pragma warning disable AsyncFixer03 // Avoid fire & forget async void methods
        private static async void HandleFileRequest(ITcpChannel channel, HttpRequest request)
#pragma warning restore AsyncFixer03 // Avoid fire & forget async void methods
        {
            HttpResponse response;
            try
            {
                var filename = ExtractFileName(request.Uri.LocalPath);
                var section = ExtractFileSection(request.Uri.LocalPath);
                if (filename == null || section == null)
                {
                    response = ResponseFactory.CreateResponse(HttpStatusCode.BadRequest, null);
                    OnResponseReady(channel, response);
                    return;
                }
                var filePath = FilesLocation + Path.DirectorySeparatorChar + section + Path.DirectorySeparatorChar + filename;

                AuthorizationResult<TAccount> authResult;
                if (FilesNeedAuthorization)
                {
                    authResult = await _fileAuthorizer.Invoke(request, section, filename);
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
                            response = HandleFileGetRequest(channel, request, filePath);
                        else if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Post)
                        {
                            response = await HandleFilePostRequest(channel, request, filePath);
                        }
                        else if (CommonHelper.HttpMethodToEnum(request.HttpMethod) == HttpMethod.Delete)
                        {
                            response = HandleFileDeleteRequest(channel, request, filePath);
                        }
                        else
                        {
                            response = ResponseFactory.CreateResponse(HttpStatusCode.MethodNotAllowed, null);
                        }
                        break;
                    case AuthorizationStatus.Unauthorized:
                        response = ResponseFactory.CreateResponse(HttpStatusCode.Unauthorized, null);
                        break;
                    case AuthorizationStatus.Forbidden:
                        response = ResponseFactory.CreateResponse(HttpStatusCode.Forbidden, null);
                        break;
                    case AuthorizationStatus.TooManyRequests:
                        response = ResponseFactory.CreateResponse((HttpStatusCode)429, null);
                        response.AddHeader("Retry-After", authResult.Userdata.ToString());
                        break;
                    default:
                        response = ResponseFactory.CreateResponse(HttpStatusCode.InternalServerError, null);
                        break;
                }
                OnResponseReady(channel, response);
            }
            catch (Exception exception)
            {
                LogException(LogLevel.Warning, exception);
                response = ResponseFactory.CreateResponse(HttpStatusCode.InternalServerError, null);
                OnResponseReady(channel, response);
            }    
        }

        private static HttpResponse HandleFileGetRequest(ITcpChannel channel, HttpRequest request, string filePath)
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

        private static async Task<HttpResponse> HandleFilePostRequest(ITcpChannel channel, HttpRequest request, string filePath)
        {
            HttpResponse response;
            if (File.Exists(filePath))
            {
                LogMessage(LogLevel.Debug, $"File {filePath} already exists");
                response = ResponseFactory.CreateResponse(HttpStatusCode.Conflict, null);
                return response;
            }
            var fileContent = request.Body;
            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                await fileContent.CopyToAsync(file);
                file.Flush();
                file.Close();
            }
            fileContent.Flush();
            fileContent.Close();
            fileContent.Dispose();
            GC.Collect();
            LogMessage(LogLevel.Trace, $"Total memory: {GC.GetTotalMemory(true)}");
            LogMessage(LogLevel.Debug, $"File {filePath} created");
            response = ResponseFactory.CreateResponse(HttpStatusCode.OK, null);
            
            return response;
        }

        private static HttpResponse HandleFileDeleteRequest(ITcpChannel channel, HttpRequest request, string filePath)
        {
            HttpResponse response;
            if (! File.Exists(filePath))
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
            var result = localPath.Trim('/');
            var slashIndex = result.LastIndexOf("/", StringComparison.Ordinal);
            result = result.Remove(0, slashIndex + 1);
            LogMessage(LogLevel.Debug, $"File requested: {result}");
            return result;
        }

        private static FileSection ExtractFileSection(string localPath)
        {
            if (localPath == null)
                return null;
            var sectionName = localPath.Trim('/');
            var slashIndex = sectionName.IndexOf("/", StringComparison.Ordinal);
            var lastSlashIndex = sectionName.LastIndexOf("/", StringComparison.Ordinal);
            sectionName = sectionName.Substring(slashIndex + 1, lastSlashIndex - slashIndex - 1);
            LogMessage(LogLevel.Debug, $"Section requested: {sectionName}");
            return FileSections.FirstOrDefault(s => s.Folder == sectionName);
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

        /// <summary>
        ///     Add server request processor
        /// </summary>
        /// <param name="requestProcessor"></param>
        /// <returns></returns>
        public static bool AddRequestProcessor(RequestProcessor<TAccount> requestProcessor)
        {
            if (CheckRequestProcessor(requestProcessor) != true)
                return false;

            requestProcessor.SubUri = requestProcessor.SubUri.Trim('/');
            var duplicate = RequestProcessors.FirstOrDefault(rp => RequestProcessorDuplicates(rp, requestProcessor));
            if (duplicate != null)
                RequestProcessors.Remove(duplicate);

            RequestProcessors.Add(requestProcessor);
            return true;
        }

        public static bool AddRequestProcessorRange(
            IEnumerable<RequestProcessor<TAccount>> requestProcessors)
        {
            if (requestProcessors == null)
                return true;

            return requestProcessors.All(AddRequestProcessor);
        }

        private static bool CheckRequestProcessor(RequestProcessor<TAccount> requestProcessor)
        {
            if (requestProcessor.SubUri == null && requestProcessor.LegacyName == null)
                return false;
            return true;
        }

        /// <summary>
        ///     Selects a way to handle incoming request
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Request processor or null if it is not found</returns>
        private static RequestProcessor<TAccount> SelectRequestProcessor(HttpRequest request)
        {
            var requestMethod = CommonHelper.HttpMethodToEnum(request.HttpMethod);
            foreach (var requestProcessor in RequestProcessors)
            {
                if (requestProcessor.LegacyName != null
                    && request.QueryString[requestProcessor.LegacyName] != null
                    && requestMethod == requestProcessor.Method)
                {
                    return requestProcessor;
                }
            }

            foreach (var requestProcessor in RequestProcessors)
            {
                if (request.Uri.LocalPath.Trim('/') == requestProcessor.SubUri)
                {
                    if (requestMethod == requestProcessor.Method ||
                        (requestMethod == HttpMethod.Head && requestProcessor.Method == HttpMethod.Get))
                    {
                        return requestProcessor;
                    }
                }
            }
            return null;

        }

        /// <summary>
        ///     Legacy API method -> Request processor map
        /// </summary>
        public static readonly List<RequestProcessor<TAccount>> RequestProcessors =
            new List<RequestProcessor<TAccount>>();

        #endregion
    }
}