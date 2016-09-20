namespace ITCC.HTTP.SslConfigUtil.Core.Enums
{
    public enum BindingStatus
    {
        Unspecified,
        Ok,
        InvalidParams,
        InvalidSubjectnameFormat,
        SslCertFromFileError,
        SslCertificateNotFound,
        SslCertificateGenerationFailed,
        SslCertificateExpired,
        PortIsAlreadyAssigned,
        UnbindingError,
        UnknownError
    }
}