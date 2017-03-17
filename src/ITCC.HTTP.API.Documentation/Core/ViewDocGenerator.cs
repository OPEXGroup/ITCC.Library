// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Text;
using ITCC.HTTP.API.Documentation.Utils;

namespace ITCC.HTTP.API.Documentation.Core
{
    /// <summary>
    ///     Class used to generate single view documentation
    /// </summary>
    internal class ViewDocGenerator
    {
        #region public

        public ViewDocGenerator(StringBuilder builder)
        {
            _builder = builder;
        }

        public bool SetSettings(ViewDocGeneratorSettings settings)
        {
            if (settings == null || !settings.Valid())
                return false;

            _settings = settings;
            return true;
        }

        public void WriteBodyDescription(Type type)
        {
            WriteBodyExamples(type);
            WriteBodyDescriptionAndRestrictions(type);
        }

        #endregion

        #region private

        private void WriteBodyExamples(Type type)
        {
            
        }

        private void WriteBodyDescriptionAndRestrictions(Type type)
        {
            
        }

        private ViewDocGeneratorSettings _settings;
        private StringBuilder _builder;

        #endregion
    }
}
