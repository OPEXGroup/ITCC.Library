using ITCC.Logging;
using ITCC.UI.Utils;
using ITCC.UI.ViewModels;
using static ITCC.UI.Common.Delegates;

namespace ITCC.UI.Loggers
{
    public class ObservableLogger : ILogReceiver
    {
        public ObservableLogger(int capacity, UiThreadRunner uiThreadRunner)
        {
            LogEntryCollection = new BoundedObservableCollection<LogEntryEventArgsViewModel>(capacity);
            _uiThreadRunner = uiThreadRunner;
        }

        public BoundedObservableCollection<LogEntryEventArgsViewModel> LogEntryCollection { get; }
        public LogLevel Level { get; set; }

        public void WriteEntry(object sender, LogEntryEventArgs args)
        {
            if (args.Level > Level)
                return;
            _uiThreadRunner.Invoke(() => { LogEntryCollection.Add(new LogEntryEventArgsViewModel(args)); });
        }

        private readonly UiThreadRunner _uiThreadRunner;
    }
}