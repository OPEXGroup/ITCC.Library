﻿using System.Net.Http;
using ITCC.Logging.Core;

namespace ITCC.HTTP.Server.Utils
{
    /// <summary>
    ///     Simple methods to deal with request/response statuses
    /// </summary>
    public static class CommonHelper
    {
        /// <summary>
        ///     Converts Griffin's http method notation to system enum
        /// </summary>
        /// <param name="methodName">Method name string</param>
        /// <returns>Enum element</returns>
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
    }
}