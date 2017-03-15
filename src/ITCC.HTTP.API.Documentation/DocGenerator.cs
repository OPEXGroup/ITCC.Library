// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.Logging.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ITCC.HTTP.API.Documentation
{
    public class DocGenerator
    {
        #region public

        public DocGenerator()
        {
            _builder = new StringBuilder();
        }

        public async Task<bool> GenerateApiDocumentationAsync(Type markerType, Stream outputStream)
        {
            _outputStream = outputStream;

            if (!TryLoadTargetAssembly(markerType))
                return false;

            if (!AssemblyIsValid())
            {
                LogDebug("Assembly configuration is invalid, generation cancelled");
                return false;
            }

            LogDebug("Writing header");
            if (!await TryWriteHeaderAsync())
            {
                LogDebug("Failed to write header!");
                return false;
            }
            LogDebug("Header written");

            LogDebug("Writing footer");
            if (!await TryWriteFooterAsync())
            {
                LogDebug("Failed to write footer!");
                return false;
            }
            LogDebug("Footer written");

            return await TryWriteResultAsync();
        }

        protected virtual Task<bool> TryWriteHeaderAsync()
        {
            _builder.AppendLine("HEADER");
            return Task.FromResult(true);
        }

        protected virtual Task<bool> TryWriteFooterAsync()
        {
            _builder.AppendLine("FOOTER");
            return Task.FromResult(true);
        }

        #endregion

        #region private

        private bool TryLoadTargetAssembly(Type markerType) => DoSafe(() =>
        {
            _targetAssembly = Assembly.GetAssembly(markerType);
            LogDebug($"Assembly {_targetAssembly.FullName} loaded");
            return true;
        });

        private bool AssemblyIsValid() => DoSafe(() =>
        {
            var properties = GetAllProperties();

            return true;
        });

        private Task<bool> TryWriteResultAsync() => DoSafeAsync(async () =>
        {
            var result = _builder.ToString();
            _builder.Clear();
            using (var writer = new StreamWriter(_outputStream))
            {
                await writer.WriteAsync(result);
                await writer.FlushAsync();
            }

            return true;
        });

        private bool DoSafe(Func<bool> method)
        {
            try
            {
                return method.Invoke();
            }
            catch (Exception exception)
            {
                LogExceptionDebug(exception);
                return false;
            }
        }

        private async Task<bool> DoSafeAsync(Func<Task<bool>> asyncMethod)
        {
            try
            {
                return await asyncMethod.Invoke();
            }
            catch (Exception exception)
            {
                LogExceptionDebug(exception);
                return false;
            }
        }

        private List<PropertyInfo> GetAllProperties() => _targetAssembly.GetTypes().SelectMany(t => t.GetProperties()).ToList();

        private static void LogDebug(string message) => Logger.LogDebug(LogScope, message);
        private static void LogExceptionDebug(Exception exception) => Logger.LogExceptionDebug(LogScope, exception);

        private Stream _outputStream;
        private readonly StringBuilder _builder;
        private Assembly _targetAssembly;

        private const string LogScope = "DOC GENER";

        #endregion
    }
}
