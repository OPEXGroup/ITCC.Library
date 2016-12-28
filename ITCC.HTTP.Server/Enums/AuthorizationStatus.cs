// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.HTTP.Server.Enums
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

        /// <summary>
        ///     Internal server error occured during authentification
        /// </summary>
        InternalError
    }
}