// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;

namespace ITCC.HTTP.API.Attributes
{
    /// <summary>
    ///     Describes API method HTTP query param
    ///     Must ONLY be used with properties implementing <see cref="Common.Interfaces.IRequestProcessor"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ApiQueryParamAttribute : Attribute
    {
        public ApiQueryParamAttribute(string name, string description, bool optional = true)
        {
            Name = name;
            Description = description;
            Optional = optional;
        }

        public string Name { get; }
        public string Description { get; }
        public bool Optional { get; }
    }
}
