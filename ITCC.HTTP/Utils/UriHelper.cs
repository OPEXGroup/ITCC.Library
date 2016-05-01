using System;
using System.Collections.Generic;
using System.Text;
using ITCC.HTTP.Enums;

namespace ITCC.HTTP.Utils
{
    /// <summary>
    ///     Contains static methods for uri processing
    /// </summary>
    internal static class UriHelper
    {
        /// <summary>
        ///     Builds uri that can be passed to HttpClient
        /// </summary>
        /// <param name="fullServerAddress">Server address with post-address uri</param>
        /// <param name="partialUri">Uri part after address and port</param>
        /// <param name="parameters">Params going after ?</param>
        /// <param name="maxSize">Maximum result address length</param>
        /// <returns>URI or null in case of error</returns>
        public static string BuildFullUri(string fullServerAddress, string partialUri,
            IDictionary<string, string> parameters, int maxSize = 4096)
        {
            if (fullServerAddress == null || maxSize < 5)
                return null;
            if (!fullServerAddress.ToLower().StartsWith("http"))
                return null;

            var builder = new StringBuilder(maxSize);

            builder.Append(fullServerAddress);
            if (!fullServerAddress.EndsWith("/") && !partialUri.StartsWith("/"))
                builder.Append("/");
            if (partialUri != null)
                builder.Append(partialUri);

            if (parameters == null || parameters.Count <= 0)
                return Uri.EscapeUriString(builder.ToString());

            builder.Append("?");
            foreach (var item in parameters)
            {
                if (item.Key == null || item.Value == null)
                    return null;
                builder.Append($"{item.Key}={item.Value}&");
            }

            return Uri.EscapeUriString(builder.ToString().TrimEnd('&'));
        }

        /// <summary>
        ///     Builds uri that can be passed to HttpClient
        /// </summary>
        /// <param name="protocol">Connection protocol</param>
        /// <param name="serverIp">Server IPv4 address</param>
        /// <param name="serverPort">Server listening port</param>
        /// <param name="partialUri">URI part after port</param>
        /// <param name="parameters">Params going after ?</param>
        /// <param name="maxSize">Maximum result address length</param>
        /// <returns>URI or null in case of error</returns>
        public static string BuildFullUri(Protocol protocol, string serverIp, string serverPort, string partialUri,
            IDictionary<string, string> parameters, int maxSize = 4096)
        {
            if (serverIp == null)
                return null;
            var fullserverAddress = "http"
                                    + (protocol == Protocol.Https ? "s" : string.Empty)
                                    + serverIp
                                    + (serverPort != null ? ":" + serverPort : string.Empty) + "/";
            return BuildFullUri(fullserverAddress, partialUri, parameters, maxSize);
        }
    }
}