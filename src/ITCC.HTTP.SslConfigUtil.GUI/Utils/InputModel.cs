// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ITCC.HTTP.SslConfigUtil.GUI.Enums;

namespace ITCC.HTTP.SslConfigUtil.GUI.Utils
{
    public class InputModel : IDataErrorInfo, INotifyPropertyChanged
    {
        private static readonly Regex SubjectNamePattern = new Regex(@"(?=^.{1,254}$)(^(?:(?!\d|-)[a-zA-Z0-9\-]{1,63}(?<!-)\.?)+(?:[a-zA-Z]{2,})$)", RegexOptions.Compiled);

        #region Properties

        public string AssemblyPath
        {
            get { return _assemblyPath; }
            set { _assemblyPath = value; OnPropertyChanged(); }
        }
        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; OnPropertyChanged(); }
        }
        public string Port
        {
            get { return _port; }
            set { _port = value; OnPropertyChanged(); }
        }
        public bool CertificateChosen
        {
            get { return _certificateChosen; }
            set { _certificateChosen = value; OnPropertyChanged(); }
        }
        public string SubjectName
        {
            get { return _subjectName; }
            set { _subjectName = value; OnPropertyChanged(); }
        }
        public string CertificatePath
        {
            get { return _certificatePath; }
            set { _certificatePath = value; OnPropertyChanged(); }
        }
        public string CertificatePassword
        {
            get { return _certificatepassword; }
            set { _certificatepassword = value; OnPropertyChanged(); }
        }
        public CertificateMode CertificateMode
        {
            get { return _certificateMode; }
            set { _certificateMode = value; OnPropertyChanged(); }
        }

        public bool IsBindButtonEnabled
        {
            get { return _isBindButtonEnabled; }
            set
            {
                if (value == _isBindButtonEnabled)
                    return;
                _isBindButtonEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool IsUnbindButtonEnabled
        {
            get { return _isUnbindButtonEnabled; }
            set
            {
                if (value == _isUnbindButtonEnabled)
                    return;
                _isUnbindButtonEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool ChooseCertFromStoreEnabled
        {
            get { return _chooseCertFromStoreEnabled; }
            set
            {
                if (value == _chooseCertFromStoreEnabled)
                    return;
                _chooseCertFromStoreEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool ChooseCertFromFileEnabled
        {
            get { return _chooseCertFromFileEnabled; }
            set
            {
                if (value == _chooseCertFromFileEnabled)
                    return;
                _chooseCertFromFileEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool ChooseCertGenerationEnabled
        {
            get { return _chooseCertGenerationEnabled; }
            set
            {
                if (value == _chooseCertGenerationEnabled)
                    return;
                _chooseCertGenerationEnabled = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                var error = string.Empty;
                switch (columnName)
                {
                    case nameof(IpAddress):
                        IPAddress ipAddress;
                        if (string.IsNullOrEmpty(IpAddress))
                            break;
                        if (!IPAddress.TryParse(IpAddress, out ipAddress))
                            error = "IP address has incorrect format.";
                        break;
                    case nameof(Port):
                        ushort port;
                        if (string.IsNullOrEmpty(Port))
                            break;
                        if (!ushort.TryParse(Port, out port) || port == 0)
                            error = "Invalid port value. Port must be a number between 1 and 65535.";
                        break;
                    case nameof(SubjectName):
                        if (string.IsNullOrEmpty(SubjectName))
                            break;

                        if (!SubjectNamePattern.IsMatch(SubjectName))
                            error = "Invalid subjectname format.";
                        break;
                    case nameof(AssemblyPath):
                        if (string.IsNullOrEmpty(CertificatePath))
                            break;

                        if (AssemblyPath.Length > 260)
                            error = "Assembly path is too long.";
                        break;
                    case nameof(CertificatePath):
                        if (string.IsNullOrEmpty(CertificatePath))
                            break;

                        if (CertificatePath.Length > 260)
                            error = "Certificate path is too long.";
                        break;
                }
                return error;
            }
        }
        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            switch (_certificateMode)
            {
                case CertificateMode.Undefined:
                    ChooseCertFromStoreEnabled = false;
                    ChooseCertFromFileEnabled = false;
                    ChooseCertGenerationEnabled = false;
                    break;
                case CertificateMode.FromStore:
                    ChooseCertFromStoreEnabled = true;
                    ChooseCertFromFileEnabled = false;
                    ChooseCertGenerationEnabled = false;
                    break;
                case CertificateMode.FromFile:
                    ChooseCertFromStoreEnabled = false;
                    ChooseCertFromFileEnabled = true;
                    ChooseCertGenerationEnabled = false;
                    break;
                case CertificateMode.SelfSigned:
                    ChooseCertFromStoreEnabled = false;
                    ChooseCertFromFileEnabled = false;
                    ChooseCertGenerationEnabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!string.IsNullOrEmpty(_ipAddress) && !string.IsNullOrEmpty(_port) &&
                !string.IsNullOrEmpty(_assemblyPath))
            {
                IsUnbindButtonEnabled = true;
                switch (_certificateMode)
                {
                    case CertificateMode.Undefined:
                        IsBindButtonEnabled = false;
                        break;
                    case CertificateMode.FromStore:
                        IsBindButtonEnabled = _certificateChosen;
                        break;
                    case CertificateMode.FromFile:
                        IsBindButtonEnabled = !string.IsNullOrEmpty(_certificatePath);
                        break;
                    case CertificateMode.SelfSigned:
                        IsBindButtonEnabled = !string.IsNullOrEmpty(_subjectName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
                IsUnbindButtonEnabled = false;
        }

        #endregion

        #region Fields
        private string _assemblyPath;
        private string _ipAddress = "127.0.0.1";
        private string _port;
        private string _subjectName;
        private bool _certificateChosen;
        private string _certificatePath;
        private string _certificatepassword;
        private CertificateMode _certificateMode;

        private bool _chooseCertFromStoreEnabled;
        private bool _chooseCertFromFileEnabled;
        private bool _chooseCertGenerationEnabled;

        private bool _isBindButtonEnabled;
        private bool _isUnbindButtonEnabled;
        #endregion
    }
}