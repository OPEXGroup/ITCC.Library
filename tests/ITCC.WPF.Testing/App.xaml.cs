// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ITCC.WPF.Testing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static async Task RunOnUiThreadAsync(Action action) => await Current.Dispatcher.InvokeAsync(action);
    }
}
