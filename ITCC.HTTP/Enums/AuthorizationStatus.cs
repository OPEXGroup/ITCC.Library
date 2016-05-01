namespace ITCC.HTTP.Enums
{
    /// <summary>
    ///     Represents possible authorization result
    /// </summary>
    public enum AuthorizationStatus
    {
        NotRequired,
        /// <summary>
        ///     Request will be processed
        /// </summary>
        Ok,

        /// <summary>
        ///     Another authentification token must be provided (401)
        /// </summary>
        Unauthorized,

        /// <summary>
        ///     Resourse access in permanently forbidded for this account (403)
        /// </summary>
        Forbidden,

        /// <summary>
        ///     Too many requests from current user
        /// </summary>
        TooManyRequests,

        // <summary>
        ///     Internal server error occured during authentification
        /// </summary>
        InternalError
    }
}