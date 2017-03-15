// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;

namespace ITCC.HTTP.API.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumValueInfoAttribute : Attribute
    {
        public string DisplayName { get; }
        public string Description { get; }

        public EnumValueInfoAttribute(string displayName, string description = null)
        {
            DisplayName = displayName;
            Description = description;
        }
    }
}
