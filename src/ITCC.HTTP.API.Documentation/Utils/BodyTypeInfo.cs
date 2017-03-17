// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using ITCC.HTTP.API.Documentation.Enums;

namespace ITCC.HTTP.API.Documentation.Utils
{
    internal class BodyTypeInfo
    {
        public Type Type { get; set; }
        public ObjectCountType CountType { get; set; }
    }
}
