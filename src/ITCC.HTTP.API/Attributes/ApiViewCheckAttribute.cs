// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace ITCC.HTTP.API.Attributes
{
    /// <summary>
    ///     This attribute can be used to mark some instance methods as additional checks.
    /// </summary>
    /// <remarks>
    ///     Method marked with this attribute must:
    ///     * Be public instance method
    ///     * Have Func &lt;bool&gt; signature
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiViewCheckAttribute : Attribute
    {
        public ApiViewCheckAttribute(string checkDescription = null)
        {
            CheckDescription = checkDescription;
        }

        public string CheckDescription { get; }
    }
}
