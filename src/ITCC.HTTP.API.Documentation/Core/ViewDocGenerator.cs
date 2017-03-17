// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Linq;
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
            _descriptionGenerator = new ViewDescriptionGenerator(_builder);
        }

        public bool SetSettings(ViewDocGeneratorSettings settings)
        {
            if (settings == null || !settings.Valid())
                return false;

            _settings = settings;
            _hasSerializers = _settings.Serializers != null && _settings.Serializers.Any();
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
            _builder.AppendLine();
            _builder.AppendLine(_settings.ExamplesHeaderPattern);

            if (!_hasSerializers)
            {
                _builder.AppendLine(_settings.NoExampleAvailablePattern);
                return;
            }

            var exampleObject = ViewExampleGenerator.GenerateViewExample(type);
            foreach (var serializer in _settings.Serializers)
            {
                _builder.AppendLine();
                _builder.AppendLine(serializer.ExampleHeader);
                _builder.AppendLine();
                _builder.AppendLine(_settings.ExampleStartPattern);
                _builder.AppendLine(serializer.Serialize(exampleObject));
                _builder.AppendLine(_settings.ExampleEndPattern);
            }
        }

        private void WriteBodyDescriptionAndRestrictions(Type type)
        {
            Wrappers.AppendPaddedLines(_builder, _settings.DescriptionAndRestrictionsPattern);

            _descriptionGenerator.GenerateViewDescription(type);
        }

        private bool _hasSerializers;
        private ViewDocGeneratorSettings _settings;
        private readonly StringBuilder _builder;

        private ViewDescriptionGenerator _descriptionGenerator;

        #endregion
    }
}
