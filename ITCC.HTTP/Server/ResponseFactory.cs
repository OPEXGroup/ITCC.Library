using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ITCC.HTTP.Common;
using ITCC.HTTP.Enums;
using ITCC.Logging;

namespace ITCC.HTTP.Server
{
    /// <summary>
    ///     Server response factory
    /// </summary>
    internal static class ResponseFactory
    {
        private static readonly Dictionary<AuthorizationStatus, HttpStatusCode> AuthResultDictionary = new Dictionary<AuthorizationStatus, HttpStatusCode>
        {
            {AuthorizationStatus.NotRequired, HttpStatusCode.OK },
            {AuthorizationStatus.Ok, HttpStatusCode.OK },
            {AuthorizationStatus.Unauthorized, HttpStatusCode.Unauthorized },
            {AuthorizationStatus.Forbidden, HttpStatusCode.Forbidden },
            {AuthorizationStatus.TooManyRequests, (HttpStatusCode)429 },
            {AuthorizationStatus.InternalError, HttpStatusCode.OK }
        };

        /// <summary>
        ///     Status code -> reason phrase map
        /// </summary>
        private static readonly Dictionary<HttpStatusCode, string> ReasonPhrases = new Dictionary
            <HttpStatusCode, string>
        {
            {HttpStatusCode.OK, "OK"},
            {HttpStatusCode.Created, "Created"},
            {HttpStatusCode.Accepted, "Accepted"},
            {HttpStatusCode.NoContent, "No Content"},
            {HttpStatusCode.PartialContent, "Partial Content"},
            {HttpStatusCode.MovedPermanently, "Moved Permanently" },
            {HttpStatusCode.Found, "Found" },
            {HttpStatusCode.BadRequest, "Bad request"},
            {HttpStatusCode.Unauthorized, "Unauthorized"},
            {HttpStatusCode.Forbidden, "Forbidden"},
            {HttpStatusCode.NotFound, "Not found"},
            {HttpStatusCode.Conflict, "Conflict"},
            {HttpStatusCode.RequestEntityTooLarge, "Request Entity Too Large" },
            {HttpStatusCode.RequestedRangeNotSatisfiable, "Requested Range Not Satisfiable" },
            {(HttpStatusCode)429, "Too many requests"},
            {HttpStatusCode.InternalServerError, "Internal Server Error"},
            {HttpStatusCode.NotImplemented, "Not implemented"},
            {HttpStatusCode.ServiceUnavailable, "Service unavailable" },
            {HttpStatusCode.MethodNotAllowed, "Method Not Allowed" }
        };

        /// <summary>
        ///     Common response headers
        /// </summary>
        private static Dictionary<string, string> _commonHeaders;

        private static Delegates.BodySerializer _bodySerializer;

        /// <summary>
        ///     Every server response will have this headers
        /// </summary>
        /// <param name="headers">Headers dictionary</param>
        /// <returns>Operation status</returns>
        public static bool SetCommonHeaders(IDictionary<string, string> headers)
        {
            if (headers == null)
                return false;

            _commonHeaders = new Dictionary<string, string>(headers);
            return true;
        }

        public static void SetBodySerializer(Delegates.BodySerializer serializer)
        {
            _bodySerializer = serializer;
        }

        public static void SetBodyEncoding(Encoding encoding)
        {
            _bodyEncoding = encoding;
        }

        public static async Task BuildResponse(HttpListenerResponse httpResponse, AuthentificationResult authentificationResult, bool gzipResponse = false)
        {
            if (authentificationResult == null)
                throw new ArgumentNullException(nameof(authentificationResult));

            await BuildResponse(httpResponse, authentificationResult.Status, authentificationResult.AccountView, authentificationResult.AdditionalHeaders, false, gzipResponse);
        }

        public static async Task BuildResponse<TAccount>(HttpListenerResponse httpResponse, AuthorizationResult<TAccount> authorizationResult, bool gzipResponse = false)
            where TAccount : class
        {
            if (authorizationResult == null)
                throw new ArgumentNullException(nameof(authorizationResult));

            var httpStatusCode = AuthResultDictionary.ContainsKey(authorizationResult.Status)
                ? AuthResultDictionary[authorizationResult.Status]
                : HttpStatusCode.InternalServerError;

            await BuildResponse(httpResponse,
                httpStatusCode,
                (object)authorizationResult.Account ?? authorizationResult.ErrorDescription,
                authorizationResult.AdditionalHeaders,
                gzipResponse);
        }

        public static async Task BuildResponse(HttpListenerResponse httpResponse, HandlerResult handlerResult, bool alreadyEncoded = false, bool gzipResponse = false)
        {
            if (handlerResult == null)
                throw new ArgumentNullException(nameof(handlerResult));

            await BuildResponse(httpResponse, handlerResult.Status, handlerResult.Body, handlerResult.AdditionalHeaders, alreadyEncoded, gzipResponse);
        }

