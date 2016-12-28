// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ITCC.HTTP.Common;
using ITCC.HTTP.Server.Core;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server.Utils
{
    /// <summary>
    ///     Simple methods to deal with request/response statuses
    /// </summary>
    public static class CommonHelper
    {
        #region public

        public static void SetSerializationLimitations(int requestBodyLogLimit,
            List<string> prohibitedQueryParams,
            List<string> prohibitedHeaders,
            Encoding encoding)
        {
            _requestBodyLogLimit = requestBodyLogLimit;
            _prohibitedHeaders = prohibitedHeaders;
            _prohibitedQueryParams = prohibitedQueryParams;
            _encoding = encoding;
        }

        public static HttpMethod HttpMethodToEnum(string methodName)
        {
            if (methodName == null)
                return HttpMethod.Get;
            var upperCaseMethodName = methodName.ToUpper();

            switch (upperCaseMethodName)
            {
                case "GET":
                    return HttpMethod.Get;
                case "POST":
                    return HttpMethod.Post;
                case "PUT":
                    return HttpMethod.Put;
                case "DELETE":
                    return HttpMethod.Delete;
                case "HEAD":
                    return HttpMethod.Head;
                case "OPTIONS":
                    return HttpMethod.Options;
                default:
                    Logger.LogEntry("COMMONHELPER", LogLevel.Warning,
                        $"Unknown method `{methodName}`, defaults to `GET`");
                    return HttpMethod.Get;
            }
        }

        public static bool UriMatchesString(Uri uri, string str)
            => string.Equals(uri.LocalPath.Trim('/'), str.Trim('/'), StringComparison.OrdinalIgnoreCase);

        public static string SerializeHttpRequest(HttpListenerContext context,
            bool absolutePath = false,
            bool serializeBody = true)
        {
            var request = context.Request;

            if (request == null)
                return null;

            var builder = new StringBuilder();
            var queryString = string.Join("&", request.QueryString.AllKeys.Select(k => $"{k}={QueryParamValueForLog(request, k)}"));
            var separator = string.IsNullOrEmpty(queryString) ? string.Empty : "?";
            var url = absolutePath ? request.Url.ToString() : request.Url.LocalPath;
            builder.AppendLine($"{request.HttpMethod} {url}{separator}{queryString} HTTP/{request.ProtocolVersion}");

            foreach (var key in request.Headers.AllKeys)
            {
                builder.AppendLine(_prohibitedHeaders.Contains(key)
                    ? $"{key}: {Constants.RemovedLogString}"
                    : $"{key}: {request.Headers[key]}");
            }

            if (!request.HasEntityBody)
                return builder.ToString();

            builder.AppendLine();
            if (serializeBody)
            {
                var memoryStream = new MemoryStream();
                request.InputStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(memoryStream, _encoding, true, 4096, true))
                {
                    var bodyString = reader.ReadToEnd();
                    bodyString = ResponseFactory.LogBodyReplacePatterns.Aggregate(bodyString, (current, replacePattern) => Regex.Replace(current, replacePattern.Item1, replacePattern.Item2));
                    builder.AppendLine(bodyString);
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                request.GetType().InvokeMember("m_RequestStream", BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic, null, request, new object[] { memoryStream });
            }
            else
            {
                builder.AppendLine("<Binary content>");
            }

            return builder.ToString();
        }
        #endregion

        #region private
        private static string QueryParamValueForLog(HttpListenerRequest request, string paramName) => _prohibitedQueryParams.Contains(paramName)
            ? Constants.RemovedLogString
            : request.QueryString[paramName];

        private static int _requestBodyLogLimit = -1;
        private static List<string> _prohibitedQueryParams = new List<string>();
        private static List<string> _prohibitedHeaders = new List<string>();
        private static Encoding _encoding = Encoding.UTF8;
        #endregion
    }
}