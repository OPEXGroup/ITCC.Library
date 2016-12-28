// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using System.Linq;
using ITCC.Logging.Core;

namespace ITCC.WPF.Utils
{
    internal static class EnumHelper
    {
        #region LogLevel

        private static readonly Dictionary<LogLevel, string> LogLevelDictionary = new Dictionary<LogLevel, string>
        {
            {LogLevel.None, "Отключен"},
            {LogLevel.Critical, "Критический" },
            {LogLevel.Error, "Ошибка" },
            {LogLevel.Warning, "Предупреждение" },
            {LogLevel.Info, "Информация" },
            {LogLevel.Debug, "Отладка" },
            {LogLevel.Trace, "Трассировка" }
        };

        public static string LogLevelName(LogLevel logLevel) => LogLevelDictionary.ContainsKey(logLevel) ? LogLevelDictionary[logLevel] : "НЕТ";

        public static LogLevel LogLevelByName(string name) => LogLevelDictionary.Where(item => item.Value == name).Select(item => item.Key).FirstOrDefault();

        #endregion
    }
}
