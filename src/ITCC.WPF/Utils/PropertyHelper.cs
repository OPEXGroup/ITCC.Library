// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.ComponentModel;
using System.Reflection;
using ITCC.UI.Attributes;

namespace ITCC.WPF.Utils
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

                var header = propertyDescriptor.Attributes[typeof(HeaderAttribute)] as HeaderAttribute;
                if (header != null)
                {
                    return header.DisplayName;
                }
            }
            else
            {
                var propertyInfo = descriptor as PropertyInfo;
                if (propertyInfo == null)
                    return null;

                var attributes = propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                foreach (var attr in attributes)
                {
                    var displayName = attr as DisplayNameAttribute;
                    if (displayName == null || ReferenceEquals(displayName, DisplayNameAttribute.Default))
                        continue;
                    return displayName.DisplayName;
                }

                var headerAttributes = propertyInfo.GetCustomAttributes(typeof(HeaderAttribute), true);
                foreach (var attr in attributes)
                {
                    var headerAttribute = attr as HeaderAttribute;
                    if (headerAttribute == null)
                        continue;
                    return headerAttribute.DisplayName;
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