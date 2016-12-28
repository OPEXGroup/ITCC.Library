// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ITCC.HTTP.SslConfigUtil.Core;
using ITCC.HTTP.SslConfigUtil.Core.Enums;
using ITCC.HTTP.SslConfigUtil.Core.Utils;
using ITCC.HTTP.SslConfigUtil.Core.Views;
using ITCC.HTTP.SslConfigUtil.GUI.Enums;
using ITCC.HTTP.SslConfigUtil.GUI.Utils;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Loggers;
using ITCC.Logging.Windows.Loggers;

namespace ITCC.HTTP.SslConfigUtil.GUI
{
   
    
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker _bindBackgroundWorker;
        private readonly BackgroundWorker _unbindBackgroundWorker;
        public ObservableCollection<CertificateView> CertificateCollection { get; set; } = new ObservableCollection<CertificateView>();
        private readonly InputModel _input = new InputModel();
        private CertificateView _chosenCertificate;
        private bool _unsafeBinding;

        public MainWindow()
        {
            InitializeComponent();
            Logger.Level = LogLevel.Trace;
            Logger.RegisterReceiver(new DebugLogger(), true);
            _bindBackgroundWorker = (BackgroundWorker)FindResource("BindBackgroundWorker");
            _unbindBackgroundWorker = (BackgroundWorker)FindResource("UnbindBackgroundWorker");
            CertificatesComboBox.DataContext = this;
            DataContext = _input;
            CertificatesComboBox.SelectionChanged += (sender, args) =>
            {
                var control = sender as ComboBox;
                if (control == null)
                    throw new Exception();

                var item = control.SelectedItem as CertificateView;
                if (item == null)
                    return;
                
                _chosenCertificate = item;
                _input.CertificateChosen = true;
            };
            CertificatesComboBox.DropDownOpened += (sender, args) =>
            {

                var selectedThumbprint = _chosenCertificate?.Thumbprint;
                CertificateCollection.Clear();
                foreach (var cert in Binder.GetCertificateList())
                    CertificateCollection.Add(cert);
                if (selectedThumbprint == null)
                    return;
                var comboBox = sender as ComboBox;
                if (comboBox != null)
                    comboBox.SelectedItem =
                        CertificateCollection.FirstOrDefault(x => x.Thumbprint == selectedThumbprint);
            };
        }

      
        private void ChooseFromStore_OnChecked(object sender, RoutedEventArgs e) => _input.CertificateMode = CertificateMode.FromStore;
        private void ChooseFromFile_OnChecked(object sender, RoutedEventArgs e) => _input.CertificateMode = CertificateMode.FromFile;
        private void CreateNew_OnChecked(object sender, RoutedEventArgs e) => _input.CertificateMode = CertificateMode.SelfSigned;
        private void BrowseAssembly_OnClick(object sender, RoutedEventArgs e)
        {
            var filePicker = new System.Windows.Forms.OpenFileDialog
            {
                Title = "Select your aplication",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Executable Files (.exe) | *.exe"
            };
            var result = filePicker.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    AssemblyPathTextbox.Text = filePicker.FileName;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                case System.Windows.Forms.DialogResult.None:
                case System.Windows.Forms.DialogResult.Abort:
                case System.Windows.Forms.DialogResult.Retry:
                case System.Windows.Forms.DialogResult.Ignore:
                case System.Windows.Forms.DialogResult.Yes:
                case System.Windows.Forms.DialogResult.No:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void BrowseCertificate_OnClick(object sender, RoutedEventArgs e)
        {
            var filePicker = new System.Windows.Forms.OpenFileDialog
            {
                Title = "Select certificate file",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Certificate (.pfx) | *.pfx"
            };
            var result = filePicker.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                CertificatePathTextbox.Text = filePicker.FileName;

        }
        private static SecureString ToSecureString(string unsecurePassword)
        {
            if (string.IsNullOrEmpty(unsecurePassword))
                return null;

            var password = new SecureString();
            foreach (var c in unsecurePassword)
                password.AppendChar(c);
            return password;
        }

        private void BindButton_OnClick(object sender, RoutedEventArgs e)
        {
            InputRootGrid.IsEnabled = false;
            ButtonsGrid.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            ResultTextblock.Text = string.Empty;

            _bindBackgroundWorker.RunWorkerAsync(new RunBindingParams
            {
                Mode = _input.CertificateMode,
                AssemplyPath = AssemblyPathTextbox.Text,
                Ip = _input.IpAddress,
                Port = _input.Port,
                SubjectName = _input.SubjectName,
                Thumbprint = _chosenCertificate?.Thumbprint,
                CertificatePath = CertificatePathTextbox.Text,
                Password = ToSecureString(CertificatePasswordBox.Password),
                UnsafeBinding = _unsafeBinding
            });
        }
        private void BindBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BindingResult result;
            var args = e.Argument as RunBindingParams;
            if (args == null)
                throw new ArgumentException();

            switch (_input.CertificateMode)
            {
                case CertificateMode.FromStore:
                    result = Binder.Bind(args.AssemplyPath, args.Ip, args.Port, new CertificateThumbprintBindingParams(args.Thumbprint),args.UnsafeBinding);
                    break;
                case CertificateMode.FromFile:
                    result = Binder.Bind(args.AssemplyPath, args.Ip, args.Port, new CertificateFileBindingParams(args.CertificatePath, args.Password), args.UnsafeBinding);
                    break;
                case CertificateMode.SelfSigned:
                    result = Binder.Bind(args.AssemplyPath, args.Ip, args.Port, new CertificateSubjectnameParams(args.SubjectName), args.UnsafeBinding);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            e.Result = result;
        }
        private void BindBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = e.Result as BindingResult;
            if (result == null)
                throw new Exception();

            ResultTextblock.Text = EnumHelper.DisplayName(result.Status);
            if (!string.IsNullOrEmpty(result.Reason))
                ResultTextblock.Text += $"\n{result.Reason}";
            ResultTextblock.Foreground = result.Status != BindingStatus.Ok ? new SolidColorBrush(Colors.DarkRed) : new SolidColorBrush(Colors.DarkGreen);

            InputRootGrid.IsEnabled = true;
            ButtonsGrid.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Collapsed;
        }


