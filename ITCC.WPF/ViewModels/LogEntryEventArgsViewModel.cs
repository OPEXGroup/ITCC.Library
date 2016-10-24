using System.ComponentModel;
using System.Windows.Controls;
using ITCC.Logging.Core;
using ITCC.WPF.Attributes;
using ITCC.WPF.Utils;

namespace ITCC.WPF.ViewModels
{
    public class LogEntryEventArgsViewModel
    {
        public readonly LogEntryEventArgs Subject;

        public LogEntryEventArgsViewModel(LogEntryEventArgs subject)
        {
            Subject = subject;
        }

        
        [HeaderTooltip(@"Время добавления сообщения")]
        [DisplayName(@"Время")]
        public string Time => Subject.Time.ToString("HH:mm:ss.fff");

        [HeaderTooltip(@"Уровень сообщения")]
        [DisplayName(@"Уровень")]
        public string Level => EnumHelper.LogLevelName(Subject.Level);

        [HeaderTooltip(@"Источник сообщения")]
        [DisplayName(@"Контекст")]
        public string Scope => Subject.Scope.ToString();

        [HeaderTooltip(@"Идентификатор потока, с которого было отправлено сообщение. Служебное поле.", true)]
        [DisplayName(@"ID потока")]
        public int ThreadId => Subject.ThreadId;

        [DatagridColumnStyle(wrappedText: true, columnPreferredWidth: 1, columnWidthUnitType: DataGridLengthUnitType.Star)]
        [DisplayName(@"Сообщение")]
        public string Message => Subject.Message;
    }
}