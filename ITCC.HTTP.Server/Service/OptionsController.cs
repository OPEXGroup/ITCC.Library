// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ITCC.HTTP.Server.Core;
using ITCC.HTTP.Server.Interfaces;

namespace ITCC.HTTP.Server.Service
{
    internal class OptionsController<TAccount>: IServiceController
        where TAccount: class
    {
        #region IServiceController
        public bool RequestIsSuitable(HttpListenerRequest request)
        {
            if (request == null)
            {
                return false;
            }
            return request.HttpMethod.ToUpper() == "OPTIONS";
        }

        public Task HandleRequest(HttpListenerContext context, Stopwatch stopwatch, Action<HttpListenerContext, Stopwatch> completionCallback)
        {
            var allowValues = new List<string>();
            var request = context.Request;

                foreach (var requestProcessor in _requestProcessors)
                {
                    if (request.Url.LocalPath.Trim('/') == requestProcessor.SubUri)
                    {
                        allowValues.Add(requestProcessor.Method.Method);
                        if (requestProcessor.Method == HttpMethod.Get)
                            allowValues.Add("HEAD");
                    }
                }


            if (allowValues.Any())
            {
                ResponseFactory.BuildResponse(context, HttpStatusCode.OK, null);
                context.Response.AddHeader("Allow", string.Join(", ", allowValues));
            }
            else
            {
                ResponseFactory.BuildResponse(context, HttpStatusCode.NotFound, null);
            }

            completionCallback(context, stopwatch);
            return Task.FromResult(0);
        }

        public string Name => "Options";
        #endregion

        #region public

        public OptionsController(IEnumerable<RequestProcessor<TAccount>> requestProcessors)
        {
            _requestProcessors = requestProcessors;
        }
        #endregion

        #region private

        private readonly IEnumerable<RequestProcessor<TAccount>> _requestProcessors;

        #endregion
    }
}
