// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.WPF.Enums
{
    /// <summary>
    ///     Credential persistence type
    /// </summary>
    public enum CredentialPersistence : uint
    {
        Session = 1,
        LocalMachine,
        Enterprise
    }
}
