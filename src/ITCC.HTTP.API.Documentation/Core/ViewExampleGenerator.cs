// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Reflection;

namespace ITCC.HTTP.API.Documentation.Core
{
    internal static class ViewExampleGenerator
    {
        #region public

        public static object GenerateViewExample(PropertyInfo info)
        {
            var type = info.PropertyType;
            return Activator.CreateInstance(type);
        }

        #endregion

        #region private

        

        #endregion
    }
}
