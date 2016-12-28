// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ITCC.Logging.Core;
using ITCC.WPF.Loggers;
using ITCC.WPF.Utils;
using ITCC.WPF.ViewModels;

namespace ITCC.WPF.Windows
{
    /// <summary>
    ///     Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        private static readonly Dictionary<LogLevel, SolidColorBrush> ColorDict = new Dictionary<LogLevel, SolidColorBrush>
        {
            {LogLevel.Critical, Brushes.Brown},
            {LogLevel.Error, Brushes.Red },
            {LogLevel.Warning, Brushes.Orange },
            {LogLevel.Info, Brushes.Aqua },
            {LogLevel.Debug, Brushes.CadetBlue },
            {LogLevel.Trace, Brushes.White }
        };

        public LogWindow(ObservableLogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            InitializeComponent();

            LogDataGrid.ItemsSource = logger.LogEntryCollection;
            WindowState = WindowState.Maximized;

            var names = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().Select(EnumHelper.LogLevelName).ToList();
            LocalLogLevelComboBox.ItemsSource = names;
            LocalLogLevelComboBox.SelectedValue = EnumHelper.LogLevelName(Logger.Level); 
        }

        private void LogDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e) => DataGridHelper.HandleAutoGeneratingColumn(sender, e);

        private void LogDataGrid_OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row;
            var viewModel = (LogEntryEventArgsViewModel)row.Item;
            row.Background = ColorDict[viewModel.Subject.Level];
        }

        private void LocalLogLevelComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LocalLogLevelComboBox.SelectedValue == null)
                return;

            var newLevel = EnumHelper.LogLevelByName(LocalLogLevelComboBox.SelectedValue as string);
            if (newLevel == Logger.Level)
                return;
            Logger.LogEntry("LOGGER", LogLevel.Debug, $"Changing loglevel from {Logger.Level} to {newLevel}");
            Logger.Level = newLevel;
        }

        private async void LogWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var border = VisualTreeHelper.GetChild(LogDataGrid, 0) as Decorator;
            var scroll = border?.Child as ScrollViewer;
            if (scroll != null)
            {
                scroll.CanContentScroll = false;
                // A bit dirty hack but it works...
                await Task.Delay(50);
                scroll.SetAlwaysScrollToEnd(true);
            }
        }
    }
}