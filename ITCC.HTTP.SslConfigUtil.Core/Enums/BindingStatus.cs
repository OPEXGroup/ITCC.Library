﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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