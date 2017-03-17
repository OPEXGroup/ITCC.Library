// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Reflection;

namespace ITCC.HTTP.API.Documentation.Core
{
    internal static class ViewExampleGenerator
    {
        #region public

        public static object GenerateViewExample(Type type) => GenerateViewExampleInner(type, null);

        #endregion

        #region private

        private static object GenerateViewExampleInner(Type type, PropertyInfo info)
        {
            return Activator.CreateInstance(type);
        }

        #endregion
    }
}
