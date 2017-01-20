// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ITCC.Logging.Core;
using ITCC.WPF.Loggers;
using ITCC.WPF.Windows;

namespace ITCC.WPF.Testing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        public MainWindow()
        {
            InitializeComponent();

            Logger.Level = LogLevel.Trace;
            _observableLogger = new ObservableLogger(1000, App.RunOnUiThreadAsync);
            Logger.RegisterReceiver(_observableLogger);

            Logger.LogEntry("Test", LogLevel.Info, "Started");
        }

        public void Dispose() => _observableLogger.Dispose();

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

            for (int j = 0; j < 15; j++)
            {
                for (var i = 0; i < 1; ++i)
                {
                    Logger.LogEntry("Test", LogLevel.Trace, $"Sample message #{i}");
                }
                Thread.Sleep(1);
            }
            
        }

        private LogWindow _logWindow;
        private readonly ObservableLogger _observableLogger;

        private async void DeadlockTestButton_OnClick(object sender, RoutedEventArgs e)
        {
            var rnd = new Random();
            for (var i = 0; i < 50; ++i)
            {
                var str = Guid.NewGuid().ToString();
                var emptyCount = rnd.Next(5);
                for (var j = 0; j < emptyCount; ++j)
                {
                    str += "\n";
                }
                str += "END";
                Logger.LogEntry("TEST", LogLevel.Trace, str);
                await Task.Delay(20);
            }

            //Parallel.For(0, 6, i =>
            //{
            //    for (var j = 0; j < 50; ++j)
            //        Logger.LogEntry("TEST", LogLevel.Trace, $"Maybe thread #{i} (Actually {Thread.CurrentThread.ManagedThreadId})");
            //});
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e) => _logWindow?.Close();
        
    }
}
