// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using ITCC.HTTP.API.Interfaces;
using ITCC.Logging.Core;

namespace ITCC.HTTP.API.Documentation.Utils
{
    internal class ViewDocGeneratorSettings
    {
        #region properties
        /// <summary>
        ///     Example serializers
        /// </summary>
        public IEnumerable<IExampleSerializer> Serializers { get; set; } = new List<IExampleSerializer>();
        public string NoPropertyContractPattern { get; set; }
        public string NoPropertyDescriptionPattern { get; set; }
        public string NoExampleAvailablePattern { get; set; }
        public string ExampleStartPattern { get; set; }
        public string ExampleEndPattern { get; set; }
        public string ExamplesHeaderPattern { get; set; }
        public string DescriptionAndRestrictionsPattern { get; set; }
        public Func<Type, string> TypeNameFunc { get; set; }

        #endregion

        #region methods

        public bool Valid()
        {
            if ((Serializers == null || !Serializers.Any()) && string.IsNullOrWhiteSpace(NoExampleAvailablePattern))
            {
                LogDebug("No serialization info provided");
                return false;
            }

            return true;
        }

        private static void LogDebug(string message) => Logger.LogDebug("DOC SETTINGS", message);

        #endregion
    }
}
