// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ITCC.Logging.Core;

namespace ITCC.UI.Commands
{
    public sealed class NotifyTaskCompletion<TResult> : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public

        internal NotifyTaskCompletion(Task<TResult> task)
        {
            Task = task;
            if (!task.IsCompleted)
                TaskCompletion = WatchTaskAsync(task);
        }

        public Task<TResult> Task { get; private set; }
        public Task TaskCompletion { get; private set; }
        public TResult Result => IsSuccessfullyCompleted ? Task.Result : default(TResult);
        public TaskStatus Status => Task.Status;
        public bool IsCompleted => Task.IsCompleted;
        public bool IsNotCompleted => !Task.IsCompleted;
        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;
        public bool IsCanceled => Task.IsCanceled;
        public bool IsFaulted => Task.IsFaulted;
        public AggregateException Exception => Task.Exception;
        public Exception InnerException => Exception?.InnerException;
        public string ErrorMessage => InnerException?.Message;

        #endregion

        #region private

        private async Task WatchTaskAsync(Task task)
        {
            if (task == null)
            {
                Logger.LogEntry("NOTIFY TASK", LogLevel.Debug, "WatchTaskAsync(null)");
                return;
            }
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                Logger.LogException("NOTIFY TASK", LogLevel.Warning, exception);
            }
            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged(this, new PropertyChangedEventArgs(nameof(Status)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsNotCompleted)));
            if (task.IsCanceled)
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
            else if (task.IsFaulted)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(Exception)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(InnerException)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
            else
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(Result)));
            }
        }

        #endregion
    }
}
