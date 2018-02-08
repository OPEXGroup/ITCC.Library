// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.WPF.Enums;

namespace ITCC.WPF.Credentials
{
    /// <summary>
    ///     Represents Windows application credential
    /// </summary>
    public class Credential
    {
        public CredentialType CredentialType { get; }
        public string ApplicationName { get; }
        public string UserName { get; }
        public string Password { get; }

        public Credential(CredentialType credentialType, string applicationName, string userName, string password)
        {
            ApplicationName = applicationName;
            UserName = userName;
            Password = password;
            CredentialType = credentialType;
        }

        public override string ToString() => $"CredentialType: {CredentialType}, ApplicationName: {ApplicationName}, UserName: {UserName}, Password: {Password}";
    }
}
