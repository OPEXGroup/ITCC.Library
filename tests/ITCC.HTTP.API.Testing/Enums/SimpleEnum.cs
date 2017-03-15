// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.HTTP.API.Attributes;

namespace ITCC.HTTP.API.Testing.Enums
{
    public enum SimpleEnum
    {
        [EnumValueInfo(displayName:"1")]
        Zero,
        [EnumValueInfo(displayName: "2")]
        First,
        [EnumValueInfo(displayName: "3")]
        Second,
        Third
    }
}
