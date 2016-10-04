using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace ITCC.HTTP.Server.Interfaces
{
    /// <summary>
    ///     Used to process some kinds of requests with special methods
    /// </summary>
    public interface IServiceController
    {
        bool RequestIsSuitable(HttpListenerRequest request);

        Task HandleRequest(HttpListenerContext context, Stopwatch stopwatch, Action<HttpListenerContext, Stopwatch> completionCallback);

        string Name { get; }
    }
}
