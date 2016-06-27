using System.Threading.Tasks;

namespace ITCC.Logging.Interfaces
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
