// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using ITCC.HTTP.Server.Common;
using ITCC.HTTP.Server.Core;
using ITCC.HTTP.Server.Interfaces;
using ITCC.HTTP.Server.Utils;

namespace ITCC.HTTP.Server.Service
{
    internal class StatisticsController<TAccount> : IServiceController, IDisposable
        where TAccount : class 
    {
        #region IServiceController
        public bool RequestIsSuitable(HttpListenerRequest request)
        {
            if (request == null)
            {
                return false;
            }
            return CommonHelper.UriMatchesString(request.Url, "statistics") && StatisticsEnabled;
        }

        public async Task HandleRequestAsync(HttpListenerContext context)
        {
            var response = context.Response;
            if (_statisticsAuthorizer != null)
            {
                if (await _statisticsAuthorizer.Invoke(context.Request))
                {
                    var responseBody = Statistics?.Serialize();
                    ResponseFactory.BuildResponse(context, HttpStatusCode.OK, responseBody, null, true);
                    response.ContentType = "text/plain";
                }
                else
                {
                    ResponseFactory.BuildResponse(context, HttpStatusCode.Forbidden, null);
                }
            }
            else
            {
                var responseBody = Statistics?.Serialize();
                ResponseFactory.BuildResponse(context, HttpStatusCode.OK, responseBody, null, true);
                response.ContentType = "text/plain";
            }
        }

        public string Name => "Statistics";

        #endregion

        #region IDisposable

        public void Dispose() => Statistics.Dispose();

        #endregion

        #region public
        public StatisticsController(ServerStatistics<TAccount> statistics, Delegates.StatisticsAuthorizer statisticsAuthorizer)
        {
            Statistics = statistics;
            _statisticsAuthorizer = statisticsAuthorizer;
        }

        public bool StatisticsEnabled => Statistics != null;

        public ServerStatistics<TAccount> Statistics { get; }
        #endregion

        #region private

        private readonly Delegates.StatisticsAuthorizer _statisticsAuthorizer;

        #endregion

        
    }
}
