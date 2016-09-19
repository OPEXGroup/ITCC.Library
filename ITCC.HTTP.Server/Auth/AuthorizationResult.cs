using System;
using System.Collections.Generic;
using ITCC.HTTP.Server.Enums;

namespace ITCC.HTTP.Server.Auth
{
    /// <summary>
    ///     Represent authentification status
    /// </summary>
    public class AuthorizationResult<TAccount>
    {
        public AuthorizationResult(TAccount account, AuthorizationStatus status, IDictionary<string, string> additionalHeaders = null)
        {
            Account = account;
            Status = status;
            AdditionalHeaders = additionalHeaders;
        }

        public AuthorizationResult(TAccount account, AuthorizationStatus status, string errorDescription, IDictionary<string, string> additionalHeaders = null)
        {
            // For internal checks
            if (status == AuthorizationStatus.Ok)
                throw new ArgumentException("Error description provided, but status is OK");

            Account = account;
            Status = status;
            ErrorDescription = errorDescription;
            AdditionalHeaders = additionalHeaders;
        }

        /// <summary>
        ///     Resulting account
        /// </summary>
        public TAccount Account { get; set; }

        /// <summary>
        ///     Authentification status (if positive, then Account is not null)
        /// </summary>
        public AuthorizationStatus Status { get; set; }

        /// <summary>
        ///     Can be used in case Status != AuthorizationStatus.Ok
        /// </summary>
        public string ErrorDescription { get; set; }

        /// <summary>
        ///     Custom additional headers. User SHOULD provide Retry-After header in case of Status == 429
        /// </summary>
        public IDictionary<string, string> AdditionalHeaders { get; set; }
    }
}