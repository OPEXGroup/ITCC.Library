using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ITCC.HTTP.API.Utils;
using ITCC.HTTP.Client.Common;
using ITCC.HTTP.Client.Interfaces;
using ITCC.HTTP.Client.Utils;
using ITCC.HTTP.Common.Enums;

namespace ITCC.HTTP.Client.Core
{
    /// <summary>
    ///     Represents static Http client (no instances, single server)
    /// </summary>
    public static class StaticClient
    {
        #region private

        private static readonly RegularClient RegularClient = new RegularClient("http://127.0.0.1/");

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
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Task<RequestResult<TResult>> PerformRequestAsync<TBody, TResult>(
            HttpMethod method,
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            TBody bodyArg = default(TBody),
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            Stream outputStream = null,
            CancellationToken cancellationToken = default(CancellationToken))
                where TResult : class
                where TBody : class
                => RegularClient.PerformRequestAsync<TBody, TResult>(method,
                    partialUri,
                    parameters,
                    headers,
                    bodyArg,
                    authentificationProvider,
                    outputStream,
                    RegularClient.AllowedRedirectCount,
                    cancellationToken);

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
        internal static Task<VariadicRequestResult<TResponseSuccess, TResponseError>> PerformVariadicRequestAsync
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
        => RegularClient.PerformVariadicRequestAsync<TRequestBody, TResponseSuccess, TResponseError>(
            method,
            partialUri,
            parameters,
            headers,
            bodyArg,
            authentificationProvider,
            outputStream,
            redirectsLeft,
            cancellationToken);
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
                => RegularClient.GetRawAsync(partialUri,
                    parameters,
                    headers,
                    authentificationProvider,
                    cancellationToken);

        /// <summary>
        ///     Methods sends GET and tries to deserialize body
        /// </summary>
        /// <typeparam name="TResult">Result object type</typeparam>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        public static Task<RequestResult<TResult>> GetDeserializedAsync<TResult>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                where TResult : class
                => RegularClient.GetDeserializedAsync<TResult>(partialUri,
                    parameters,
                    headers,
                    authentificationProvider,
                    cancellationToken);

