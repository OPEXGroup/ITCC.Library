﻿## ITCC.WPF

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


#### `class EnumDescriptionTypeConverter`

Применяется для вывода описания элемента перечисления вместо его значения в графическом интерфейсе, при этом модификация XAML не требуется. Текст описания берется либо из значение в атрибуте `System.ComponentModel.DescriptionAttribute`, либо из файла ресурсов (при использовании `ITCC.WPF.Utils.LocalizedDescriptionAttribute`).

```
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum MyEnum
{
    [Description("Описание значения № 1")]
    Value1,
    [Description("Описание значения № 2")]
    Value2
}
```


#### `class LocalizedDescriptionAttribute`

Применяется для локализации перечислений в приложениях при помощи resx-файлов. При использовании необходимо указать ключ и тип ресурса.

```
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum MyEnum
{
    [LocalizedDescription("MyEnumValue1Description", typeof(ResourceType))]
    Value1,
    [LocalizedDescription("MyEnumValue2Description", typeof(ResourceType))]
    Value2
}
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