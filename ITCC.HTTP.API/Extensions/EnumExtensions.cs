using System;
using System.Linq;
using System.Reflection;

namespace ITCC.HTTP.API.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        ///     Check if <b>ENUM</b> value is in enum range (including Flags case)
        /// </summary>
        /// <typeparam name="TEnum">Enum type. Method will always return true if it is not enum type</typeparam>
        /// <param name="value">Value to check</param>
        /// <returns>False if value has enum type and is not in enum values range, otherwise, true</returns>
        public static bool IsInEnumRange<TEnum>(this TEnum value)
            where TEnum : struct
        {
            var type = value.GetType();
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsEnum)
                return true;

            try
            {
                if (!typeInfo.GetCustomAttributes<FlagsAttribute>().Any())
                    return Enum.IsDefined(typeof(TEnum), value);

                var values = Enum.GetValues(type);
                foreach (var subset in values.GetSubsets())
                {
                    var subsetValueSum = subset.Select(v => (int)v).Sum();
                    if (Convert.ToInt32(value) == subsetValueSum)
                        return true;
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        /// <summary>
        ///     Check if <b>ENUM</b> value is <b>NOT</b> in enum range (including Flags case)
        /// </summary>
        /// <typeparam name="TEnum">Enum type. Method will always return false if it is not enum type</typeparam>
        /// <param name="value">Value to check</param>
        /// <returns>True if value has enum type and is not in enum values range, otherwise, false</returns>
        public static bool IsNotInEnumRange<TEnum>(this TEnum value)
            where TEnum : struct => !value.IsInEnumRange();
    }
}
