﻿using System;
using System.Net.Http;
using ITCC.HTTP.Common;

namespace ITCC.HTTP.Server
{
    public class RequestProcessor<TAccount>
    {
        /// <summary>
        ///     True iff some credentials must be provided
        /// </summary>
        public bool AuthorizationRequired;

        /// <summary>
        ///     Actual request handler
        /// </summary>
        public Delegates.RequestHandler<TAccount> Handler;

        /// <summary>
        ///     Isengard 1.0 API method name (null if not present in 1.0)
        /// </summary>
        [Obsolete]
        public string LegacyName;

        /// <summary>
        ///     Request method
        /// </summary>
        public HttpMethod Method;

        /// <summary>
        ///     Request SubUri
        /// </summary>
        public string SubUri;
    }
}