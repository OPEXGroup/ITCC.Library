namespace ITCC.HTTP.SslConfigUtil.Core.Enums
{
    internal enum LoadAssemblyStatus
    {
        Ok,
        IsNullOrWhiteSpace,
        PathTooLong,
        DirectoryNotFound,
        FileNotFound,
        BadAssemblyFormat,
        AccessDenied,
        UnknownError
    }
}