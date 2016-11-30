using System;
using System.Collections.Generic;
using ITCC.HTTP.API.Utils;
using ITCC.HTTP.Client.Enums;

namespace ITCC.HTTP.Client.Utils
{
    public class VariadicRequestResult<TSuccess, TError>
        where TSuccess : class
        where TError : class
    {
        #region properties

        /// <summary>
        ///     Success request result (processed response body)
        /// </summary>
        public TSuccess Success => _either?.First;

        /// <summary>
        ///     True iff Success != null;
        /// </summary>
        public bool IsSuccess => Success != null;

        /// <summary>
        ///     Success request result (processed response body)
        /// </summary>
        public TError Error => _either?.Second;

        /// <summary>
        ///     True iff Error != null;
        /// </summary>
        public bool IsError => Error != null;

        /// <summary>
        ///     Request success status
        /// </summary>
        public ServerResponseStatus Status { get; internal set; }

        /// <summary>
        ///     Some additional data
        /// </summary>
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        /// <summary>
        ///     Exception thrown during request execution
        /// </summary>
        public Exception Exception { get; internal set; }
        #endregion

        #region construction

        public VariadicRequestResult()
        {

        }

        public VariadicRequestResult(TSuccess success, ServerResponseStatus status, IDictionary<string, string> headers = null)
        {
            if (success != null)
                _either = new Either<TSuccess, TError>(success);
            Status = status;
            Headers = headers ?? new Dictionary<string, string>();
        }

        public VariadicRequestResult(TError error, ServerResponseStatus status, IDictionary<string, string> headers = null)
        {
            if (error != null)
                _either = new Either<TSuccess, TError>(error);
            Status = status;
            Headers = headers ?? new Dictionary<string, string>();
        }

        public VariadicRequestResult(TError error, ServerResponseStatus status, Exception exception)
        {
            if (error != null)
                _either = new Either<TSuccess, TError>(error);
            Status = status;
            Exception = exception;
        }

        public static VariadicRequestResult<TSuccess, TError> Transform<TOtherSuccess>(
            VariadicRequestResult<TOtherSuccess, TError> otherResult, Func<TOtherSuccess, TSuccess> transformation)
            where TOtherSuccess : class
            => new VariadicRequestResult<TSuccess, TError>(transformation(otherResult.Success), otherResult.Status, otherResult.Headers);

        #endregion

        #region private

        private readonly Either<TSuccess, TError> _either;

        #endregion
    }
}
