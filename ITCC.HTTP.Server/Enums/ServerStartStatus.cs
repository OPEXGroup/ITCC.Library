// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.HTTP.Server.Enums
{
    /// <summary>
    ///     <see cref="Core.StaticServer&lt;TAccount&gt;.Start" />
    /// </summary>
    public enum ServerStartStatus
    {
        /// <summary>
        ///     Server started correctly
        /// </summary>
        Ok,

        /// <summary>
        ///     Failed to bind to specified port
        /// </summary>
        BindingError,

        /// <summary>
        ///     Failed to retrieve HTTPS certificate
        /// </summary>
        CertificateError,

        /// <summary>
        ///     Bad Start() parameters
        /// </summary>
        BadParameters,

        /// <summary>
        ///     Server is up and running already
        /// </summary>
        AlreadyStarted,

        UnknownError
    }
}