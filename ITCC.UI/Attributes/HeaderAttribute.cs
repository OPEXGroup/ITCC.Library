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
