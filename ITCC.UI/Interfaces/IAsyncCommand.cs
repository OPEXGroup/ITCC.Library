using System.Threading.Tasks;
using System.Windows.Input;

namespace ITCC.UI.Interfaces
{
    /// <summary>
    ///     Used for WPF bindings. 
    ///     Used for asynchronous commands with possible asynchronous cancellation support.
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}
