using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace ITCC.HTTP.Server
{
    /// <summary>
    ///     Represents client handler result
    /// </summary>
    public class HandlerResult
    {
        public HandlerResult(HttpStatusCode statusCode, object body, IDictionary<string, string> additionalHeaders = null)
        {
            Status = statusCode;
            Body = body;
            AdditionalHeaders = additionalHeaders;
        }

        /// <summary>
        ///     Generated response
        /// </summary>
        public object Body { get; set; }

        /// <summary>
        ///     General handler status
        /// </summary>
        public HttpStatusCode Status { get; set; }

        /// <summary>
        ///     
        /// </summary>
        public IDictionary<string, string> AdditionalHeaders { get; set; }
    }
}