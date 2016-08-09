using ITCC.Logging.Core;
using ITCC.Logging.Core.Interfaces;
using ITCC.UI.Utils;
using ITCC.UI.ViewModels;
using static ITCC.UI.Common.Delegates;

namespace ITCC.UI.Loggers
{
    public class ObservableLogger : ILogReceiver
    {
        #region ILogReceiver
        public LogLevel Level { get; set; }

        public void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;
            
            _uiThreadRunner.Invoke(() => { LogEntryCollection.Add(new LogEntryEventArgsViewModel(args)); });
        }
        #endregion

        #region public
        public ObservableLogger(int capacity, UiThreadRunner uiThreadRunner)
        {
            Level = Logger.Level;
            LogEntryCollection = new BoundedObservableCollection<LogEntryEventArgsViewModel>(capacity);
            _uiThreadRunner = uiThreadRunner;
        }

        public ObservableLogger(LogLevel level, int capacity, UiThreadRunner uiThreadRunner)
        {
            Level = level;
            LogEntryCollection = new BoundedObservableCollection<LogEntryEventArgsViewModel>(capacity);
            _uiThreadRunner = uiThreadRunner;
        }

        public BoundedObservableCollection<LogEntryEventArgsViewModel> LogEntryCollection { get; }

        #endregion

        #region private
        private readonly UiThreadRunner _uiThreadRunner;
        #endregion
    }
}