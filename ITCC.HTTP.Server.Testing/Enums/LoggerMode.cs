// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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
