using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ITCC.WPF.Commands
{
    public class DelegateCommand : ICommand, INotifyPropertyChanged
    {
        #region ICommand
        public bool CanExecute(object parameter)
        {
            var value = _executionCondition.Invoke();
            Enabled = value;
            return value;
        }

        public void Execute(object parameter)
        {
            if (_hasParam)
                _parameterizedCommand(parameter);
            else
                _command();
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region public

        public DelegateCommand(Action command, Func<bool> executionCondition = null)
        {
            _command = command;
            if (executionCondition != null)
                _executionCondition = executionCondition;
        }
        public DelegateCommand(Action<object> command, Func<bool> executionCondition = null)
        {
            _hasParam = true;
            _parameterizedCommand = command;
            if (executionCondition != null)
                _executionCondition = executionCondition;
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

        #endregion

        #region private

        private readonly Action _command;
        private readonly Action<object> _parameterizedCommand;
        private readonly Func<bool> _executionCondition = () => true;
        private readonly bool _hasParam;
        private bool _enabled;

        #endregion
    }
}
