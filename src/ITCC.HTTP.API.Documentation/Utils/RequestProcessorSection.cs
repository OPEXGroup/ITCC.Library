// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using System.Reflection;

namespace ITCC.HTTP.API.Documentation.Utils
{
    /// <summary>
    ///     Used for separate request processor sections in API docs
    /// </summary>
    public class RequestProcessorSection
    {
        /// <summary>
        ///     Section title
        /// </summary>
        public string TitlePattern { get; set; }
        /// <summary>
        ///     Section description (optional)
        /// </summary>
        public string DescriptionPattern { get; set; }
        /// <summary>
        ///     Section methods (ordered)
        /// </summary>
        public List<PropertyInfo> RequestProcessorInfos { get; set; }
    }
}