        /// <summary>
        ///     Method tries to deserialize response as TResult or TError
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
                => RegularClient.GetAsync<TResult>(partialUri,
                    parameters,
                    headers,
                    authentificationProvider,
                    cancellationToken);

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
        public static Task<VariadicRequestResult<TSuccess, TError>> GetVariadicAsync<TSuccess, TError>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
            where TSuccess : class
            where TError : class
            => RegularClient.GetVariadicAsync<TSuccess, TError>(
                partialUri,
                parameters,
                headers,
                authentificationProvider,
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
        public static Task<VariadicRequestResult<TSuccess, ApiErrorView>> GetWithApiErrorAsync<TSuccess>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
            where TSuccess : class
            => RegularClient.GetWithApiErrorAsync<TSuccess>(
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
        public static Task<RequestResult<string>> GetFileAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string fileName = null,
            bool allowRewrite = true,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                => RegularClient.GetFileAsync(partialUri,
                    parameters,
                    headers,
                    fileName,
                    allowRewrite,
                    authentificationProvider,
                    cancellationToken);

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
                => RegularClient.PostRawAsync(partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    cancellationToken);

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
            CancellationToken cancellationToken = default(CancellationToken)) where TResult : class
                => RegularClient.PostAsync<TResult>(partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    cancellationToken);

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
        /// <returns>Response result and deserialized TResult</returns>
        public static Task<VariadicRequestResult<TSuccess, TError>> PostVariadicAsync<TSuccess, TError>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                where TSuccess : class
                where TError : class
                => RegularClient.PostVariadicAsync<TSuccess, TError>(
                    partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
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
        /// <returns>Response result and deserialized TResult</returns>
        public static Task<VariadicRequestResult<TSuccess, ApiErrorView>> PostWithApiErrorAsync<TSuccess>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                where TSuccess : class
                => RegularClient.PostWithApiErrorAsync<TSuccess>(
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
        public static Task<RequestResult<string>> PostFileAsync(string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string filePath = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                => RegularClient.PostFileAsync(partialUri,
                    parameters,
                    headers,
                    filePath,
                    authentificationProvider,
                    cancellationToken);

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
                => RegularClient.PutRawAsync(partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    cancellationToken);

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
        /// <returns>Response result and deserialized TResult</returns>
        public static Task<VariadicRequestResult<string, TError>> PutVariadicAsync<TError>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
            where TError : class
        => RegularClient.PutVariadicAsync<TError>(
            partialUri,
            parameters,
            headers,
            data,
            authentificationProvider,
            cancellationToken);

        /// <summary>
        ///     Puts object and returns deserialized TSuccess or ApiErrorView
        /// </summary>
        /// <param name="partialUri">Uri part after server address/fqdn and port</param>
        /// <param name="parameters">Request parameters after `?`</param>
        /// <param name="headers">Request headers</param>
        /// <param name="data">Raw request body</param>
        /// <param name="authentificationProvider">Authentification provider</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Response result and deserialized TResult</returns>
        public static Task<VariadicRequestResult<string, ApiErrorView>> PutWithApiErrorAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                => RegularClient.PutVariadicAsync<ApiErrorView>(
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
        public static Task<RequestResult<string>> PutFileAsync(string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string filePath = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                => RegularClient.PutFileAsync(partialUri,
                    parameters,
                    headers,
                    filePath,
                    authentificationProvider,
                    cancellationToken);

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
                => RegularClient.DeleteRawAsync(partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
                    cancellationToken);

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
        /// <returns>Response result and deserialized TResult</returns>
        public static Task<VariadicRequestResult<string, TError>> DeleteVariadicAsync<TError>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                where TError : class
                => RegularClient.DeleteVariadicAsync<TError>(
                    partialUri,
                    parameters,
                    headers,
                    data,
                    authentificationProvider,
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
        /// <returns>Response result and deserialized TResult</returns>
        public static Task<VariadicRequestResult<string, ApiErrorView>> DeleteWithApiErrorAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
                => RegularClient.DeleteWithApiErrorAsync(
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
        public static void AllowUntrustedServerCertificates() => RegularClient.AllowUntrustedServerCertificates();

        /// <summary>
        ///     Perform some real checks
        /// </summary>
        public static void DisallowUntrustedServerCertificates() => RegularClient.DisallowUntrustedServerCertificates();

        #endregion

        #region properties

        /// <summary>
        ///     Request body serializer
        /// </summary>
        public static IBodySerializer BodySerializer
        {
            get
            {
                lock (ClientLock)
                {
                    return RegularClient.BodySerializer;
                }
            }
            set
            {
                lock (ClientLock)
                {
                    RegularClient.BodySerializer = value;
                }
            }
        }

        /// <summary>
        ///     We assume responses have this content type by default
        /// </summary>
        public static ContentType DefaultContentType
        {
            get
            {
                lock (ClientLock)
                {
                    return RegularClient.DefaultContentType;
                }
            }
            set
            {
                lock (ClientLock)
                {
                    RegularClient.DefaultContentType = value;
                }
            }
        }

        /// <summary>
        ///     Http(s) server address
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when server address set to null</exception>
        /// <exception cref="ArgumentException">Thrown when value passed to ServerAddress is not a valid address</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if non-http(s) connection is requested</exception>
        public static string ServerAddress
        {
            get
            {
                lock (ClientLock)
                {
                    return RegularClient.ServerAddress;
                }
            }
            set
            {
                lock (ClientLock)
                {
                    RegularClient.ServerAddress = value;
                }
            }
        }

        /// <summary>
        ///     Server request timeout in seconds. Negative values mean infinity
        /// </summary>
        public static double RequestTimeout
        {
            get
            {
                lock (ClientLock)
                {
                    return RegularClient.RequestTimeout;
                }
            }
            set
            {
                lock (ClientLock)
                {
                    RegularClient.RequestTimeout = value;
                }
            }
        }

        /// <summary>
        ///     Does the client use SSL/TLS?
        /// </summary>
        public static Protocol ServerProtocol => RegularClient.ServerProtocol;

        /// <summary>
        ///     If true, client will send Accept-Encoding and decompress responses automatically
        /// </summary>
        public static bool AllowGzipEncoding
        {
            get
            {
                lock (ClientLock)
                {
                    return RegularClient.AllowGzipEncoding;
                }
            }
            set
            {
                lock (ClientLock)
                {
                    RegularClient.AllowGzipEncoding = value;
                }
            }
        }

        /// <summary>
        ///     How many times client is allowed to follow Location: headers in case of 3xx responses. Defaults to 1.
        ///     Note: client will NEVER follow redirects if methods is not GET or HEAD. Redirect will be returned explicitely
        /// </summary>
        public static int AllowedRedirectCount
        {
            get
            {
                lock (ClientLock)
                {
                    return RegularClient.AllowedRedirectCount;
                }
            }
            set
            {
                lock (ClientLock)
                {
                    RegularClient.AllowedRedirectCount = value;
                }
            }
        }

        /// <summary>
        ///     If true, the same authorization procedure will be used for every redirect.
        /// </summary>
        public static bool PreserveAuthorizationOnRedirect
        {
            get
            {
                lock (ClientLock)
                {
                    return RegularClient.PreserveAuthorizationOnRedirect;
                }
            }
            set
            {
                lock (ClientLock)
                {
                    RegularClient.PreserveAuthorizationOnRedirect = value;
                }
            }
        }

        /// <summary>
        ///     Tuples thing-to-replace thing-for-replacement for request and response bodies
        /// </summary>
        public static List<Tuple<string, string>> LogBodyReplacePatterns => RegularClient.LogBodyReplacePatterns;
        /// <summary>
        ///     Headers' names. Their values will be replaced with non-critical info
        /// </summary>
        public static List<string> LogProhibitedHeaders => RegularClient.LogProhibitedHeaders;

        private static readonly object ClientLock = new object();

        #endregion
    }
}