        public static async Task BuildResponse(HttpListenerResponse httpResponse, HttpStatusCode code, object body, IDictionary<string, string> additionalHeaders = null, bool alreadyEncoded = false, bool gzipResponse = false)
        {
            httpResponse.StatusCode = (int)code;
            httpResponse.StatusDescription = SelectReasonPhrase(code);

            if (_commonHeaders != null)
            {
                foreach (var header in _commonHeaders)
                {
                    httpResponse.AddHeader(header.Key, header.Value);
                }
            }

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                {
                    httpResponse.AddHeader(header.Key, header.Value);
                }
            }

            if (body == null)
            {
                if (Logger.Level >= LogLevel.Trace)
                    Logger.LogEntry("RESP FACTORY", LogLevel.Trace, $"Response built: \n{SerializeResponse(httpResponse, null)}");
                return;
            }
            

            string bodyString;
            if (!alreadyEncoded)
                bodyString = _bodySerializer == null ? body.ToString() : _bodySerializer(body);
            else
            {
                bodyString = body as string;
            }

            var encoding = _bodyEncoding;
            if (gzipResponse)
            {
                using (var uncompressedStream = new MemoryStream(encoding.GetBytes(bodyString ?? string.Empty)))
                {
                    using (var gzipStream = new GZipStream(httpResponse.OutputStream, CompressionMode.Compress, true))
                    {
                        await uncompressedStream.CopyToAsync(gzipStream);
                    }
                }
                httpResponse.AddHeader("Content-Encoding", "gzip");
            }
            else
            {
                var bodyBuffer = encoding.GetBytes(bodyString ?? string.Empty);
                await httpResponse.OutputStream.WriteAsync(bodyBuffer, 0, bodyBuffer.Length);
            }

            httpResponse.ContentType = "application/json; charset=utf-8";
            // httpResponse.ContentLength64 = httpResponse.OutputStream.Length;

            if (Logger.Level >= LogLevel.Trace)
                Logger.LogEntry("RESP FACTORY", LogLevel.Trace, $"Response built: \n{SerializeResponse(httpResponse, bodyString)}");
        }

        /// <summary>
        ///     Makes string from http response
        /// </summary>
        /// <param name="response">Response</param>
        /// <param name="bodyString">Body string. we do not use response.Body because of gzip encoding</param>
        /// <returns>String representation</returns>
        public static string SerializeResponse(HttpListenerResponse response, string bodyString)
        {
            if (response == null)
                return string.Empty;

            var builder = new StringBuilder();
            builder.AppendLine($"HTTP/{response.ProtocolVersion} {response.StatusCode} {response.StatusDescription}");
            foreach (var key in response.Headers.AllKeys)
            {
                if (LogProhibitedHeaders.Contains(key))
                    builder.AppendLine($"{key}: {Constants.RemovedLogString}");
                builder.AppendLine($"{key}: {response.Headers[key]}");
            }
            if (response.OutputStream == null)
                return builder.ToString();

            builder.AppendLine();
            if (!LogResponseBodies)
                return builder.ToString();

            if (response.OutputStream is FileStream)
            {
                builder.AppendLine($"<File {((FileStream)response.OutputStream).Name} content>");
            }
            else
            {
                try
                {
                    var processedBodyString = bodyString;
                    foreach (var prohibitedPattern in LogBodyReplacePatterns)
                    {
                        processedBodyString = Regex.Replace(processedBodyString, prohibitedPattern.Item1,
                            prohibitedPattern.Item2);
                    }

                    if (ResponseBodyLogLimit < 1 || processedBodyString.Length <= ResponseBodyLogLimit)
                        builder.AppendLine(processedBodyString);
                    else
                    {
                        builder.AppendLine(processedBodyString.Substring(0, ResponseBodyLogLimit));
                        builder.AppendLine($"[And {processedBodyString.Length - ResponseBodyLogLimit} more bytes...]");
                    }
                }
                catch (Exception)
                {
                    return builder.ToString();
                }
            }

            return builder.ToString();
        }

        /// <summary>
        ///     Reason phrase selector
        /// </summary>
        /// <param name="code">Statuc code (enum element)</param>
        /// <returns>Reaon phrase</returns>
        private static string SelectReasonPhrase(HttpStatusCode code)
        {
            return ReasonPhrases.ContainsKey(code) ? ReasonPhrases[code] : "UNKNOWN REASON";
        }

        private static Encoding _bodyEncoding = Encoding.UTF8;
        public static bool LogResponseBodies = true;
        public static int ResponseBodyLogLimit = -1;
        public static readonly List<Tuple<string, string>> LogBodyReplacePatterns = new List<Tuple<string, string>>();
        public static readonly List<string> LogProhibitedHeaders = new List<string>(); 
    }
}