        private void UnbindButton_OnClick(object sender, RoutedEventArgs e)
        {
            InputRootGrid.IsEnabled = false;
            ButtonsGrid.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            ResultTextblock.Text = string.Empty;

            _unbindBackgroundWorker.RunWorkerAsync(new RunUnbindingParams()
            {
                ApplicationPath = AssemblyPathTextbox.Text,
                IpAddress = _input.IpAddress,
                Port = _input.Port,
                Unsafe = _unsafeBinding
            });
        }
        private void UnbindBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = e.Argument as RunUnbindingParams;
            if (args == null)
                throw new ArgumentException();
            e.Result = Binder.Unbind(args.ApplicationPath, args.IpAddress, args.Port, args.Unsafe);
        }
        private void UnbindBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = e.Result as UnbindResult;
            if (result == null)
                throw new Exception();

            InputRootGrid.IsEnabled = true;
            ButtonsGrid.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Collapsed;

            ResultTextblock.Text = EnumHelper.DisplayName(result.Status);
            
            ResultTextblock.Foreground = result.Status != UnbindStatus.Ok ? new SolidColorBrush(Colors.DarkRed) : new SolidColorBrush(Colors.DarkGreen);
        }

        private void UnsafeBinding_OnChecked(object sender, RoutedEventArgs e)
        {
            var warningDialog = MessageBox.Show("Safe binding prevents from occasionally binding rewrite. " +
                                                "If you want to enable Unsafe binding MAKE SURE you enter correct ip address and password. " +
                                                "Enable unsafe binding?", "Unsafe binding", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (warningDialog != MessageBoxResult.Yes)
            {
                UnsafeBindingCheckbox.IsChecked = false;
                return;
            }

            _unsafeBinding = true;
        }
        private void UnsafeBinding_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _unsafeBinding = false;
        }
    }
}