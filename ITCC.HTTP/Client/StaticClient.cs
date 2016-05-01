using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ITCC.HTTP.Common;
using ITCC.HTTP.Enums;
using ITCC.HTTP.Security;
using ITCC.HTTP.Utils;
using ITCC.Logging;
using Newtonsoft.Json;

namespace ITCC.HTTP.Client
{
    /// <summary>
    ///     Represents static Http client (no instances, single server)
    /// </summary>
    public static class StaticClient
    {
        #region private

        private static string _serverAddress = "http://127.0.0.1/";

        #endregion

        #region general

        /// <summary>
        ///     Most general http method
        /// </summary>
        /// <typeparam name="TBody">Request body type</typeparam>
        /// <typeparam name="TResult">Response body type</typeparam>
        /// <param name="method">Request method (GET/POST...)</param>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="bodyArg">Object to be serialized to request body</param>
        /// <param name="requestBodySerializer">Method to serialize request body</param>
        /// <param name="responseBodyDeserializer">Method to deserialize response body</param>
        /// <param name="authentificationProvider">Method to add authentification data</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<RequestResult<TResult>> PerformRequestAsync<TBody, TResult>(
            HttpMethod method,
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            TBody bodyArg = default(TBody),
            Delegates.BodySerializer requestBodySerializer = null,
            Delegates.BodyDeserializer<TResult> responseBodyDeserializer = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken)) where TResult : class
        {
            var fullUri = UriHelper.BuildFullUri(_serverAddress, partialUri, parameters);
            if (fullUri == null)
            {
                LogMessage(LogLevel.Debug, $"Failed to build uri with addr={_serverAddress} and uri {partialUri}");
                return new RequestResult<TResult>
                {
                    Result = default(TResult),
                    Status = ServerResponseStatus.ClientError
                };
            }

            try
            {
                using (var client = new HttpClient())
                {
                    if (RequestTimeout > 0)
                        client.Timeout = TimeSpan.FromSeconds(RequestTimeout);

                    using (var request = new HttpRequestMessage(method, fullUri))
                    {
                        if (headers != null && headers.Count > 0)
                        {
                            foreach (var header in headers)
                            {
                                request.Headers.Add(header.Key, header.Value);
                            }
                        }
                        authentificationProvider?.Invoke(request);

                        var requestBody = "";
                        if (bodyArg != null)
                        {
                            if (typeof (TBody) != typeof (string))
                            {
                                var bodyStream = bodyArg as Stream;
                                if (bodyStream != null)
                                {
                                    request.Content = new StreamContent(bodyStream);
                                }
                                else
                                {
                                    // We will be unable to serialize request body
                                    if (requestBodySerializer == null)
                                    {
                                        LogMessage(LogLevel.Debug, "Unable to serialize request body");
                                        return new RequestResult<TResult>
                                        {
                                            Result = default(TResult),
                                            Status = ServerResponseStatus.ClientError
                                        };
                                    }
                                    requestBody = requestBodySerializer(bodyArg);
                                    request.Content = new StringContent(requestBody);
                                }
                            }
                            else
                            {
                                requestBody = bodyArg as string;
                                request.Content = new StringContent(requestBody);
                            }
                        }

#if TRACE
                        LogMessage(LogLevel.Trace,
                            $"Sending request:\n{SerializeHttpRequestMessage(request, requestBody)}");
#endif

                        using (var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                        {
                            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

#if TRACE
                            LogMessage(LogLevel.Trace,
                                $"Got response:\n{SerializeHttpResponseMessage(response, responseBody)}");
#endif

                            object userdata = null;
                            ServerResponseStatus status;
                            switch (response.StatusCode)
                            {
                                case HttpStatusCode.OK:
                                case HttpStatusCode.Created:
                                    status = ServerResponseStatus.Ok;
                                    break;
                                case HttpStatusCode.NoContent:
                                    status = ServerResponseStatus.NothingToDo;
                                    break;
                                case HttpStatusCode.Unauthorized:
                                    status = ServerResponseStatus.Unauthorized;
                                    break;
                                case HttpStatusCode.Forbidden:
                                    status = ServerResponseStatus.Forbidden;
                                    break;
                                case HttpStatusCode.Conflict:
                                case HttpStatusCode.BadRequest:
                                case HttpStatusCode.NotFound:
                                    status = ServerResponseStatus.ClientError;
                                    break;
                                case HttpStatusCode.InternalServerError:
                                case HttpStatusCode.NotImplemented:
                                    status = ServerResponseStatus.ServerError;
                                    break;
                                case (HttpStatusCode)429:
                                    status = ServerResponseStatus.TooManyRequests;
                                    userdata = response.Headers.RetryAfter?.Delta;
                                    break;
                                default:
                                    status = ServerResponseStatus.IncompehensibleResponse;
                                    break;
                            }

                            if (responseBodyDeserializer != null)
                            {
                                try
                                {
                                    return new RequestResult<TResult>
                                    {
                                        Result = responseBodyDeserializer(responseBody),
                                        Status = status,
                                        Userdata = userdata
                                    };
                                }
                                catch (Exception ex)
                                {
                                    LogException(LogLevel.Debug, ex);
                                    return new RequestResult<TResult>
                                    {
                                        Result = default(TResult),
                                        Status = ServerResponseStatus.IncompehensibleResponse,
                                        Userdata = userdata
                                    };
                                }
                            }
                            if (typeof (TResult) != typeof (string))
                            {
                                return new RequestResult<TResult>
                                {
                                    Result = default(TResult),
                                    Status = ServerResponseStatus.ClientError,
                                    Userdata = userdata
                                };
                            }
                            return new RequestResult<TResult>
                            {
                                Result = responseBody as TResult,
                                Status = status,
                                Userdata = userdata
                            };
                        }
                    }
                }
            }
            catch (OperationCanceledException ocex)
            {
                LogMessage(LogLevel.Debug, $"Request {method.Method} /{partialUri} has been cancelled");
                return new RequestResult<TResult>
                {
                    Result = default(TResult),
                    Status = ServerResponseStatus.RequestCanceled,
                    Userdata = ocex
                };
            }
            catch (HttpRequestException networkException)
            {
                LogException(LogLevel.Debug, networkException);
                return new RequestResult<TResult>
                {
                    Result = default(TResult),
                    Status = ServerResponseStatus.ConnectionError,
                    Userdata = networkException
                };
            }
            catch (Exception exception)
            {
                LogException(LogLevel.Debug, exception);
                return new RequestResult<TResult>
                {
                    Result = default(TResult),
                    Status = ServerResponseStatus.ClientError,
                    Userdata = exception
                };
            }
        }

        #endregion

        #region get

        /// <summary>
        ///     Basic GET method: returns status and body
        /// </summary>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        public static Task<RequestResult<string>> GetRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return PerformRequestAsync<string, string>(
                HttpMethod.Get,
                partialUri,
                parameters,
                headers,
                null,
                null,
                null,
                authentificationProvider,
                cancellationToken
                );
        }

        /// <summary>
        ///     Methods sends GET and tries to deserialize body
        /// </summary>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="bodyDeserializer">Method to convert string -> TResult</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        public static Task<RequestResult<TResult>> GetDeserializedAsync<TResult>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.BodyDeserializer<TResult> bodyDeserializer = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken)) where TResult : class
        {
            return PerformRequestAsync<string, TResult>(
                HttpMethod.Get,
                partialUri,
                parameters,
                headers,
                null,
                null,
                bodyDeserializer,
                authentificationProvider,
                cancellationToken
                );
        }

        /// <summary>
        ///     Method tries to deserialize response as json
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        public static Task<RequestResult<TResult>> GetAsync<TResult>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken)) where TResult : class
        {
            return PerformRequestAsync<string, TResult>(
                HttpMethod.Get,
                partialUri,
                parameters,
                headers,
                null,
                null,
                JsonConvert.DeserializeObject<TResult>,
                authentificationProvider,
                cancellationToken
                );
        }

        #endregion

        #region post

        /// <summary>
        ///     Basic POST method: posts data and returns status and body
        /// </summary>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="data">Raw request body</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Request status and raw response body</returns>
        public static Task<RequestResult<string>> PostRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return PerformRequestAsync<string, string>(
                HttpMethod.Post,
                partialUri,
                parameters,
                headers,
                data,
                null,
                null,
                authentificationProvider,
                cancellationToken
                );
        }

        /// <summary>
        ///     Posts object and returns deserialized json response
        /// </summary>
        /// <typeparam name="TResult">Response body type</typeparam>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="data">Raw request body</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Response result and deserialized TResult</returns>
        public static Task<RequestResult<TResult>> PostAsync<TResult>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
            where TResult : class
        {
            return PerformRequestAsync(
                HttpMethod.Post,
                partialUri,
                parameters,
                headers,
                data,
                JsonConvert.SerializeObject,
                JsonConvert.DeserializeObject<TResult>,
                authentificationProvider,
                cancellationToken
                );
        }

        public static async Task<RequestResult<object>> PostFileAsync(string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string filePath = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var fileStream = new FileStream(filePath, FileMode.Open);
                return await PerformRequestAsync<FileStream, object>(
                    HttpMethod.Post,
                    partialUri,
                    parameters,
                    headers,
                    fileStream,
                    null,
                    null,
                    authentificationProvider,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                return new RequestResult<object>
                {
                    Status = ServerResponseStatus.ClientError,
                    Result = null
                };
            }
        }

        #endregion

        #region put

        /// <summary>
        ///     Basic PUT method: puts data and returns status and body
        /// </summary>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="data">Raw request body</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Request status and raw response body</returns>
        public static Task<RequestResult<string>> PutRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return PerformRequestAsync<string, string>(
                HttpMethod.Put,
                partialUri,
                parameters,
                headers,
                data,
                null,
                null,
                authentificationProvider,
                cancellationToken
                );
        }

        #endregion

        #region delete

        /// <summary>
        ///     Basic DELETE method: sends data and returns status and body
        /// </summary>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="data">Raw request body</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Request status and raw response body</returns>
        public static Task<RequestResult<string>> DeleteRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return PerformRequestAsync<string, string>(
                HttpMethod.Delete,
                partialUri,
                parameters,
                headers,
                data,
                null,
                null,
                authentificationProvider,
                cancellationToken
                );
        }

        #endregion

        #region security

        /// <summary>
        ///     Allows connections to servers using invalid TLS certificates
        /// </summary>
        public static void AllowUntrustedServerCertificates()
        {
            ServicePointManager.ServerCertificateValidationCallback =
                CertificateController.MockCertificateValidationCallBack;
        }

        /// <summary>
        ///     Perform some real checks
        /// </summary>
        public static void DisallowUntrustedServerCertificates()
        {
            ServicePointManager.ServerCertificateValidationCallback =
                CertificateController.RealCertificateValidationCallBack;
        }

        #endregion

        #region log

        private static void LogMessage(LogLevel level, string message)
        {
            Logger.LogEntry("HTTP CLIENT", level, message);
        }

        private static void LogException(LogLevel level, Exception exception)
        {
            Logger.LogException("HTTP CLIENT", level, exception);
        }

        /// <summary>
        ///     FOR DEBUG ONLY
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string SerializeHttpRequestMessage(HttpRequestMessage request, string requestBody)
        {
            try
            {
                if (request == null)
                    return "NULL";

                var builder = new StringBuilder();
                builder.AppendLine($"{request.Method.ToString().ToUpper()} {request.RequestUri} HTTP/{request.Version}");
                foreach (var header in request.Headers)
                {
                    builder.AppendLine($"{header.Key}: {string.Join("", header.Value)}");
                }

                if (requestBody == null)
                    return builder.ToString();

                builder.AppendLine(requestBody);
                builder.AppendLine();

                return builder.ToString();
            }
            catch (Exception)
            {
                return "ERROR SERIALIZING REQUEST";
            }
        }

        /// <summary>
        ///     FOR DEBUG ONLY
        /// </summary>
        /// <param name="response"></param>
        /// <param name="responseBody"></param>
        /// <returns></returns>
        private static string SerializeHttpResponseMessage(HttpResponseMessage response, string responseBody)
        {
            try
            {
                if (response == null)
                    return null;
                var builder = new StringBuilder();
                builder.AppendLine($"HTTP/{response.Version} {(int) response.StatusCode} {response.ReasonPhrase}");
                foreach (var header in response.Headers)
                {
                    var normalValue = string.Join("", header.Value);
                    builder.AppendLine($"{header.Key}: {normalValue}");
                }

                if (responseBody == null)
                    return builder.ToString();

                builder.AppendLine();
                builder.AppendLine(responseBody);
                return builder.ToString();
            }
            catch (Exception)
            {
                return "ERROR SERIALIZING RESPONSE";
            }
        }

        #endregion

        #region properties

        /// <summary>
        ///     Http(s) server address
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when server address set to null</exception>
        /// <exception cref="ArgumentException">Thrown when value passed to ServerAddress is not a valid address</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if non-http(s) connection is requested</exception>
        public static string ServerAddress
        {
            get { return _serverAddress; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (value.Length < 5)
                    throw new ArgumentException("Server address too short", nameof(value));
                var lower = value.ToLower();
                if (!lower.StartsWith("http"))
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported protocol");

                ServerProtocol = lower[4] == 's' ? Protocol.Https : Protocol.Http;
                _serverAddress = value;
            }
        }

        /// <summary>
        ///     Server request timeout in seconds. Negative values mean infinity
        /// </summary>
        public static double RequestTimeout = -1;

        /// <summary>
        ///     Does the client use SSL/TLS?
        /// </summary>
        public static Protocol ServerProtocol { get; private set; } = Protocol.Http;

        #endregion
    }
}