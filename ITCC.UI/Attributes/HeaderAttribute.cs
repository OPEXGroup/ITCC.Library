// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;

namespace ITCC.UI.Attributes
{
    /// <summary>
    ///     Used if System.ComponentModel.DisplayNameAttribute is not available
    /// </summary>
    public class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; }
    }
}
