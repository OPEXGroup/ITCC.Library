using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ITCC.UI.Interfaces;

namespace ITCC.UI.Commands
{
    /// <summary>
    ///     Class represents asynchronous command to be called from GUI
    /// </summary>
    /// <typeparam name="TResult">Command result type</typeparam>
    /// <example>
    ///     Add viewmodel to window resourses:
    ///     <code>
    ///         &lt;Window.DataContext&gt;
    ///              &lt;vms:MyViewModel x:Name="ViewModel"&gt;&lt;/vms:MyViewModel&gt;
    ///         &lt;/Window.DataContext&gt;
    ///         ...
    ///         &lt;Button Click={Binding UpdateCommand} /&gt;
    ///     </code>
    ///     In viewmodel:
    ///     <code>
    ///         class MyViewModel
    ///         {
    ///             public MyViewModel
    ///             {
    ///                 UpdateCommand = AsyncCommandFactory.Create(() => UpdateAsync);
    ///             }
    ///
    ///             private async Task UpdateAsync()
    ///             {
    ///                 ...
    ///             }
    ///
    ///             AsyncCommand UpdateCommand { get; }
    ///         }
    ///     </code>
    /// </example>
    public class AsyncCommand<TResult> : IAsyncCommand, INotifyPropertyChanged
    {
        #region IAsyncCommand
        public bool CanExecute(object parameter)
        {
            var value = _canExecuteCondition.Invoke() && (Execution == null || Execution.IsCompleted);
            Enabled = value;
            return value;
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }
        
        public async Task ExecuteAsync(object parameter)
        {
            _cancelCommand.NotifyCommandStarting();
            Execution = new NotifyTaskCompletion<TResult>(_command(parameter, _cancelCommand.Token));
            RaiseCanExecuteChanged();
            if (Execution?.TaskCompletion != null)
                await Execution.TaskCompletion;
            _cancelCommand.NotifyCommandFinished();
            RaiseCanExecuteChanged();
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region public

        public AsyncCommand(Func<object, CancellationToken, Task<TResult>> command, Func<bool> canExecuteCondition = null)
        {
            if (canExecuteCondition != null)
                _canExecuteCondition = canExecuteCondition;
            _command = command;
            _cancelCommand = new CancelAsyncCommand();
        }

        public bool Enabled
        {
            get { return _enabled; }
            private set
            {
                _enabled = value;
                OnPropertyChanged();
            }
        }

        public NotifyTaskCompletion<TResult> Execution
        {
            get { return _execution; }
            private set
            {
                _execution = value;
                OnPropertyChanged();
            }
        }

        public ICommand CancelCommand => _cancelCommand;

        #endregion

        #region private

        private readonly CancelAsyncCommand _cancelCommand;
        private readonly Func<object, CancellationToken, Task<TResult>> _command;
        private readonly Func<bool> _canExecuteCondition = () => true;
        private NotifyTaskCompletion<TResult> _execution;
        private bool _enabled;

        #endregion
    }

    public sealed class AsyncCommand : AsyncCommand<object>
    {
        public AsyncCommand(Func<object, CancellationToken, Task<object>> command, Func<bool> canExecuteCondition = null)
            : base(command, canExecuteCondition)
        {
        }
    }
}
