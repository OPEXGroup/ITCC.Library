using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ITCC.HTTP.Client.Common;
using ITCC.HTTP.Client.Utils;
using ITCC.HTTP.Common.Enums;

namespace ITCC.HTTP.Client
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
            Delegates.BodySerializer requestBodySerializer = null,
            Delegates.BodyDeserializer<TResult> responseBodyDeserializer = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            Stream outputStream = null,
            CancellationToken cancellationToken = default(CancellationToken)) where TResult : class => RegularClient.PerformRequestAsync(method, partialUri, parameters, headers, bodyArg,
                requestBodySerializer, responseBodyDeserializer, authentificationProvider, outputStream, RegularClient.AllowedRedirectCount, cancellationToken);

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
                => RegularClient.GetDeserializedAsync(partialUri,
                    parameters,
                    headers,
                    bodyDeserializer,
                    authentificationProvider,
                    cancellationToken);

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
                => RegularClient.GetAsync<TResult>(partialUri,
                    parameters,
                    headers,
                    authentificationProvider,
                    cancellationToken);

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