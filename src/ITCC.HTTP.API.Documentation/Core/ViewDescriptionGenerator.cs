// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ITCC.HTTP.API.Attributes;
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

        public void GenerateViewDescription(Type type) => WriteViewDescriptionInner(type, null, 0);
        #endregion

        #region private

        private void WriteViewDescriptionInner(Type type, PropertyInfo info, int propertyLevel)
        {
            if (!IsApiViewOrApiViewList(type))
            {
                WriteSimpleTypeDescription(type, info, propertyLevel);
                return;
            }

            WritePropertiesDescription(type, info, propertyLevel);
            WriteAdditionalChecksDescription(type, propertyLevel);
        }

        private void WritePropertiesDescription(Type type, PropertyInfo info, int propertyLevel)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in properties)
            {
                var propertyType = propertyInfo.PropertyType;
                WriteViewDescriptionInner(propertyType, propertyInfo, propertyLevel + 1);
            }
        }

        private void WriteSimpleTypeDescription(Type type, PropertyInfo info, int propertyLevel)
        {
            var propertyName = info.Name;
            var typeName = _settings.TypeNameFunc(type);

            WriteLine($"* {propertyName} - {typeName}", propertyLevel);
        }

        private bool IsApiViewOrApiViewList(Type type)
        {
            var targetType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
                ? type.GenericTypeArguments[0]
                : type;

            return targetType.GetCustomAttributes<ApiViewAttribute>().Any();
        }

        private void WriteAdditionalChecksDescription(Type type, int propertyLevel)
        {
            var additionalChecks = type
                .GetCustomAttributes<ApiViewCheckAttribute>()
                .Where(ac => ac.CheckDescription != null)
                .ToList();
            if (!additionalChecks.Any())
                return;

            WriteLine(_settings.AdditionalChecksHeaderPattern, propertyLevel);
            WriteLine(@"", propertyLevel);
            foreach (var additionalCheck in additionalChecks)
            {
                WriteLine($"* {additionalCheck.CheckDescription}");
            }
        }

        private void WriteLine(string line = @"", int propertyLevel = 0)
        {
            var prefix = string.Concat(Enumerable.Repeat(PrefixUnit, propertyLevel));
            _builder.AppendLine($"{prefix}{line}");
        }

        private readonly StringBuilder _builder;
        private ViewDescriptionGeneratorSettings _settings;

        private const string PrefixUnit = "\t";

        #endregion
    }
}
