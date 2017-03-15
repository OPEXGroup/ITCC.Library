// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace ITCC.HTTP.Server.Enums
{
    [Flags]
    internal enum AcceptTypeMatch
    {
        NotMatch,

        TypeStar = 2,

        SubtypeStar = 4,

        TypeMatch = 8,

        SubtypeMatch = 16
    }
}
