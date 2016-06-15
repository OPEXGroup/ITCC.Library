namespace ITCC.HTTP.Enums
{
    /// <summary>
    ///     Enums for server response status
    /// </summary>
    public enum ServerResponseStatus
    {
        /// <summary>
        ///     Everything is fine
        /// </summary>
        Ok,

        /// <summary>
        ///     Nothing changed and no actions should be taken
        /// </summary>
        NothingToDo,

        /// <summary>
        ///     Client-side error
        /// </summary>
        ClientError,

        /// <summary>
        ///     Server-side error
        /// </summary>
        ServerError,

        /// <summary>
        ///     Operattion was not allowed
        /// </summary>
        Unauthorized,

        /// <summary>
        ///     Operation is permanently forbidden
        /// </summary>
        Forbidden,

        /// <summary>
        ///     Too many requests have been sent from the current client
        /// </summary>
        TooManyRequests,

        /// <summary>
        ///     Server responded with good status code, but content cannot be recognized
        /// </summary>
        IncompehensibleResponse,

        /// <summary>
        ///     Request was canceled before whole response
        /// </summary>
        RequestCancelled,

        /// <summary>
        ///     Request has been timed out
        /// </summary>
        RequestTimeout,

        /// <summary>
        ///     No response (some network problems)
        /// </summary>
        ConnectionError
    }
}