## ITCC.UI

Библиотека классов для использования в GUI-приложениях на WPF, UWP и Xamarin Forms.

### Attributes

Классы используемых атрибутов

#### `class DataGridColumnStyleAttribute`

Используется `DataGridHelper` для задания размера и стиля колонки. Используется при создании `ViewModel`'ов. Конструктор:

```
DatagridColumnStyleAttribute(bool wrappedText = false, double columnPreferredWidth = -1, DataGridLengthUnitType columnWidthUnitType = DataGridLengthUnitType.Auto);
```

#### `class DataGridIgnoreAttribute`

Используется `DataGridHelper` для опционального игнорирования столбца:

```
DataGridIgnoreAttribute(bool ignoreColumn = true)
```

#### `class HeaderAttribute`

Используется `DataGridHelper` для генерации заголовков в `Datagrid`. Используется при создании `ViewModel`'ов. Конструктор:

```
HeaderAttribute(string displayName);
```

#### `class HeaderTooltipAttribute`

Используется `DataGridHelper` для генерации пояснений к заголовкам в `Datagrid`. Используется при создании `ViewModel`'ов. Конструктор:

```
HeaderTooltipAttribute(string tooltipContent, bool isLongTooltip = false)
```

### Commands

#### `class AsyncCommand<TResult> : IAsyncCommand, INotifyPropertyChanged`

Класс представляет асинхронную команду, вызываемую через GUI. Поддерживает асинхронную отмену, отслеживание возможности исполнения и отслеживание текущего статуса исполнения. Пример использования через привязки приведен в документации в исходном коде.
Конструктор:

```
AsyncCommand(Func<object, CancellationToken, Task<TResult>> command, Func<bool> canExecuteCondition = null);
```

Все публичные свойства уведомляют о своем изменении. Свойства (не считая реализаций интерфейсов):

```
bool Enabled { get; }                               // Может ли команда быть исполнена
NotifyTaskCompletion<TResult> Execution { get; }    // Объект для отслеживания текущего статуса
ICommand CancelCommand { get; }                     // Команда, вызывающая отмену
```

#### `class AsyncCommand : AsyncCommand<object>`

То же, что и `AsyncCommand<TResult>` за исключением того, что получение результата не поддерживается

#### `static class AsyncCommandFactory`

Класс для создания асинронных команд (популярные сценарии). Методы:

```
AsyncCommand Create(Func<Task> command, Func<bool> canExecuteCondition);
AsyncCommand Create(Func<object, Task> command, Func<bool> canExecuteCondition);
AsyncCommand<TResult> Create<TResult>(Func<Task<TResult>> command, Func<bool> canExecuteCondition);
AsyncCommand<TResult> Create<TResult>(Func<object, Task<TResult>> command, Func<bool> canExecuteCondition);
AsyncCommand Create(Func<CancellationToken, Task> command, Func<bool> canExecuteCondition);
AsyncCommand Create(Func<object, CancellationToken, Task> command, Func<bool> canExecuteCondition);
AsyncCommand<TResult> Create<TResult>(Func<CancellationToken, Task<TResult>> command, Func<bool> canExecuteCondition);
AsyncCommand<TResult> Create<TResult>(Func<object, CancellationToken, Task<TResult>> command, Func<bool> canExecuteCondition);
```

#### `class DelegateCommand : ICommand, INotifyPropertyChanged`

Класс представляет синхронную команду, вызываемую через GUI. Конструкторы:

```
public DelegateCommand(Action command, Func<bool> executionCondition = null);
public DelegateCommand(Action<object> command, Func<bool> executionCondition = null);
```

Все публичные свойства уведомляют о своем изменении. Свойства (не считая реализаций интерфейсов):

```
bool Enabled { get; }                               // Может ли команда быть исполнена
```

#### `sealed class NotifyTaskCompletion<TResult> : INotifyPropertyChanged`

Класс для отслеживания состояния выполнения асихронной команды. Конструируется только внутри команд. 
Все публичные свойства уведомляют о своем изменении. Свойства (не считая реализаций интерфейсов):

