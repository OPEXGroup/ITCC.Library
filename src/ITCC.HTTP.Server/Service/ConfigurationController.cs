using System.Net;
using System.Threading.Tasks;
using ITCC.HTTP.Server.Core;
using ITCC.HTTP.Server.Interfaces;
using ITCC.HTTP.Server.Utils;

namespace ITCC.HTTP.Server.Service
{
    internal class ConfigurationController<TAccount> : IServiceController
        where TAccount : class
    {
        private readonly HttpServerConfiguration<TAccount> _configuration;

        public ConfigurationController(HttpServerConfiguration<TAccount> configuration)
        {
            _configuration = configuration;
        }

        #region IServiceController
        public bool RequestIsSuitable(HttpListenerRequest request)
            => request != null && CommonHelper.UriMatchesString(request.Url, "config");

        public Task HandleRequestAsync(HttpListenerContext context)
        {
            ResponseFactory.BuildResponse(context, HttpStatusCode.OK, _configuration);
            return Task.FromResult(0);
        }

        public string Name => "Configuration";
        #endregion
    }
}
