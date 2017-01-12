// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ITCC.HTTP.API.Utils;
using ITCC.HTTP.Client.Common;
using ITCC.HTTP.Client.Enums;
using ITCC.HTTP.Client.Interfaces;
using ITCC.HTTP.Client.Utils;
using ITCC.HTTP.Common;
using ITCC.HTTP.Common.Enums;
using ITCC.Logging.Core;
using Newtonsoft.Json;

namespace ITCC.HTTP.Client.Core
{
    public class RegularClient
    {
        #region constructors

        public RegularClient(string serverAddress = null)
        {
            if (serverAddress != null)
                ServerAddress = serverAddress;

            ServicePointManager.Expect100Continue = false;
        }

        #endregion

        #region private

        private string _serverAddress = "http://127.0.0.1/";

        private static readonly Dictionary<HttpStatusCode, ServerResponseStatus> ServerResponseStatusDictionary = new Dictionary
            <HttpStatusCode, ServerResponseStatus>
            {
                {HttpStatusCode.OK, ServerResponseStatus.Ok},
                {HttpStatusCode.Created, ServerResponseStatus.Ok},
                {HttpStatusCode.Accepted, ServerResponseStatus.Ok},
                {HttpStatusCode.PartialContent, ServerResponseStatus.Ok},
                {HttpStatusCode.NoContent, ServerResponseStatus.NothingToDo},
                {HttpStatusCode.Unauthorized, ServerResponseStatus.Unauthorized},
                {HttpStatusCode.Forbidden, ServerResponseStatus.Forbidden},
                {HttpStatusCode.BadRequest, ServerResponseStatus.ClientError},
                {HttpStatusCode.NotFound, ServerResponseStatus.ClientError},
                {HttpStatusCode.Conflict, ServerResponseStatus.ClientError},
                {HttpStatusCode.MethodNotAllowed, ServerResponseStatus.ClientError},
                {HttpStatusCode.NotAcceptable, ServerResponseStatus.ClientError},
                {HttpStatusCode.RequestEntityTooLarge, ServerResponseStatus.ClientError},
                {HttpStatusCode.UnsupportedMediaType, ServerResponseStatus.ClientError},
                {HttpStatusCode.RequestedRangeNotSatisfiable, ServerResponseStatus.ClientError},
                {(HttpStatusCode) 429, ServerResponseStatus.TooManyRequests},
                {HttpStatusCode.InternalServerError, ServerResponseStatus.ServerError},
                {HttpStatusCode.NotImplemented, ServerResponseStatus.ServerError},
                {HttpStatusCode.ServiceUnavailable, ServerResponseStatus.TemporaryUnavailable},
                {HttpStatusCode.MovedPermanently, ServerResponseStatus.Redirect},
                {HttpStatusCode.Found, ServerResponseStatus.Redirect}
            };

        #endregion

        #region general

        /// <summary>
        ///     Most general http method
        /// </summary>
        /// <typeparam name="TBody">Request body type. Use object for empty body</typeparam>
        /// <typeparam name="TResult">Response body type. Use object for empty body</typeparam>
        /// <param name="method">Request method (GET/POST...)</param>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="bodyArg">Object to be serialized to request body</param>
        /// <param name="authentificationProvider">Method to add authentification data</param>
        /// <param name="outputStream">If not null, response body will be copied to this stream</param>
        /// <param name="redirectsLeft">How many times client is allowed to follow redirects</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once FunctionComplexityOverflow
        internal async Task<RequestResult<TResult>> PerformRequestAsync<TBody, TResult>(
            HttpMethod method,
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            TBody bodyArg = default(TBody),
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            Stream outputStream = null,
            int redirectsLeft = 0,
            CancellationToken cancellationToken = default(CancellationToken))
            where TResult : class
            where TBody : class
        {
            var variadicResult = await PerformVariadicRequestAsync<TBody, TResult, NoError>(
                method,
                partialUri,
                parameters,
                headers,
                bodyArg,
                authentificationProvider,
                outputStream,
                redirectsLeft,
                cancellationToken);

            return variadicResult.Exception != null
                ? new RequestResult<TResult>(variadicResult.Success, variadicResult.Status, variadicResult.Exception)
                : new RequestResult<TResult>(variadicResult.Success, variadicResult.Status, variadicResult.Headers);
        }

