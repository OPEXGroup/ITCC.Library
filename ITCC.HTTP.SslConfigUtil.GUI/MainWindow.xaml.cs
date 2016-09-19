using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ITCC.HTTP.SslConfigUtil.Core;
using ITCC.HTTP.SslConfigUtil.Core.Views;

namespace ITCC.HTTP.SslConfigUtil.GUI
{
    public partial class MainWindow : Window
    {
        private enum CertificateMode
        {
            Undefined,
            FromStore,
            FromFile,
            SelfSigned
        }
        public ObservableCollection<CertificateView> CertificateCollection { get; } = new ObservableCollection<CertificateView>();
        private readonly InputModel _input = new InputModel();
        private CertificateMode _certificateMode = CertificateMode.Undefined;


        public MainWindow()
        {
            InitializeComponent();
            CertificatesComboBox.DataContext = this;
            foreach (var cert in CertificateController.GetCertificates())
                CertificateCollection.Add(cert);

            this.DataContext = _input;
        }

        private void ChooseFromStore_OnChecked(object sender, RoutedEventArgs e)
        {
            CertificatesComboBox.IsEnabled = true;
            FromFileGrid.IsEnabled = false;
            SubjectNameTextBox.IsEnabled = false;
            _certificateMode = CertificateMode.FromStore;
        }
        private void ChooseFromFile_OnChecked(object sender, RoutedEventArgs e)
        {
            CertificatesComboBox.IsEnabled = false;
            FromFileGrid.IsEnabled = true;
            SubjectNameTextBox.IsEnabled = false;
            _certificateMode = CertificateMode.FromFile;
        }
        private void CreateNew_OnChecked(object sender, RoutedEventArgs e)
        {
            CertificatesComboBox.IsEnabled = false;
            FromFileGrid.IsEnabled = false;
            SubjectNameTextBox.IsEnabled = true;
            _certificateMode = CertificateMode.SelfSigned;
        }

        private async void BindButton_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Click started");
            AssemblyPathTextbox.IsEnabled = false;
            BrowseAssembly.IsEnabled = false;
            IpAddressTextBox.IsEnabled = false;
            PortTextBox.IsEnabled = false;
            CertificatesComboBox.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;

            await Task.Factory.StartNew(() =>
            {
                switch (_certificateMode)
                {
                    case CertificateMode.Undefined:
                        break;
                    case CertificateMode.FromStore:
                        break;
                    case CertificateMode.FromFile:
                        break;
                    case CertificateMode.SelfSigned:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            AssemblyPathTextbox.IsEnabled = true;
            BrowseAssembly.IsEnabled = true;
            IpAddressTextBox.IsEnabled = true;
            PortTextBox.IsEnabled = true;
            CertificatesComboBox.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Collapsed;

            Debug.WriteLine("Click ended");
        }

        private void BrowseAssembly_OnClick(object sender, RoutedEventArgs e)
        {
            var filePicker = new OpenFileDialog
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
            
        }
    }
}