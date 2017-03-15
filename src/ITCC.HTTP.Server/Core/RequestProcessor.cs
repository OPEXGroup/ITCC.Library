// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Net.Http;
using ITCC.HTTP.Server.Common;

namespace ITCC.HTTP.Server.Core
{
    public class RequestProcessor<TAccount>
    {
        /// <summary>
        ///     True iff some credentials must be provided
        /// </summary>
        public bool AuthorizationRequired { get; set; }

        /// <summary>
        ///     Actual request handler
        /// </summary>
        public Delegates.RequestHandler<TAccount> Handler { get; set; }

        /// <summary>
        ///     Request method
        /// </summary>
        public HttpMethod Method { get; set; } = HttpMethod.Get;

        /// <summary>
        ///     Request SubUri
        /// </summary>
        public string SubUri { get; set; }
    }
}