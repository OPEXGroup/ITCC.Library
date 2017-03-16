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
        public string TitlePattern { get; set; }
        public string DescriptionPattern { get; set; }
        public List<PropertyInfo> RequestProcessorInfos { get; set; }
    }
}
