namespace ITCC.HTTP.SslConfigUtil.Core
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