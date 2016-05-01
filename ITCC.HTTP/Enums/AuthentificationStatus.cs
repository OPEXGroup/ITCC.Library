﻿namespace ITCC.HTTP.Enums
{
    /// <summary>
    ///     Represents possible authentification result
    /// </summary>
    public enum AuthentificationStatus
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
        ///     Internal server error occured during authentification
        /// </summary>
        InternalError,

        /// <summary>
        ///     Too many requests from current user
        /// </summary>
        TooManyRequests
    }
}