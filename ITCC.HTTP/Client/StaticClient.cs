using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ITCC.HTTP.Common;
using ITCC.HTTP.Enums;
using ITCC.HTTP.Security;
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
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<RequestResult<TResult>> PerformRequestAsync<TBody, TResult>(
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
            return RegularClient.PerformRequestAsync(method, partialUri, parameters, headers, bodyArg,
                requestBodySerializer, responseBodyDeserializer, authentificationProvider, cancellationToken);
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

        private static void LogException(LogLevel level, Exception exception)
        {
            Logger.LogException("HTTP CLIENT", level, exception);
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

        private static readonly object ClientLock = new object();

        #endregion
    }
}