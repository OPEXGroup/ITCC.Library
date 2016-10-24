using System;
using System.Windows;

namespace ITCC.WPF.Testing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly object DispactherQueueLock = new object();
        public static void RunOnUiThread(Action action) => Current?.Dispatcher?.BeginInvoke(action);
    }
}
