// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Reflection;
using System.Text;

namespace ITCC.HTTP.API.Documentation.Core
{
    internal class ViewDescriptionGenerator
    {
        #region public
        public ViewDescriptionGenerator(StringBuilder builder)
        {
            _builder = builder;
        }

        public void GenerateViewDescription(Type type) => GenerateViewDescriptionInner(type, null);

        #endregion

        #region private

        private void GenerateViewDescriptionInner(Type type, PropertyInfo info)
        {
            
        }

        private readonly StringBuilder _builder;

        #endregion
    }
}