```
Task<TResult> Task { get; }             // Отслеживаемая задача
Task TaskCompletion { get; }            // Задача, завершающаяся вместе с отслеживаемой
TResult Result { get; }                 // Результат выполнения задачи
TaskStatus Status { get; }              // Текущий статус отслеживаемой задачи
bool IsCompleted { get; }               // Завершена ли задача
bool IsNotCompleted { get; }            // ! IsCompleted
bool IsSuccessfullyCompleted { get; }   // Задача завершилась успехом
bool IsCanceled { get; }                // Задача была отменена
bool IsFaulted { get; }                 // При выполнении задачи произошла ошибка
AggregateException Exception { get; }   // Исключение, возникшее 
AggregateException Exception { get; }   // Исключение, возникшее в процессе выполнения задачи (обертка)
Exception InnerException { get; }       // Реальное исключение, возникшее в процессе выполнения задачи
string ErrorMessage { get; }            // Сообщение о возникшей ошибке
```

### Common

Общие классы

#### `static class Delegates`

Тут просто хранятся объявления делегатов библитеки. Публичные:

```
/*
  Используется в ObservableLogger
*/
delegate Task AsyncUiThreadRunner(Action action);
```

### Interfaces

Используемые интерфейсы

#### `interface IAsyncCommand : ICommand`

Интерфейс асинхронной команды, отдаваемой из GUI. Сигнатура:

```
Task ExecuteAsync(object parameter);
```

### Loggers

Уже написанные варианты получателя лога

#### `class ObservableLogger : ILogReceiver`

Предоставляет `ObservableCollection` для отображения в интерфейсе. Используется с `LogWindow`.  Основной конструктор
```
ObservableLogger(LogLevel level, int capacity, AsyncUiThreadRunner asyncUiThreadRunner)
```

Пример `AsyncUiThreadRunner` для WPF:

```
async Task RunOnUiThreadAsync(Action action) => await Application.Current.Dispatcher.InvokeAsync(action);
```

### Utils

Небольшие служебные классы

#### `class ConditionValidator`

Класс для проверки наборов условий. **ВАЖНО**: прри добавлении метода его результат вычисляется сразу же.

Ключевые методы:

```
void NonNull(object something, string message = null);
void NonWhitespaceString(string text, string message = null);
void DoesNotThrowException(Action action, string message = null);
void AddSafeCondition(Func<bool> conditionMethod, string message = null);
bool AddCondition(bool condition, string message = null);
bool AddCondition(Func<bool> conditionMethod, string message = null);
async Task<bool> AddConditionAsync(Task<bool> conditionTask, string message = null);
async Task<bool> AddConditionAsync(Func<Task<bool>> conditionMethod, string message = null);
```

Ключевые свойства:

```
ValidationResult ValidationResult { get; }
bool ValidationPassed { get; }
string ErrorMessage { get; } // null тогда и только тогда, когда ValidationPassed == true
```

#### `static class EnumHelper`

Работа с `LogLevel`. Методы:

```
string LogLevelName(LogLevel logLevel;
LogLevel LogLevelByName(string name;
```

#### `class ObservableRingBuffer<TItem> : ObservableCollection<TItem>`

Представляет наблюдаемую коллекцию с ограниченным размером. При добавлении новых элементов в заполненную коллекцию ротация идет из конца в начало.

Основной конструктор:

```
ObservableRingBuffer(int capacity)
```

Ключевые свойства:

```
int Capacity { get; }
```

#### `class ValidationResult`

Используется в `ConditionValidator`. Ключевые свойства:

```
bool Condition { get; }
string ErrorMessage { get; }
```

### ViewModels

Прослойка между данными и окнами.

#### `class LogEntryEventArgsViewModel`

Представление строки лога. Основной конструктор:

```
LogEntryEventArgsViewModel(LogEntryEventArgs subject);
```

Ключевые свойства:

```
public string Time { get; }
public string Level { get; }
public string Scope { get; }
public int ThreadId { get; }
public string Message { get; }
```