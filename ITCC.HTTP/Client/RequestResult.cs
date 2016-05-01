using ITCC.HTTP.Enums;

namespace ITCC.HTTP.Client
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
        public TResult Result;

        /// <summary>
        ///     Request success status
        /// </summary>
        public ServerResponseStatus Status;

        /// <summary>
        ///     Some additional data
        /// </summary>
        public object Userdata;

        public RequestResult<TOther> ChangeResultType<TOther>()
            where TOther : class
        {
            return new RequestResult<TOther>
            {
                Result = Result as TOther,
                Status = Status,
                Userdata = Userdata
            };
        }
    }
}