using System;
using System.Runtime.CompilerServices;

namespace ITCC.HTTP.API.Attributes
{
    /// <summary>
    ///     This attribute can be used to mark some instance methods as additional checks.
    /// </summary>
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
