using System;

namespace ITCC.HTTP.Server.Enums
{
    [Flags]
    internal enum AcceptTypeMatch
    {
        NotMatch,

        TypeStar = 1,

        SubtypeStar = 2,

        TypeMatch = 8,

        SubtypeMatch = 16
    }
}
