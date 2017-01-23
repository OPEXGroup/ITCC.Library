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

### Windows

Классы окон

#### `partial class LogWindow : Window`

Окно для отобрражения и настройки лога. Основной конструктор:

```
LogWindow(ObservableLogger logger)
```
