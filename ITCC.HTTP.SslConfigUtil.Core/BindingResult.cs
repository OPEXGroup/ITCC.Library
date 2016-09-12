namespace ITCC.HTTP.SslConfigUtil.Core
{
    public enum BindingResult
    {
        Unspecified,

        Ok,

        AssemblyLoadFailed,

        InvalidIpAddress,
        InvalidPortValue,
        InvalidCertificateCn,


        SslCertificateNotFound,

        SslCertificateGenerationFailed,

        PortIsAlreadyAssigned,
        SslCertificateExpired


    }
}