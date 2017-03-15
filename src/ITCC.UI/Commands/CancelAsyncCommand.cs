// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Threading;
using System.Windows.Input;

namespace ITCC.UI.Commands
{
    internal sealed class CancelAsyncCommand : ICommand
    {
        #region ICommand
        public bool CanExecute(object parameter)
            => _commandExecuting && !_cts.IsCancellationRequested;

        public void Execute(object parameter)
        {
            _cts.Cancel();
            RaiseCanExecuteChanged();
        }

        public event EventHandler CanExecuteChanged;

        private void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        #endregion

        #region public

        public CancellationToken Token => _cts.Token;

        public void NotifyCommandStarting()
        {
            _commandExecuting = true;
            _cts = new CancellationTokenSource();
            RaiseCanExecuteChanged();
        }
        public void NotifyCommandFinished()
        {
            _commandExecuting = false;
            RaiseCanExecuteChanged();
        }

        #endregion

        #region private

        private bool _commandExecuting;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        #endregion
    }
}
