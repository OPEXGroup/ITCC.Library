// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace ITCC.WPF.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HeaderTooltipAttribute : Attribute
    {
        public HeaderTooltipAttribute(string tooltipContent, bool isLongTooltip = false)
        {
            TooltipContent = tooltipContent;
            IsLongTooltip = isLongTooltip;
        }

        public readonly string TooltipContent;

        public readonly bool IsLongTooltip;
    }
}
