// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Reflection;

namespace ITCC.HTTP.API.Extensions
{
    /// <summary>
    ///     Extension methods for property type checks
    /// </summary>
    public static class PropertyInfoExtensions
    {
        /// <summary>
        ///     Check if property type extends, implements or matches given interface type
        /// </summary>
        /// <typeparam name="TInterface">Interface type</typeparam>
        /// <param name="propertyInfo">Property info</param>
        /// <returns></returns>
        public static bool ExtendsOrImplements<TInterface>(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            return typeof(TInterface).GetTypeInfo().IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo());
        }
    }
}
