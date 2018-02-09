// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Runtime.InteropServices;
using System.Security;
using ITCC.Logging.Core;

namespace ITCC.WPF.Utils
{
    public static class SecureStringHelper
    {
        private const string LogScope = "SECUR_STRING";

        [SecurityCritical]
        public static string GetInsecureString(SecureString secureString)
        {
            if (secureString == null)
                return null;

            try
            {
                var passwordBstr = Marshal.SecureStringToBSTR(secureString);
                return Marshal.PtrToStringBSTR(passwordBstr);
            }
            catch (Exception exception)
            {
                Logger.LogException(LogScope, LogLevel.Error, exception);
                return string.Empty;
            }
        }

        [SecurityCritical]
        public static SecureString GetSecureString(string insecureString)
        {
            if (string.IsNullOrEmpty(insecureString))
            {
                Logger.LogEntry(LogScope, LogLevel.Warning, "Null or empty input string");
                return null;
            }
            var secureString = new SecureString();
            foreach (var c in insecureString)
                secureString.AppendChar(c);

            return secureString;
        }

        [SecurityCritical]
        public static int GetSecureStringLength(SecureString secureString) => GetInsecureString(secureString)?.Length ?? 0;

        [SecurityCritical]
        public static bool IsMatch(SecureString secureString1, SecureString secureString2)
        {
            if (secureString1 == null || secureString2 == null)
                return false;

            var insecureString1 = GetInsecureString(secureString1);
            var insecureString2 = GetInsecureString(secureString2);

            if (string.IsNullOrEmpty(insecureString1) || string.IsNullOrEmpty(insecureString2))
                return false;

            return insecureString1 == insecureString2;
        }
    }
}
