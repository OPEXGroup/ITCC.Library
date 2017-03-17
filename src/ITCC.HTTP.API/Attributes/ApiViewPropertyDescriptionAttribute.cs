// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;

namespace ITCC.HTTP.API.Attributes
{
    /// <summary>
    ///     Property used to describe API view property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ApiViewPropertyDescriptionAttribute : Attribute
    {
        public ApiViewPropertyDescriptionAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }
}
