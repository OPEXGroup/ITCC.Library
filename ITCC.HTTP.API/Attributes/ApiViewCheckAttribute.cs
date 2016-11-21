using System;
using System.Runtime.CompilerServices;

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
        public ApiViewCheckAttribute([CallerMemberName] string errorDescription = null)
        {
            ErrorDescription = errorDescription;
        }

        public string ErrorDescription { get; }
    }
}
