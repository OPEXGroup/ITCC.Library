using System.Collections.Generic;
using System.Net;

namespace ITCC.HTTP.Server
{
    /// <summary>
    ///     Represent authentification status
    /// </summary>
    public class AuthentificationResult
    {
        public AuthentificationResult(object accountView, HttpStatusCode status, IDictionary<string, string> additionalHeaders = null)
        {
            AccountView = accountView;
            Status = status;
            AdditionalHeaders = additionalHeaders;
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
        ///     Custom additional headers. User SHOULD provide Retry-After header in case of Status == 429
        /// </summary>
        public IDictionary<string, string> AdditionalHeaders { get; set; }
    }
}