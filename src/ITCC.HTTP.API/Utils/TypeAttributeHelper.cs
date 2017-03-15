// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using ITCC.HTTP.API.Interfaces;

namespace ITCC.HTTP.API.Utils
{
    internal static class TypeAttributeHelper
    {
        public static List<Type> GetTypes(this ITypeAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            var result = new List<Type>();

            if (attribute.Type1 != null)
                result.Add(attribute.Type1);

            if (attribute.Type2 != null)
                result.Add(attribute.Type2);

            if (attribute.Type3 != null)
                result.Add(attribute.Type3);

            if (attribute.Type4 != null)
                result.Add(attribute.Type4);

            return result;
        }
    }
}
