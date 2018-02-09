// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using ITCC.Logging.Core;
using ITCC.WPF.Enums;
using ITCC.WPF.Utils;
using Microsoft.Win32.SafeHandles;

namespace ITCC.WPF.Credentials
{
    /// <summary>
    ///     Used to store user credentials in Windows applications
    /// </summary>
    public class CredentialManager
    {
        private readonly string _applicationName;

        /// <summary>
        ///     Create an instance of <see cref="CredentialManager"/>
        /// </summary>
        /// <param name="applicationName">Current application name</param>
        public CredentialManager(string applicationName)
        {
            if (string.IsNullOrEmpty(applicationName))
                throw new ArgumentException(@"Application name must be not-empty string", nameof(applicationName));

            _applicationName = applicationName;
        }

        /// <summary>
        ///     Read first credential available for application
        /// </summary>
        /// <returns>Credential object, null in case of error</returns>
        public Credential ReadCredential()
        {
            try
            {
                var read = CredRead(_applicationName, CredentialType.Generic, 0, out IntPtr nCredPtr);
                if (!read)
                    return null;

                using (var critCred = new CriticalCredentialHandle(nCredPtr))
                {
                    var credential = critCred.GetCredential();
                    return ReadCredential(credential);
                }
            }
            catch (Exception exception)
            {
                Logger.LogException("CredentialManager", LogLevel.Warning, exception);
                return null;
            }
        }

        /// <summary>
        ///     Save user credentials for current application
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="secret">User secret</param>
        /// <param name="credentialPersistence">Persistence type</param>
        /// <returns>Zero in case of success</returns>
        public int WriteCredential(string userName, string secret, CredentialPersistence credentialPersistence = CredentialPersistence.LocalMachine)
        {
            var byteArray = Encoding.Unicode.GetBytes(secret);
            if (byteArray.Length > 512)
                throw new ArgumentOutOfRangeException(nameof(secret), @"The secret message has exceeded 512 bytes.");

            var credential = new PrivateCredential
            {
                AttributeCount = 0,
                Attributes = IntPtr.Zero,
                Comment = IntPtr.Zero,
                TargetAlias = IntPtr.Zero,
                Type = CredentialType.Generic,
                Persist = (uint)credentialPersistence,
                CredentialBlobSize = (uint)Encoding.Unicode.GetBytes(secret).Length,
                TargetName = Marshal.StringToCoTaskMemUni(_applicationName),
                CredentialBlob = Marshal.StringToCoTaskMemUni(secret),
                UserName = Marshal.StringToCoTaskMemUni(userName ?? Environment.UserName)
            };

            var written = CredWrite(ref credential, 0);
            var lastError = Marshal.GetLastWin32Error();

            Marshal.FreeCoTaskMem(credential.TargetName);
            Marshal.FreeCoTaskMem(credential.CredentialBlob);
            Marshal.FreeCoTaskMem(credential.UserName);

            if (written)
                return 0;

            throw new Exception($"CredWrite failed with the error code {lastError}.");
        }

        /// <summary>
        ///     Enumerate credentials available for current application
        /// </summary>
        /// <returns>List of available credentials</returns>
        public IReadOnlyList<Credential> EnumerateCrendentials()
        {
            var result = new List<Credential>();

            var ret = CredEnumerate(null, 0, out int count, out IntPtr pCredentials);
            if (ret)
            {
                for (var n = 0; n < count; n++)
                {
                    var credential = Marshal.ReadIntPtr(pCredentials, n * Marshal.SizeOf(typeof(IntPtr)));
                    result.Add(ReadCredential((PrivateCredential)Marshal.PtrToStructure(credential, typeof(PrivateCredential))));
                }
            }
            else
            {
                var lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError);
            }

            return result;
        }

        private static Credential ReadCredential(PrivateCredential credential)
        {
            var applicationName = Marshal.PtrToStringUni(credential.TargetName);
            var userName = Marshal.PtrToStringUni(credential.UserName);
            string insecureSecret = null;
            if (credential.CredentialBlob != IntPtr.Zero)
            {
                insecureSecret = Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);
            }

            var secret = SecureStringHelper.GetSecureString(insecureSecret);
            return new Credential(credential.Type, applicationName, userName, secret);
        }

        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, CredentialType type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredWrite([In] ref PrivateCredential userCredential, [In] uint flags);

        [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredEnumerate(string filter, int flag, out int count, out IntPtr pCredentials);

        [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        private static extern bool CredFree([In] IntPtr cred);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct PrivateCredential
        {
            private readonly uint Flags;
            public CredentialType Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            private readonly System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;
        }

        private sealed class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
        {
            public CriticalCredentialHandle(IntPtr preexistingHandle)
            {
                SetHandle(preexistingHandle);
            }

            public PrivateCredential GetCredential()
            {
                if (IsInvalid)
                    throw new InvalidOperationException("Invalid CriticalHandle!");

                var credential = (PrivateCredential)Marshal.PtrToStructure(handle, typeof(PrivateCredential));
                return credential;
            }

            protected override bool ReleaseHandle()
            {
                if (IsInvalid)
                    return false;

                CredFree(handle);
                SetHandleAsInvalid();
                return true;
            }
        }
    }
}
