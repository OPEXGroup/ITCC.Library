// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using ITCC.HTTP.API.Documentation.Enums;
using ITCC.HTTP.API.Interfaces;

namespace ITCC.HTTP.API.Documentation.Utils
{
    internal static class TypeAttributeHelper
    {
        public static List<BodyTypeInfo> GetTypes(this ITypeAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            var result = new List<BodyTypeInfo>();

            var types = new List<Type>
            {
                attribute.Type1,
                attribute.Type2,
                attribute.Type3,
                attribute.Type4
            }.Where(t => t != null).ToList();

            var typeCount = types.Count;
            var currentBodyType = 0;
            result.Add(new BodyTypeInfo
            {
                Type = types[0],
                CountType = types[0].IsGenericList() ? ObjectCountType.List : ObjectCountType.Single
            });
            for (var i = 1; i < typeCount; ++i)
            {
                var currentType = types[i];
                var previousType = types[i - 1];

                if (IsSingleOrListPair(currentType, previousType))
                {
                    result[currentBodyType].CountType = ObjectCountType.SingleOrList;
                    continue;
                }

                result.Add(new BodyTypeInfo
                {
                    Type = types[i],
                    CountType = types[i].IsGenericList() ? ObjectCountType.List : ObjectCountType.Single
                });
                currentBodyType++;
            }

            return result;
        }

        private static bool IsSingleOrListPair(Type firstType, Type secondType) => IsAsymmetricSingleOrListPair(firstType, secondType)
                                                                                   || IsAsymmetricSingleOrListPair(secondType, firstType);

        private static bool IsAsymmetricSingleOrListPair(Type singleType, Type listType)
        {
            if (!listType.IsGenericList())
                return false;

            return listType.GenericTypeArguments[0] == singleType;
        }

        private static bool IsGenericList(this Type type) => type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>));
    }
}
