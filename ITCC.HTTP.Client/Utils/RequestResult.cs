using System;
using System.Collections.Generic;
using ITCC.HTTP.Common.Enums;

namespace ITCC.HTTP.Client.Utils
{
    /// <summary>
    ///     Reperesents single Http request result (for client)
    /// </summary>
    /// <typeparam name="TResult">
    ///     What we actually got in request response or null if !IsGoodClientRequestStatus(Status)
    /// </typeparam>
    public class RequestResult<TResult>
    {
        /// <summary>
        ///     Actual request result (processed resonse body)
        /// </summary>
        public TResult Result { get; set; }

        /// <summary>
        ///     Request success status
        /// </summary>
        public ServerResponseStatus Status { get; set; }

        /// <summary>
        ///     Some additional data
        /// </summary>
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        ///     Exception thrown during request execution
        /// </summary>
        public Exception Exception { get; set; }

        public RequestResult()
        {
            
        }

        public RequestResult(TResult result, ServerResponseStatus status, IDictionary<string, string> headers = null)
        {
            Result = result;
            Status = status;
            Headers = headers ?? new Dictionary<string, string>();
        }

        public RequestResult(TResult result, ServerResponseStatus status, Exception exception)
        {
            Result = result;
            Status = status;
            Exception = exception;
        }

        public RequestResult<TOther> ChangeResultType<TOther>()
            where TOther : class
        {
            return new RequestResult<TOther>
            {
                Result = Result as TOther,
                Status = Status,
                Headers = Headers,
                Exception = Exception
            };
        }
    }
}