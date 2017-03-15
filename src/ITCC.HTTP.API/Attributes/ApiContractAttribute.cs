// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.API.Attributes
{
    /// <summary>
    ///     Represents api contract.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ApiContractAttribute : Attribute
    {
        /// <summary>
        ///     Creates new instance of <see cref="ApiContractAttribute"/>
        /// </summary>
        /// <param name="type">Used contracts</param>
        /// <param name="comment">Simple comment. Do not use if <see cref="Type"/> is explicit enough</param>
        public ApiContractAttribute(ApiContractType type, string comment = null)
        {
            Type = type;
            Comment = comment;
        }

        public ApiContractType Type { get; }
        public string Comment { get; }
    }
}
