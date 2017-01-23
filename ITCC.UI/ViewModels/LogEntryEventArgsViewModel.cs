// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.Logging.Core;
using ITCC.UI.Attributes;
using ITCC.UI.Utils;

namespace ITCC.UI.ViewModels
{
    public class LogEntryEventArgsViewModel
    {
        public readonly LogEntryEventArgs Subject;

        public LogEntryEventArgsViewModel(LogEntryEventArgs subject)
        {
            Subject = subject;
        }

        [HeaderTooltip(@"Время добавления сообщения")]
        [Header(@"Время")]
        public string Time => Subject.Time.ToString("HH:mm:ss.fff");

        [HeaderTooltip(@"Уровень сообщения")]
        [Header(@"Уровень")]
        public string Level => EnumHelper.LogLevelName(Subject.Level);

        [HeaderTooltip(@"Источник сообщения")]
        [Header(@"Контекст")]
        public string Scope => Subject.Scope.ToString();

        [HeaderTooltip(@"Идентификатор потока, с которого было отправлено сообщение. Служебное поле.", true)]
        [Header(@"ID потока")]
        public int ThreadId => Subject.ThreadId;

        [DatagridColumnStyle(wrappedText: true, columnPreferredWidth: 1)]
        [Header(@"Сообщение")]
        public string Message => Subject.Message;
    }
}