namespace ITCC.HTTP.Enums
{
    /// <summary>
    ///     Enums for server response status
    /// </summary>
    public enum ServerResponseStatus
    {
        /// <summary>
        ///     Everything is fine (200, 201, 202, 206)
        /// </summary>
        Ok,

        /// <summary>
        ///     Nothing changed and no actions should be taken (204)
        /// </summary>
        NothingToDo,

        /// <summary>
        ///     Request has been redirected (301, 302)
        /// </summary>
        Redirect,

        /// <summary>
        ///     Client-side error (400, 404, 405, 409, 413, 416)
        /// </summary>
        ClientError,

        /// <summary>
        ///     Server-side error (500, 501)
        /// </summary>
        ServerError,

        /// <summary>
        ///     Operation was not allowed (401)
        /// </summary>
        Unauthorized,

        /// <summary>
        ///     Operation is permanently forbidden (403)
        /// </summary>
        Forbidden,

        /// <summary>
        ///     Too many requests have been sent from the current client (429)
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
        ConnectionError,

        /// <summary>
        ///     Resourse is temporary unavailable (503)
        /// </summary>
        TemporaryUnavailable,
    }
}