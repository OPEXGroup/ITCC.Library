namespace ITCC.HTTP.Server.Enums
{
    /// <summary>
    ///     <see cref="Isengard.HTTP.Server.Start" />
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