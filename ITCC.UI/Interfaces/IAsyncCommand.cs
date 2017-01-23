// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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
