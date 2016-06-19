using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ITCC.Logging;
using ITCC.UI.Loggers;
using ITCC.UI.Utils;
using ITCC.UI.ViewModels;

namespace ITCC.UI.Windows
{
    /// <summary>
    ///     Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        public LogWindow(ObservableLogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            InitializeComponent();

            LogDataGrid.ItemsSource = logger.LogEntryCollection;
            WindowState = WindowState.Maximized;
        }

        private void LogDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            DataGridHelper.HandleAutoGeneratingColumn(sender, e, "Сообщение");
        }

        private void LogDataGrid_OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row;
            var logEntryModelView = (LogEntryEventArgsViewModel)row.Item;


            switch (logEntryModelView.Subject.Level)
            {
                case LogLevel.Critical:
                    row.Background = Brushes.Brown;
                    break;
                case LogLevel.Error:
                    row.Background = Brushes.Red;
                    break;
                case LogLevel.Warning:
                    row.Background = Brushes.Orange;
                    break;
                case LogLevel.Info:
                    row.Background = Brushes.Aqua;
                    break;
                case LogLevel.Debug:
                    row.Background = Brushes.CadetBlue;
                    break;
                case LogLevel.Trace:
                    row.Background = Brushes.White;
                    break;
            }
        }

        private void LogWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LogDataGrid.Width = ActualWidth;
        }

        private void LogWindow_OnStateChanged(object sender, EventArgs e)
        {
            LogDataGrid.Width = ActualWidth;
        }

        private void LogDataGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LogDataGrid.Columns.Last().Width = new DataGridLength(420, DataGridLengthUnitType.Star);
        }
    }
}