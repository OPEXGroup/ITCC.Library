// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Reflection;
using System.Text;
using ITCC.HTTP.API.Documentation.Utils;

namespace ITCC.HTTP.API.Documentation.Core
{
    internal class ViewDescriptionGenerator
    {
        #region public
        public ViewDescriptionGenerator(StringBuilder builder)
        {
            _builder = builder;
        }

        public bool SetSettings(ViewDescriptionGeneratorSettings settings)
        {
            if (settings == null || !settings.Valid())
                return false;

            _settings = settings;
            return true;
        }

        public void GenerateViewDescription(Type type) => GenerateViewDescriptionInner(type, null);

        #endregion

        #region private

        private void GenerateViewDescriptionInner(Type type, PropertyInfo info)
        {
            
        }

        private string GetSimpleTypeDescription(Type type, PropertyInfo info)
        {
            return string.Empty;
        }

        private void IsApiViewOrApiViewList(Type type)
        {
            
        }

        private readonly StringBuilder _builder;
        private ViewDescriptionGeneratorSettings _settings;

        #endregion
    }
}
