// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using ITCC.HTTP.API.Interfaces;

namespace ITCC.HTTP.API.Attributes
{
    /// <summary>
    ///     Describes api request body format
    ///     Must ONLY be used with properties implementing <see cref="Common.Interfaces.IRequestProcessor"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ApiRequestBodyTypeAttribute : Attribute, ITypeAttribute
    {
        #region construction
        public ApiRequestBodyTypeAttribute(Type type1)
        {
            Type1 = type1;
        }

        public ApiRequestBodyTypeAttribute(Type type1, Type type2)
            : this(type1)
        {
            Type2 = type2;
        }

        public ApiRequestBodyTypeAttribute(Type type1, Type type2, Type type3)
            : this(type1, type2)
        {
            Type3 = type3;
        }

        public ApiRequestBodyTypeAttribute(Type type1, Type type2, Type type3, Type type4)
            : this(type1, type2, type3)
        {
            Type4 = type4;
        }
        #endregion

        #region properties
        public Type Type1 { get; }
        public Type Type2 { get; }
        public Type Type3 { get; }
        public Type Type4 { get; }
        #endregion
    }
}
