using System;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;

namespace ITCC.HTTP.SslConfigUtil.GUI
{
    public class InputModel : IDataErrorInfo
    {
        private static readonly Regex SubjectNamePattern = new Regex(@"(?=^.{1,254}$)(^(?:(?!\d|-)[a-zA-Z0-9\-]{1,63}(?<!-)\.?)+(?:[a-zA-Z]{2,})$)", RegexOptions.Compiled);
        public string IpAddress { get; set; } = "127.0.0.1";
        public string Port { get; set; }
        public string SubjectName { get; set; }

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
                        if (!ushort.TryParse(Port,out port) || port == 0)
                            error = "Invalid port value. Port must be a number between 1 and 65535.";
                        break;
                    case nameof(SubjectName):
                        if (string.IsNullOrEmpty(SubjectName))
                            break;
                        if (!SubjectNamePattern.IsMatch(SubjectName))
                            error = "Invalid subjectname format.";
                        break;
                }
                return error;
            }
        }
        public string Error
        {
            get { throw new NotImplementedException(); }
        }
    }
}
