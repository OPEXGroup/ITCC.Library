using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ITCC.HTTP.Enums;
using ITCC.Logging;

namespace ITCC.HTTP.Server
{
    internal static class ResponseFactory
    {
        #region public
        public static bool SetCommonHeaders(IDictionary<string, string> headers)
        {
            if (headers == null)
                return false;

            _commonHeaders = new Dictionary<string, string>(headers);
            return true;
        }

        public static void SetBodyEncoder(BodyEncoder encoder)
        {
            _encoder = encoder;
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
                bodyString = _encoder.Serializer == null ? body.ToString() : _encoder.Serializer(body);
            else
            {
                bodyString = body as string;
            }

            if (gzipResponse)
            {
                httpResponse.SendChunked = false;
                httpResponse.ContentType = $"{_encoder.ContentType}; charset={_encoder.Encoding.WebName}";
                httpResponse.AddHeader("Content-Encoding", "gzip");
                
                using (var uncompressedStream = new MemoryStream(_encoder.Encoding.GetBytes(bodyString ?? string.Empty)))
                {
                    using (var gzipStream = new GZipStream(httpResponse.OutputStream, CompressionMode.Compress, true))
                    {
                        await uncompressedStream.CopyToAsync(gzipStream);
                    }
                }
            }
            else
            {
                var bodyBuffer = _encoder.Encoding.GetBytes(bodyString ?? string.Empty);
                httpResponse.SendChunked = false;
                httpResponse.ContentLength64 = bodyBuffer.Length;
                httpResponse.ContentType = $"{_encoder.ContentType}; charset={_encoder.Encoding.WebName}";
                await httpResponse.OutputStream.WriteAsync(bodyBuffer, 0, bodyBuffer.Length);
            }


            if (Logger.Level >= LogLevel.Trace)
                Logger.LogEntry("RESP FACTORY", LogLevel.Trace, $"Response built: \n{SerializeResponse(httpResponse, bodyString)}");
        }

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

        public static bool LogResponseBodies = true;
        public static int ResponseBodyLogLimit = -1;
        public static readonly List<Tuple<string, string>> LogBodyReplacePatterns = new List<Tuple<string, string>>();
        public static readonly List<string> LogProhibitedHeaders = new List<string>();

        #endregion

        #region private

        private static string SelectReasonPhrase(HttpStatusCode code) => ReasonPhrases.ContainsKey(code) ? ReasonPhrases[code] : "UNKNOWN REASON";

        private static readonly Dictionary<AuthorizationStatus, HttpStatusCode> AuthResultDictionary = new Dictionary<AuthorizationStatus, HttpStatusCode>
        {
            {AuthorizationStatus.NotRequired, HttpStatusCode.OK },
            {AuthorizationStatus.Ok, HttpStatusCode.OK },
            {AuthorizationStatus.Unauthorized, HttpStatusCode.Unauthorized },
            {AuthorizationStatus.Forbidden, HttpStatusCode.Forbidden },
            {AuthorizationStatus.TooManyRequests, (HttpStatusCode)429 },
            {AuthorizationStatus.InternalError, HttpStatusCode.OK }
        };

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

        private static Dictionary<string, string> _commonHeaders;
        private static BodyEncoder _encoder;
        #endregion

    }
}