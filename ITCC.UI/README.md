## ITCC.UI

Библиотека классов (в том числе, окон) для использования в WPF-приложениях

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

#### `class HeaderTooltipAttribute`

Используется `DataGridHelper` для генерации пояснений к заголовкам в `Datagrid`. Используется при создании `ViewModel`'ов. Конструктор:

```
HeaderTooltipAttribute(string tooltipContent, bool isLongTooltip = false)
```

### Common

Общие классы

#### `static class Delegates`

Тут просто хранятся объявления делегатов библитеки. Публичные:

```
/*
  Используется в ObservableLogger
*/
delegate void UiThreadRunner(Action action);
```

### Loggers

Уже написанные варианты получателя лога для WPF-прриложений

#### `class MessageLogger : ILogReceiver`

Выводит лог через `MessageBox`'ы. Основной конструктор 
```
MessageLogger(LogLevel level);
```

#### `class ObservableLogger : ILogReceiver`

Предоставляет `ObservableCollection` для отображения в интерфейсе. Используется с `LogWindow`.  Основной конструктор
```
ObservableLogger(LogLevel level, int capacity, UiThreadRunner uiThreadRunner)
```

**ВАЖНО**: `uiThreadRunner`, при неправильной реализации, потенциально может вызвать deadlock. Далее примеры для WPF

Рекомендуемая реализация:

```
void RunOnUiThread(Action action) => Current.Dispatcher.BeginInvoke(action);
```

**Нерекомендуемая реализация**:

```
void RunOnUiThread(Action action) => Current.Dispatcher.Invoke(action);
```

### Utils

Разнообразные служебные классы

#### `class BoundedObservableCollection<TItem> : ObservableCollection<TItem>`

Представляет наблюдаемую коллекцию с ограниченным размером. При добавлении новых элементов в заполненную коллекцию ротация идет из конца в начало.

Основной конструктор:

```
BoundedObservableCollection(int capacity)
```

Ключевые свойства:

```
int Capacity { get; }
```

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

#### `class DataGridHelper`

Служебный класс для операций с `DataGrid`'ами.

Ключевые методы:

```
/*
  Используется для генерации заголовков. Следует вызывать в обработчиках AutoGeneratingColumn.
  Использует атрибуты DisplayName, HeaderTooltip, DataGridIgnore и DatagridColumnStyle у содержимого Grid'а
*/
static void HandleAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
```

#### `class ScrollViewerExtensions`

Предоставляет дополнительные `DependencyProperty` для `ScrollViewer`. Дополнительные свойства:

```
bool AlwaysScrollToEnd; // Автоматическая прокрутка вниз при добавлении содержимого
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

### Windows

Классы окон

#### `partial class LogWindow : Window`

Окно для отобрражения и настройки лога. Основной конструктор:

```
LogWindow(ObservableLogger logger)
```
