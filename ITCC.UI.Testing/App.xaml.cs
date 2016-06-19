using System;
using System.Windows;

namespace ITCC.UI.Testing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void RunOnUiThread(Action action)
        {
            Current.Dispatcher.Invoke(action);
        }
    }
}
