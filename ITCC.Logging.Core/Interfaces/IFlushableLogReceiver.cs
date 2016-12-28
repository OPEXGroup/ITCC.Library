// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Threading.Tasks;

namespace ITCC.Logging.Core.Interfaces
{
    public interface IFlushableLogReceiver : ILogReceiver
    {
        /// <summary>
        ///     Used for loggers that use message queues internally
        /// </summary>
        /// <returns></returns>
        Task Flush();
    }
}
