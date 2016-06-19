using System.Windows;
using ITCC.Logging;
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
                _logWindow = new LogWindow(_observableLogger);

            _logWindow.Show();
            Logger.LogEntry("Test", LogLevel.Info, "Opened");

            for (var i = 0; i < 50; ++i)
            {
                Logger.LogEntry("Test", LogLevel.Trace, $"Sample message #{i}");
            }

        }

        private LogWindow _logWindow;
        private readonly ObservableLogger _observableLogger;
    }
}
