using System.Net;

namespace ITCC.HTTP.Server
{
    /// <summary>
    ///     Represent authentification status
    /// </summary>
    public class AuthentificationResult
    {
        public AuthentificationResult(object accountView, HttpStatusCode status, object userdata = null)
        {
            AccountView = accountView;
            Status = status;
            Userdata = userdata;
        }

        /// <summary>
        ///     Resulting account
        /// </summary>
        public object AccountView { get; set; }

        /// <summary>
        ///     Authentification status (if positive, then Account is not null)
        /// </summary>
        public HttpStatusCode Status { get; set; }

        /// <summary>
        ///     Custom user data
        /// </summary>
        public object Userdata { get; set; }
    }
}