        /// <summary>
        ///     Most general http method for two response options
        /// </summary>
        /// <typeparam name="TRequestBody">Request body type. Use object for empty body</typeparam>
        /// <typeparam name="TResponseSuccess">Success response body type. Use object for empty body</typeparam>
        /// <typeparam name="TResponseError">Error response body type. Use object for empty body</typeparam>
        /// <param name="method">Request method (GET/POST...)</param>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="bodyArg">Object to be serialized to request body</param>
        /// <param name="authentificationProvider">Method to add authentification data</param>
        /// <param name="outputStream">If not null, response body will be copied to this stream</param>
        /// <param name="redirectsLeft">How many times client is allowed to follow redirects</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once FunctionComplexityOverflow
        internal async Task<VariadicRequestResult<TResponseSuccess, TResponseError>> PerformVariadicRequestAsync
            <TRequestBody, TResponseSuccess, TResponseError>(
                HttpMethod method,
                string partialUri,
                IDictionary<string, string> parameters = null,
                IDictionary<string, string> headers = null,
                TRequestBody bodyArg = default(TRequestBody),
                Delegates.AuthentificationDataAdder authentificationProvider = null,
                Stream outputStream = null,
                int redirectsLeft = 0,
                CancellationToken cancellationToken = default(CancellationToken))
            where TRequestBody : class
            where TResponseSuccess : class
            where TResponseError : class
        {
            var fullUri = UriHelper.BuildFullUri(_serverAddress, partialUri, parameters);
            if (fullUri == null)
            {
                LogDebug($"Failed to build uri with addr={_serverAddress} and uri {partialUri}");
                return new VariadicRequestResult<TResponseSuccess, TResponseError>(default(TResponseError),
                    ServerResponseStatus.ClientError);
            }

            try
            {
                using (var client = new HttpClient(new HttpClientHandler
                {
                    AutomaticDecompression = _decompressionMethods,
                    AllowAutoRedirect = false
                }))
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
                            if (typeof(TRequestBody) != typeof(string))
                            {
                                var bodyStream = bodyArg as Stream;
                                if (bodyStream != null)
                                {
                                    request.Content = new StreamContent(bodyStream);
                                }
                                else
                                {
                                    requestBody = _bodySerializer.Serialize(bodyArg);
                                    request.Content = new StringContent(requestBody, _bodySerializer.Encoding, _bodySerializer.ContentType);
                                }
                            }
                            else
                            {
                                requestBody = bodyArg as string;
                                request.Content = new StringContent(requestBody);
                            }
                        }
                        foreach (var contentType in SupportedContentTypes)
                        {
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
                        }

                        LogTrace($"Sending request:\n{SerializeHttpRequestMessage(request, requestBody)}");
                        using (var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                        {
                            var responseHeaders =
                                response.Headers.ToDictionary(httpResponseHeader => httpResponseHeader.Key,
                                    httpResponseHeader => string.Join(";", httpResponseHeader.Value));

                            var status = ServerResponseStatusDictionary.ContainsKey(response.StatusCode)
                                ? ServerResponseStatusDictionary[response.StatusCode]
                                : ServerResponseStatus.IncompehensibleResponse;

                            if (status == ServerResponseStatus.Redirect)
                            {
                                if (redirectsLeft <= 0)
                                {
                                    return
                                        new VariadicRequestResult<TResponseSuccess, TResponseError>(
                                            default(TResponseSuccess), ServerResponseStatus.Redirect, responseHeaders);
                                }
                                if (method == HttpMethod.Head || method == HttpMethod.Get)
                                {
                                    if (!responseHeaders.ContainsKey("Location"))
                                    {
                                        return
                                            new VariadicRequestResult<TResponseSuccess, TResponseError>(
                                                default(TResponseError),
                                                ServerResponseStatus.IncompehensibleResponse, responseHeaders);
                                    }
                                    var redirectLocation = responseHeaders["Location"];
                                    if (!redirectLocation.StartsWith(_serverAddress) && !AllowRedirectHostChange)
                                    {
                                        return
                                            new VariadicRequestResult<TResponseSuccess, TResponseError>(
                                                default(TResponseSuccess),
                                                ServerResponseStatus.Redirect, responseHeaders);
                                    }

                                    Delegates.AuthentificationDataAdder redirectAuthProvider = null;
                                    if (PreserveAuthorizationOnRedirect)
                                        redirectAuthProvider = authentificationProvider;

                                    return
                                        await
                                            PerformVariadicRequestAsync<TRequestBody, TResponseSuccess, TResponseError>(
                                                method,
                                                redirectLocation,
                                                parameters,
                                                headers,
                                                bodyArg,
                                                redirectAuthProvider,
                                                outputStream,
                                                redirectsLeft - 1,
                                                cancellationToken).ConfigureAwait(false);
                                }
                                return
                                    new VariadicRequestResult<TResponseSuccess, TResponseError>(
                                        default(TResponseSuccess), ServerResponseStatus.Redirect, responseHeaders);
                            }

                            if (outputStream != null)
                            {
                                var result =
                                    new VariadicRequestResult<TResponseSuccess, TResponseError>(
                                        default(TResponseSuccess), status, responseHeaders);
                                try
                                {
                                    if (response.Content != null)
                                        await response.Content.CopyToAsync(outputStream);
                                }
                                catch (Exception ex)
                                {
                                    LogException(LogLevel.Warning, ex);
                                    result.Status = ServerResponseStatus.ClientError;
                                    result.Exception = ex;
                                }

                                return result;
                            }

                            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                            LogTrace($"Got response:\n{SerializeHttpResponseMessage(response, responseBody)}");
                            if (typeof(TResponseSuccess) == typeof(string))
                            {
                                try
                                {
                                    var errorDeserializer = SelectDeserializer<TResponseError>(responseHeaders);
                                    if (errorDeserializer == null)
                                    {
                                        return new VariadicRequestResult<TResponseSuccess, TResponseError>(responseBody as TResponseSuccess,
                                            status, responseHeaders);
                                    }
                                    return new VariadicRequestResult<TResponseSuccess, TResponseError>(
                                        errorDeserializer(responseBody), status, responseHeaders);
                                }
                                catch (Exception)
                                {
                                    return new VariadicRequestResult<TResponseSuccess, TResponseError>(responseBody as TResponseSuccess,
                                        status, responseHeaders);
                                }
                            }

                            var successDeserializer = SelectDeserializer<TResponseSuccess>(responseHeaders);
                            if (successDeserializer != null)
                            {
                                try
                                {
                                    return new VariadicRequestResult<TResponseSuccess, TResponseError>(
                                        successDeserializer(responseBody), status, responseHeaders);
                                }
                                catch (Exception ex)
                                {
                                    LogExceptionDebug(ex);
                                    try
                                    {
                                        var errorDeserializer = SelectDeserializer<TResponseError>(responseHeaders);
                                        if (errorDeserializer == null)
                                        {
                                            return new VariadicRequestResult<TResponseSuccess, TResponseError>(default(TResponseError),
                                                ServerResponseStatus.IncompehensibleResponse, responseHeaders);
                                        }
                                        return new VariadicRequestResult<TResponseSuccess, TResponseError>(
                                            errorDeserializer(responseBody), status, responseHeaders);
                                    }
                                    catch (Exception innerException)
                                    {
                                        LogExceptionDebug(innerException);
                                        return new VariadicRequestResult<TResponseSuccess, TResponseError>(default(TResponseError),
                                            ServerResponseStatus.IncompehensibleResponse, responseHeaders);
                                    }
                                }
                            }
                            if (typeof(TResponseSuccess) != typeof(string) && typeof(TResponseSuccess) != typeof(object))
                            {
                                return new VariadicRequestResult<TResponseSuccess, TResponseError>(default(TResponseError),
                                        ServerResponseStatus.ClientError, responseHeaders);
                            }
                            return new VariadicRequestResult<TResponseSuccess, TResponseError>(responseBody as TResponseSuccess,
                                        status, responseHeaders);
                        }
                    }
                }
            }
            catch (OperationCanceledException ocex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    LogDebug($"Request {method.Method} /{partialUri} has been cancelled");
                    return new VariadicRequestResult<TResponseSuccess, TResponseError>(default(TResponseError), ServerResponseStatus.RequestCancelled, ocex);
                }
                LogDebug($"Request {method.Method} /{partialUri} has been timed out ({RequestTimeout} seconds)");
                return new VariadicRequestResult<TResponseSuccess, TResponseError>(default(TResponseError), ServerResponseStatus.RequestTimeout, ocex);
            }
            catch (HttpRequestException networkException)
            {
                LogExceptionDebug(networkException);
                return new VariadicRequestResult<TResponseSuccess, TResponseError>(default(TResponseError), ServerResponseStatus.ConnectionError, networkException);
            }
            catch (Exception exception)
            {
                LogExceptionDebug(exception);
                return new VariadicRequestResult<TResponseSuccess, TResponseError>(default(TResponseError), ServerResponseStatus.ClientError, exception);
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
        public Task<RequestResult<string>> GetRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                => PerformRequestAsync<string, string>(
                    HttpMethod.Get,
                    partialUri,
                    parameters,
                    headers,
                    null,
                    authentificationProvider,
                    null,
                    AllowedRedirectCount,
                    cancellationToken
                    );

        /// <summary>
        ///     Method tries to deserialize response as TResult
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        public Task<RequestResult<TResult>> GetAsync<TResult>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken)) where TResult : class
                => PerformRequestAsync<string, TResult>(
                    HttpMethod.Get,
                    partialUri,
                    parameters,
                    headers,
                    null,
                    authentificationProvider,
                    null,
                    AllowedRedirectCount,
                    cancellationToken
                    );

        /// <summary>
        ///     Method tries to deserialize response as TSuccess and then as TError
        /// </summary>
        /// <typeparam name="TSuccess">Success response type</typeparam>
        /// <typeparam name="TError">Error response type</typeparam>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        public Task<VariadicRequestResult<TSuccess, TError>> GetVariadicAsync<TSuccess, TError>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
            where TSuccess : class
            where TError : class 
            => PerformVariadicRequestAsync<string, TSuccess, TError>(
                HttpMethod.Get,
                partialUri,
                parameters,
                headers,
                null,
                authentificationProvider,
                null,
                AllowedRedirectCount,
                cancellationToken
                );

        /// <summary>
        ///     Method tries to deserialize response as TSuccess and then as ApiErrorView
        /// </summary>
        /// <typeparam name="TSuccess">Success response type</typeparam>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        public Task<VariadicRequestResult<TSuccess, ApiErrorView>> GetWithApiErrorAsync<TSuccess>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
            where TSuccess : class
            => GetVariadicAsync<TSuccess, ApiErrorView>(
                partialUri,
                parameters,
                headers,
                authentificationProvider,
                cancellationToken
                );

        /// <summary>
        ///     Downloads file by uri into specified location
        /// </summary>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="fileName">Target file location. If null, then uri part after last slash will be used</param>
        /// <param name="allowRewrite">If false, ClientError will be returned if file exists</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        public async Task<RequestResult<string>> GetFileAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string fileName = null,
            bool allowRewrite = true,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                string realFileName;
                if (fileName == null)
                {
                    if (!partialUri.Contains("/"))
                        realFileName = partialUri;
                    else
                    {
                        var lastSlashIndex = partialUri.LastIndexOf("/", StringComparison.Ordinal);
                        realFileName = partialUri.Remove(0, lastSlashIndex + 1);
                    }
                }
                else
                {
                    realFileName = fileName;
                }

                using (var fileStream = new FileStream(realFileName, allowRewrite ? FileMode.Create : FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    return await PerformRequestAsync<string, string>(HttpMethod.Get,
                        partialUri,
                        parameters,
                        headers,
                        null,
                        authentificationProvider,
                        fileStream,
                        AllowedRedirectCount,
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                return new RequestResult<string>(null, ServerResponseStatus.ClientError, ex);
            }
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
        public Task<RequestResult<string>> PostRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                => PerformRequestAsync<string, string>(
                    HttpMethod.Post,
                    partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    null,
                    AllowedRedirectCount,
                    cancellationToken
                    );

        /// <summary>
        ///     Posts object and returns deserialized TResult
        /// </summary>
        /// <typeparam name="TResult">Response body type</typeparam>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="data">Raw request body</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Response result and deserialized body</returns>
        public Task<RequestResult<TResult>> PostAsync<TResult>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                where TResult : class
                => PerformRequestAsync<object, TResult>(
                    HttpMethod.Post,
                    partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    null,
                    AllowedRedirectCount,
                    cancellationToken
                    );

        /// <summary>
        ///     Posts object and returns deserialized TSuccess or TError
        /// </summary>
        /// <typeparam name="TSuccess">Success response type</typeparam>
        /// <typeparam name="TError">Error response type</typeparam>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="data">Raw request body</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Response result and deserialized body</returns>
        public Task<VariadicRequestResult<TSuccess, TError>> PostVariadicAsync<TSuccess, TError>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                where TSuccess : class
                where TError : class
                => PerformVariadicRequestAsync<object, TSuccess, TError>(
                    HttpMethod.Post,
                    partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    null,
                    AllowedRedirectCount,
                    cancellationToken
                    );

        /// <summary>
        ///     Posts object and returns deserialized TSuccess or ApiErrorView
        /// </summary>
        /// <typeparam name="TSuccess">Success response type</typeparam>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="data">Raw request body</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Response result and deserialized body</returns>
        public Task<VariadicRequestResult<TSuccess, ApiErrorView>> PostWithApiErrorAsync<TSuccess>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                where TSuccess : class
                => PostVariadicAsync<TSuccess, ApiErrorView>(
                    partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    cancellationToken
                    );

        /// <summary>
        ///     Posts file's content to specified uri
        /// </summary>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="filePath">File to post</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        public async Task<RequestResult<string>> PostFileAsync(string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string filePath = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return await PerformRequestAsync<FileStream, string>(
                        HttpMethod.Post,
                        partialUri,
                        parameters,
                        headers,
                        fileStream,
                        authentificationProvider,
                        null,
                        AllowedRedirectCount,
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                return new RequestResult<string>
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
        public Task<RequestResult<string>> PutRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                => PerformRequestAsync<string, string>(
                    HttpMethod.Put,
                    partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    null,
                    AllowedRedirectCount,
                    cancellationToken
                    );

        /// <summary>
        ///     Puts object and returns success string or TError
        /// </summary>
        /// <typeparam name="TError">Error response type</typeparam>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="data">Raw request body</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Response result and deserialized body</returns>
        public Task<VariadicRequestResult<string, TError>> PutVariadicAsync<TError>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                where TError : class
                => PerformVariadicRequestAsync<object, string, TError>(
                    HttpMethod.Put,
                    partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    null,
                    AllowedRedirectCount,
                    cancellationToken
                    );

        /// <summary>
        ///     Puts object and returns deserialized TSuccess or ApiErrorView
        /// </summary>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="data">Raw request body</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Response result and deserialized body</returns>
        public Task<VariadicRequestResult<string, ApiErrorView>> PutWithApiErrorAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                => PutVariadicAsync<ApiErrorView>(
                    partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    cancellationToken
                    );

        /// <summary>
        ///     Puts file's content to specified uri
        /// </summary>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="filePath">File to post</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        public async Task<RequestResult<string>> PutFileAsync(string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string filePath = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return await PerformRequestAsync<FileStream, string>(
                        HttpMethod.Put,
                        partialUri,
                        parameters,
                        headers,
                        fileStream,
                        authentificationProvider,
                        null,
                        AllowedRedirectCount,
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                LogException(LogLevel.Warning, ex);
                return new RequestResult<string>
                {
                    Status = ServerResponseStatus.ClientError,
                    Result = null
                };
            }
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
        public Task<RequestResult<string>> DeleteRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                => PerformRequestAsync<string, string>(
                    HttpMethod.Delete,
                    partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    null,
                    AllowedRedirectCount,
                    cancellationToken
                    );

        /// <summary>
        ///     Deletes object and returns success string or TError
        /// </summary>
        /// <typeparam name="TError">Error response type</typeparam>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="data">Raw request body</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Response result and deserialized body</returns>
        public Task<VariadicRequestResult<string, TError>> DeleteVariadicAsync<TError>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                where TError : class
                => PerformVariadicRequestAsync<object, string, TError>(
                    HttpMethod.Delete,
                    partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    null,
                    AllowedRedirectCount,
                    cancellationToken
                    );

        /// <summary>
        ///     Deletes object and returns deserialized TSuccess or ApiErrorView
        /// </summary>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="data">Raw request body</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Response result and deserialized body</returns>
        public Task<VariadicRequestResult<string, ApiErrorView>> DeleteWithApiErrorAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                => DeleteVariadicAsync<ApiErrorView>(
                    partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    cancellationToken
                    );

        #endregion

        #region security

        /// <summary>
        ///     Allows connections to servers using invalid TLS certificates
        /// </summary>
        public void AllowUntrustedServerCertificates()
        {
            ServicePointManager.ServerCertificateValidationCallback =
                CertificateController.MockCertificateValidationCallBack;
        }

        /// <summary>
        ///     Perform some real checks
        /// </summary>
        public void DisallowUntrustedServerCertificates()
        {
            ServicePointManager.ServerCertificateValidationCallback =
                CertificateController.RealCertificateValidationCallBack;
        }

        #endregion

        #region private

        #region log

        [Conditional("DEBUG")]
        private static void LogTrace(string message) => Logger.LogTrace("HTTP CLIENT", message);

        [Conditional("DEBUG")]
        private static void LogDebug(string message) => Logger.LogDebug("HTTP CLIENT", message);

        private static void LogException(LogLevel level, Exception exception) => Logger.LogException("HTTP CLIENT", level, exception);

        private static void LogExceptionDebug(Exception exception) => Logger.LogExceptionDebug("HTTP CLIENT", exception);

        #endregion

        #region serialization

        /// <summary>
        ///     FOR DEBUG ONLY
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        private string SerializeHttpRequestMessage(HttpRequestMessage request, string requestBody)
        {
            try
            {
                if (request == null)
                    return "NULL";

                var builder = new StringBuilder();
                builder.AppendLine($"{request.Method.ToString().ToUpper()} {PreprocessStatusLine(request.RequestUri.ToString())} HTTP/{request.Version}");
                foreach (var header in request.Headers)
                {
                    if (LogProhibitedHeaders.Contains(header.Key))
                    {
                        builder.AppendLine($"{header.Key}: {Constants.RemovedLogString}");
                    }
                    else
                    {
                        var normalValue = string.Join(", ", header.Value);
                        builder.AppendLine($"{header.Key}: {normalValue}");
                    }
                }
                if (request.Content?.Headers != null)
                {
                    foreach (var header in request.Content.Headers)
                    {
                        var normalValue = string.Join(", ", header.Value);
                        builder.AppendLine($"{header.Key}: {normalValue}");
                    }
                }

                if (requestBody == null)
                    return builder.ToString();

                builder.AppendLine();
                var processedResponseBody = requestBody;
                foreach (var logBodyReplacePattern in LogBodyReplacePatterns)
                {
                    processedResponseBody = Regex.Replace(processedResponseBody, logBodyReplacePattern.Item1,
                        logBodyReplacePattern.Item2);
                }

                builder.AppendLine(processedResponseBody);
                builder.AppendLine();

                return builder.ToString();
            }
            catch (Exception)
            {
                return "ERROR SERIALIZING REQUEST";
            }
        }

        private string PreprocessStatusLine(string statusLine)
        {
            if (string.IsNullOrEmpty(statusLine))
                return statusLine;

            return Regex.Replace(statusLine, "password=\\w+", $"password={Constants.RemovedLogString}");
        }

        /// <summary>
        ///     FOR DEBUG ONLY
        /// </summary>
        /// <param name="response"></param>
        /// <param name="responseBody"></param>
        /// <returns></returns>
        private string SerializeHttpResponseMessage(HttpResponseMessage response, string responseBody)
        {
            try
            {
                if (response == null)
                    return null;
                var builder = new StringBuilder();
                builder.AppendLine($"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}");
                foreach (var header in response.Headers)
                {
                    if (LogProhibitedHeaders.Contains(header.Key))
                    {
                        builder.AppendLine($"{header.Key}: {Constants.RemovedLogString}");
                    }
                    else
                    {
                        var normalValue = string.Join("", header.Value);
                        builder.AppendLine($"{header.Key}: {normalValue}");
                    }
                }

                if (responseBody == null)
                    return builder.ToString();

                builder.AppendLine();
                var processedResponseBody = responseBody;
                foreach (var logBodyReplacePattern in LogBodyReplacePatterns)
                {
                    processedResponseBody = Regex.Replace(processedResponseBody, logBodyReplacePattern.Item1,
                        logBodyReplacePattern.Item2);
                }

                builder.AppendLine(processedResponseBody);
                return builder.ToString();
            }
            catch (Exception)
            {
                return "ERROR SERIALIZING RESPONSE";
            }
        }

        #endregion

        #region decoders

        private static TType DeserializeJson<TType>(string rawData) => JsonConvert.DeserializeObject<TType>(rawData);

        private static TType DeserializeXml<TType>(string rawData)
        {
            var serializer = new XmlSerializer(typeof(TType));
            using (TextReader reader = new StringReader(rawData))
            {
                return (TType)serializer.Deserialize(reader);
            }
        }

        private Func<string, TType> SelectDeserializer<TType>(IDictionary<string, string> responseHeaders)
        {
            string contentType;
            if (!responseHeaders.TryGetValue("Content-Type", out contentType) || string.IsNullOrWhiteSpace(contentType))
                return GetDefaultDecoder<TType>();

            if (contentType.StartsWith("application/json", StringComparison.InvariantCultureIgnoreCase))
                return DeserializeJson<TType>;
            if (contentType.StartsWith("application/xml", StringComparison.InvariantCultureIgnoreCase))
                return DeserializeXml<TType>;
            return null;
        }

        private Func<string, TType> GetDefaultDecoder<TType>()
        {
            switch (DefaultContentType)
            {
                case ContentType.Json:
                    return DeserializeJson<TType>;
                case ContentType.Xml:
                    return DeserializeXml<TType>;
                default:
                    return null;
            }
        }

        private static readonly List<string> SupportedContentTypes = new List<string>
        {
            "application/json",
            "application/xml"
        };

        #endregion

        #endregion

        #region properties

        /// <summary>
        ///     Request body serializer
        /// </summary>
        public IBodySerializer BodySerializer
        {
            get { return _bodySerializer; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _bodySerializer = value;
            }
        }

        /// <summary>
        ///     We assume responses have this content type by default
        /// </summary>
        public ContentType DefaultContentType { get; set; } = ContentType.Json;

        /// <summary>
        ///     Http(s) server address
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when server address set to null</exception>
        /// <exception cref="ArgumentException">Thrown when value passed to ServerAddress is not a valid address</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if non-http(s) connection is requested</exception>
        public string ServerAddress
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
        public double RequestTimeout { get; set; } = -1;

        /// <summary>
        ///     Does the client use SSL/TLS?
        /// </summary>
        public Protocol ServerProtocol { get; private set; } = Protocol.Http;

        /// <summary>
        ///     If true, client will send Accept-Encoding and decompress responses automatically
        /// </summary>
        public bool AllowGzipEncoding
        {
            get { return _decompressionMethods != DecompressionMethods.None; }
            set
            {
                if (value)
                    _decompressionMethods = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                else
                {
                    _decompressionMethods = DecompressionMethods.None;
                }
            }
        }

        /// <summary>
        ///     How many times client is allowed to follow Location: headers in case of 3xx responses. Defaults to 1.
        ///     Note: client will NEVER follow redirects if methods is not GET or HEAD. Redirect will be returned explicitely
        /// </summary>
        public int AllowedRedirectCount { get; set; } = 1;

        /// <summary>
        ///     If true, 3xx codes may cause sudden host change
        /// </summary>
        public bool AllowRedirectHostChange { get; set; } = false;

        /// <summary>
        ///     If true, the same authorization procedure will be used for every redirect.
        /// </summary>
        public bool PreserveAuthorizationOnRedirect { get; set; } = true;

        /// <summary>
        ///     Tuples thing-to-replace thing-for-replacement for request and response bodies
        /// </summary>
        public List<Tuple<string, string>> LogBodyReplacePatterns { get; } = new List<Tuple<string, string>>();
        /// <summary>
        ///     Headers' names. Their values will be replaced with non-critical info
        /// </summary>
        public List<string> LogProhibitedHeaders { get; } = new List<string>();

        private DecompressionMethods _decompressionMethods = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        private IBodySerializer _bodySerializer = new JsonBodySerializer();

        #endregion
    }
}
