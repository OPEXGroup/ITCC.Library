using ITCC.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ITCC.UI.Utils
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

        public static string LogLevelName(LogLevel logLevel)
        {
            return LogLevelDictionary.ContainsKey(logLevel) ? LogLevelDictionary[logLevel] : "НЕТ";
        }

        public static LogLevel LogLevelByName(string name)
        {
            return LogLevelDictionary.Where(item => item.Value == name).Select(item => item.Key).FirstOrDefault();
        }
        #endregion
    }
}
