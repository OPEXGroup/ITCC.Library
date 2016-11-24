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
