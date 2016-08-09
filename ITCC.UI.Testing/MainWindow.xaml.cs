using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ITCC.Logging.Core;
using ITCC.UI.Loggers;
using ITCC.UI.Windows;

namespace ITCC.UI.Testing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Logger.Level = LogLevel.Trace;
            _observableLogger = new ObservableLogger(1000, App.RunOnUiThread);
            Logger.RegisterReceiver(_observableLogger);

            Logger.LogEntry("Test", LogLevel.Info, "Started");
        }

        private void LogWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_logWindow == null)
            {
                _logWindow = new LogWindow(_observableLogger);
                _logWindow.Show();
                Activate();
            }
            else
            {
                _logWindow.Activate();
            }
            
            Logger.LogEntry("Test", LogLevel.Info, "Opened");

            for (var i = 0; i < 50; ++i)
            {
                Logger.LogEntry("Test", LogLevel.Trace, $"Sample message #{i}");
            }
        }

        private LogWindow _logWindow;
        private readonly ObservableLogger _observableLogger;

        private void DeadlockTestButton_OnClick(object sender, RoutedEventArgs e)
        {
            Parallel.For(0, 6, i =>
            {
                for (var j = 0; j < 50; ++j)
                    Logger.LogEntry("TEST", LogLevel.Trace, $"Maybe thread #{i} (Actually {Thread.CurrentThread.ManagedThreadId})");
            });
        }
    }
}
