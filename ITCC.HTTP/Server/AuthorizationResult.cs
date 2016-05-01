using System;
using ITCC.HTTP.Enums;

namespace ITCC.HTTP.Server
{
    /// <summary>
    ///     Represent authentification status
    /// </summary>
    public class AuthorizationResult<TAccount>
    {
        public AuthorizationResult(TAccount account, AuthentificationStatus status, object userdata = null)
        {
            Account = account;
            Status = status;
            Userdata = userdata;
        }

        public AuthorizationResult(TAccount account, AuthentificationStatus status, string errorDescription, object userdata = null)
        {
            // For internal checks
            if (status == AuthentificationStatus.Ok)
                throw new ArgumentException("Error description provided, but status is OK");

            Account = account;
            Status = status;
            ErrorDescription = errorDescription;
            Userdata = userdata;
        }

        /// <summary>
        ///     Resulting account
        /// </summary>
        public TAccount Account { get; set; }

        /// <summary>
        ///     Authentification status (if positive, then Account is not null)
        /// </summary>
        public AuthentificationStatus Status { get; set; }

        /// <summary>
        ///     Can be used in case Status != AuthentificationStatus.Ok
        /// </summary>
        public string ErrorDescription { get; set; }

        /// <summary>
        ///     Custom additional data
        /// </summary>
        public object Userdata { get; set; }
    }
}