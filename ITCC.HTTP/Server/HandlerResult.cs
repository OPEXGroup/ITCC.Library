using System.Net;

namespace ITCC.HTTP.Server
{
    /// <summary>
    ///     Represents client handler result
    /// </summary>
    public class HandlerResult
    {
        public HandlerResult(HttpStatusCode statusCode, object body)
        {
            Status = statusCode;
            Body = body;
        }

        /// <summary>
        ///     Generated response
        /// </summary>
        public object Body { get; set; }

        /// <summary>
        ///     General handler status
        /// </summary>
        public HttpStatusCode Status { get; set; }
    }
}