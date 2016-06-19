﻿using System.ComponentModel;
using System.Reflection;
using ITCC.UI.Attributes;

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

        public static HeaderTooltipAttribute GetPropertyHeaderTooltip(object descriptor)
        {
            var propertyDescriptor = descriptor as PropertyDescriptor;

            if (propertyDescriptor != null)
            {
                var headerTooltipAttribute = propertyDescriptor.Attributes[typeof(HeaderTooltipAttribute)] as HeaderTooltipAttribute;
                if (headerTooltipAttribute != null)
                {
                    return headerTooltipAttribute;
                }
            }
            else
            {
                var propertyInfo = descriptor as PropertyInfo;
                if (propertyInfo == null) return null;

                var attributes = propertyInfo.GetCustomAttributes(typeof(HeaderTooltipAttribute), true);
                foreach (var attr in attributes)
                {
                    var headerTooltipAttribute = attr as HeaderTooltipAttribute;
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