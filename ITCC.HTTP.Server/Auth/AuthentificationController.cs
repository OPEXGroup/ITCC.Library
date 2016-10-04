using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using ITCC.HTTP.Server.Common;
using ITCC.HTTP.Server.Core;
using ITCC.HTTP.Server.Interfaces;
using ITCC.HTTP.Server.Utils;

namespace ITCC.HTTP.Server.Auth
{
    internal class AuthentificationController : IServiceController
    {
        #region IServiceRequestProcessor
        public bool RequestIsSuitable(HttpListenerRequest request)
        {
            if (request == null || _authentificator == null)
            {
                return false;
            }
            if (CommonHelper.UriMatchesString(request.Url, "login"))
            {
                return true;
            }
            return request.QueryString["login"] != null && request.QueryString["password"] != null;
        }

        public async Task HandleRequest(HttpListenerContext context, Stopwatch stopwatch, Action<HttpListenerContext, Stopwatch> completionCallback)
        {
            AuthentificationResult authResult;
            var request = context.Request;
            if (_authentificator != null)
                authResult = await _authentificator.Invoke(request);
            else
                authResult = new AuthentificationResult(null, HttpStatusCode.NotFound);
            if (authResult == null)
                throw new InvalidOperationException("Authentificator fault: null result");
            ResponseFactory.BuildResponse(context, authResult);
            completionCallback(context, stopwatch);
        }

        public string Name => "Authentification";
        #endregion

        #region public

        public AuthentificationController(Delegates.Authentificator authentificator)
        {
            _authentificator = authentificator;
        }
        #endregion

        #region private

        private readonly Delegates.Authentificator _authentificator;

        #endregion
    }
}
