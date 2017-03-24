// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;

namespace ITCC.HTTP.API.Documentation.Utils
{
    internal class ViewDescriptionGeneratorSettings
    {
        #region properties
        public Func<Type, string> TypeNameFunc { get; set; }
        public string AdditionalChecksHeaderPattern { get; set; }
        #endregion

        #region methods

        public bool Valid()
        {
            if (TypeNameFunc == null)
                return false;

            return true;
        }

        #endregion
    }
}
