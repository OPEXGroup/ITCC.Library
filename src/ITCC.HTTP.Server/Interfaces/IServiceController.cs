// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

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

        Task HandleRequestAsync(HttpListenerContext context);

        string Name { get; }
    }
}
