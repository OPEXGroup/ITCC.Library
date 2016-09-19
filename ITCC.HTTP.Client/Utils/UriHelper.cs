using System;
using System.Collections.Generic;
using System.Text;

namespace ITCC.HTTP.Client.Utils
{
    /// <summary>
    ///     Contains static methods for uri processing
    /// </summary>
    internal static class UriHelper
    {
        public static string BuildFullUri(string fullServerAddress, string uri,
            IDictionary<string, string> parameters, int maxSize = 4096)
        {
            if (fullServerAddress == null || maxSize < 5)
                return null;
            if (!fullServerAddress.ToLower().StartsWith("http"))
                return null;

            var builder = new StringBuilder(maxSize);

            if (!IsAbsoluteUrl(uri))
            {
                builder.Append(fullServerAddress);
                if (!fullServerAddress.EndsWith("/") && !uri.StartsWith("/"))
                    builder.Append("/");
            }
            builder.Append(uri ?? "");
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

        public static bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result);
        }

        public static string ExtractRelativeUri(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return null;
            return uri.LocalPath;
        }
    }
}