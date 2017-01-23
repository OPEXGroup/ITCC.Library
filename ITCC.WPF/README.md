## ITCC.WPF

Библиотека классов (в том числе, окон) для использования в WPF-приложениях

### Loggers

Уже написанные варианты получателя лога для WPF-прриложений

#### `class MessageLogger : ILogReceiver`

Выводит лог через `MessageBox`'ы. Основной конструктор 
```
MessageLogger(LogLevel level);
```

### Utils

Разнообразные служебные классы

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

### Windows

Классы окон

#### `partial class LogWindow : Window`

Окно для отобрражения и настройки лога. Основной конструктор:

```
LogWindow(ObservableLogger logger)
```
