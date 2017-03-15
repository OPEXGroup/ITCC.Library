// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.Logging.Core;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ITCC.HTTP.API.Documentation
{
    public class DocGenerator
    {
        #region public

        public DocGenerator(StringBuilder builder)
        {
            _builder = new StringBuilder();
        }

        public async Task<bool> GenerateApiDocumentationAsync(Type markerType, Stream outputStream)
        {
            _outputStream = outputStream;

            if (!TryLoadTargetAssembly(markerType))
                return false;

            return await TryWriteResultAsync();
        }

        #endregion

        #region private

        private bool TryLoadTargetAssembly(Type markerType)
        {
            try
            {
                _targetAssembly = Assembly.GetAssembly(markerType);
                LogDebug($"Assembly {_targetAssembly.FullName} loaded");
                return true;
            }
            catch (Exception exception)
            {
                LogExceptionDebug(exception);
                return false;
            }
        }

        private async Task<bool> TryWriteResultAsync()
        {
            try
            {
                var result = _builder.ToString();
                _builder.Clear();
                using (var writer = new StreamWriter(_outputStream))
                {
                    await writer.WriteAsync(result);
                    await writer.FlushAsync();
                }

                return true;
            }
            catch (Exception exception)
            {
                LogExceptionDebug(exception);
                return false;
            }
        }

        private void LogDebug(string message) => Logger.LogDebug(LogScope, message);
        private void LogExceptionDebug(Exception exception) => Logger.LogExceptionDebug(LogScope, exception);

        private Stream _outputStream;
        private readonly StringBuilder _builder;
        private Assembly _targetAssembly;

        private const string LogScope = "DOC GENER";

        #endregion
    }
}
