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

            var unwrapped = UnwrapListType(type);

            WritePropertySelfDescription(type, info, propertyLevel);
            WritePropertiesDescription(unwrapped, info, propertyLevel);
            WriteAdditionalChecksDescription(unwrapped, propertyLevel);
        }

        private string GetPropertyDescription(MemberInfo info) => info
            .GetCustomAttributes<ApiViewPropertyDescriptionAttribute>()
            .FirstOrDefault()?
            .Description;

        private void WritePropertySelfDescription(Type type, PropertyInfo info, int propertyLevel)
        {
            if (info == null)
                return;

            var fullDescription = $"* {info.Name} - {_settings.TypeNameFunc(type)}. ";
            var propertyDescription = GetPropertyDescription(info);
            if (!string.IsNullOrWhiteSpace(propertyDescription))
                fullDescription += propertyDescription;
            WriteLine(fullDescription, propertyLevel);
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

        private Type UnwrapListType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
                ? type.GenericTypeArguments[0]
                : type;

        private bool IsApiViewOrApiViewList(Type type)
        {
            var targetType = UnwrapListType(type);

            return targetType.GetCustomAttributes<ApiViewAttribute>().Any();
        }

        private void WriteAdditionalChecksDescription(IReflect type, int propertyLevel)
        {
            var additionalChecks = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(mi => mi.ReturnType == typeof(bool) && mi.GetParameters().Length == 0)
                .SelectMany(c => c.GetCustomAttributes<ApiViewCheckAttribute>())
                .Where(ac => ac.CheckDescription != null)
                .ToList();

            if (!additionalChecks.Any())
                return;

            WritePaddedLine(_settings.AdditionalChecksHeaderPattern, propertyLevel);
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

        private void WritePaddedLine(string line = @"", int propertyLevel = 0)
        {
            _builder.AppendLine();
            WriteLine(line, propertyLevel);
            _builder.AppendLine();
        }

        private readonly StringBuilder _builder;
        private ViewDescriptionGeneratorSettings _settings;

        private const string PrefixUnit = "\t";

        #endregion
    }
}
