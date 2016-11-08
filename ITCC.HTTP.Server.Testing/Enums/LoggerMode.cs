using System;

namespace ITCC.HTTP.Server.Testing.Enums
{
    [Flags]
    internal enum LoggerMode
    {
        None = 0,

        Console = 1,

        File = 2
    }
}
