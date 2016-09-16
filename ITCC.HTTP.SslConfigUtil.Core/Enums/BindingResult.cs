namespace ITCC.HTTP.SslConfigUtil.Core.Enums
{
    public enum BindingResult
    {
        Unspecified,

        Ok,

        AssemblyLoadFailed,

        InvalidIpAddress,
        InvalidPortValue,
        InvalidCertificateCn,
        InvalidSubjectnameFormat,

        SslCertificateNotFound,

        SslCertificateGenerationFailed,

        PortIsAlreadyAssigned,
        SslCertificateExpired


    }
}