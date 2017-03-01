// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ITCC.HTTP.API.Attributes;
using ITCC.Logging.Core;

namespace ITCC.HTTP.API.Utils
{
    public static class EnumInfoProvider
    {
        #region public

        public static string GetElementName(object element)
        {
            var attributes = GetAttributes(element);
            return attributes == null ? null : string.Join(Separator, attributes.Select(attr => attr.DisplayName));
        }
        public static string GetElementDescription(object element)
        {
            var attributes = GetAttributes(element);
            return attributes == null ? null : string.Join(Separator, attributes.Select(attr => attr.Description));
        }
        public static object GetEnumElementByName<TEnum>(string name)
        {
            if (name == null)
                return null;

            var dictionary = GetInfoDictionaty<TEnum>();
            if (dictionary == null)
                return null;

            if (IsFlagEnum<TEnum>())
            {
                var list = new List<TEnum>();
                var stringValues = name.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var stringValue in stringValues)
                {
                    if (dictionary.Values.Any(value => value?.DisplayName == stringValue))
                        list.Add(dictionary.First(keyValuePair => keyValuePair.Value.DisplayName == stringValue).Key);
                    else
                        return null;
                }

                return (TEnum)(object) list.Aggregate(0, (current, elem) => current | (int) (object) elem);
            }

            if (dictionary.Values.All(value => value?.DisplayName != name))
                return null;

            return dictionary.First(keyValuePair => keyValuePair.Value.DisplayName == name).Key;
        }

        public static Dictionary<object, EnumValueInfoAttribute> GetInfoDictionaty(Type enumType)
        {
            if (!enumType.GetTypeInfo().IsEnum)
            {
                Logger.LogEntry("ENUM INFO", LogLevel.Warning, $"Attempt to get EnumValueInfoAttribute value for non-enum object: {enumType.FullName}.");
                return null;
            }

            Dictionary<object, EnumValueInfoAttribute> dictionary;

            if (!Dictionary.ContainsKey(enumType))
            {
                dictionary = Enum.GetValues(enumType)
                    .Cast<object>()
                    .ToDictionary(
                        elem => elem,
                        elem => elem.GetType()
                                .GetRuntimeField(elem.ToString())
                                .GetCustomAttributes<EnumValueInfoAttribute>()
                                .FirstOrDefault());

                Dictionary.TryAdd(enumType, dictionary);
            }
            else
            {
                dictionary = Dictionary[enumType];
            }

            return dictionary;
        }
        public static Dictionary<TEnum, EnumValueInfoAttribute> GetInfoDictionaty<TEnum>() => GetInfoDictionaty(typeof(TEnum))
            ?.ToDictionary(keyValuePair => (TEnum)keyValuePair.Key, keyValuePair => keyValuePair.Value);

        public static IEnumerable<TEnum> SplitEnum<TEnum>(TEnum value)
          => Enum.GetValues(typeof(TEnum))
              .Cast<TEnum>()
              .Where(elem =>
              {
                  var enumValue = value as Enum;
                  var enumElement = elem as Enum;
                    // ReSharper disable once PossibleInvalidCastException
                    return enumElement != null && enumValue != null && (int)(object)enumElement != 0 && enumValue.HasFlag(enumElement);
              });

        #endregion

        #region private

        private const string Separator = ", ";
        private static readonly ConcurrentDictionary<Type, Dictionary<object, EnumValueInfoAttribute>> Dictionary = new ConcurrentDictionary<Type, Dictionary<object, EnumValueInfoAttribute>>();

        private static IList<EnumValueInfoAttribute> GetAttributes(object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            var objType = value.GetType();

            if (!objType.GetTypeInfo().IsEnum)
            {
                Logger.LogEntry("ENUM INFO", LogLevel.Warning, $"Attempt to get EnumValueInfoAttribute value for non-enum object: {objType.FullName}.");
                return null;
            }

            var enumValue = (Enum)value;
            var dictionary = GetInfoDictionaty(objType);

            if (IsFlagEnum(objType))
            {
                var splittedEnum = ((IEnumerable)typeof(EnumInfoProvider).GetTypeInfo()
                                    .GetDeclaredMethod(nameof(SplitEnum))
                                    .MakeGenericMethod(objType)
                                    .Invoke(null, new object[] { enumValue })
                                    ).Cast<Enum>();

                return splittedEnum.Select(elem =>
                {
                    EnumValueInfoAttribute attribute;
                    return !dictionary.TryGetValue(elem, out attribute) ? null : attribute;
                }).ToList();
            }

            EnumValueInfoAttribute enumValueInfoAttribute;
            if (!dictionary.TryGetValue(value, out enumValueInfoAttribute))
            {
                Logger.LogDebug("ENUM INFO", $"Invalid enum value: {value}");
            }

            return enumValueInfoAttribute == null ? null : new List<EnumValueInfoAttribute> { enumValueInfoAttribute };
        }
        private static bool IsFlagEnum(Type enumType) => enumType.GetTypeInfo().GetCustomAttributes<FlagsAttribute>().Any();
        private static bool IsFlagEnum<TEnum>() => typeof(TEnum).GetTypeInfo().GetCustomAttributes<FlagsAttribute>().Any();
        #endregion
    }
}
