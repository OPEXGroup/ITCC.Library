// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using ITCC.HTTP.Server.Core;
using ITCC.HTTP.Server.Interfaces;
using ITCC.HTTP.Server.Utils;

namespace ITCC.HTTP.Server.Service
{
    public class PingController : IServiceController
    {
        #region IServiceController
        public bool RequestIsSuitable(HttpListenerRequest request)
            => request != null && CommonHelper.UriMatchesString(request.Url, "ping");

        public Task HandleRequest(HttpListenerContext context)
        {
            var responseBody = new PingResponse(CommonHelper.SerializeHttpRequest(context, true));
            ResponseFactory.BuildResponse(context, HttpStatusCode.OK, responseBody);
            return Task.FromResult(0);
        }

        public string Name => "Ping";
        #endregion
    }
}
