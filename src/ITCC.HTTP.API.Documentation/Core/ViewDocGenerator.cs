// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.HTTP.API.Documentation.Utils;

namespace ITCC.HTTP.API.Documentation.Core
{
    /// <summary>
    ///     Class used to generate single view documentation
    /// </summary>
    internal static class ViewDocGenerator
    {
        #region public

        public static bool SetSettings(ViewDocGeneratorSettings settings)
        {
            if (settings == null || !settings.Valid())
                return false;

            _settings = settings;
            return true;
        }

        #endregion

        #region private

        private static ViewDocGeneratorSettings _settings;

        #endregion
    }
}
