using System;
using System.ComponentModel;
using System.Reflection;

namespace ITCC.UI.Utils
{
    internal static class PropertyHelper
    {
        public static string GetPropertyDisplayName(object descriptor)
        {
            var propertyDescriptor = descriptor as PropertyDescriptor;

            if (propertyDescriptor != null)
            {
                var displayName = propertyDescriptor.Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
                if (displayName != null && !ReferenceEquals(displayName, DisplayNameAttribute.Default))
                {
                    return displayName.DisplayName;
                }
            }
            else
            {
                var propertyInfo = descriptor as PropertyInfo;
                if (propertyInfo == null) return null;

                var attributes = propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                foreach (var attr in attributes)
                {
                    var displayName = attr as DisplayNameAttribute;
                    if (displayName == null || ReferenceEquals(displayName, DisplayNameAttribute.Default))
                        continue;
                    var s = displayName.DisplayName;
                    return s;
                }
                return null;
            }

            return null;
        }

        public static TAttribute GetPropertyAttribute<TAttribute>(object descriptor)
            where TAttribute : Attribute
        {
            var propertyDescriptor = descriptor as PropertyDescriptor;

            if (propertyDescriptor != null)
            {
                var headerTooltipAttribute = propertyDescriptor.Attributes[typeof(TAttribute)] as TAttribute;
                if (headerTooltipAttribute != null)
                {
                    return headerTooltipAttribute;
                }
            }
            else
            {
                var propertyInfo = descriptor as PropertyInfo;
                if (propertyInfo == null) return null;

                var attributes = propertyInfo.GetCustomAttributes(typeof(TAttribute), true);
                foreach (var attr in attributes)
                {
                    var headerTooltipAttribute = attr as TAttribute;
                    if (headerTooltipAttribute != null)
                    {
                        return headerTooltipAttribute;
                    }
                }
                return null;
            }

            return null;
        }
    }
}