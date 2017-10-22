# ITCC.Logging.Core

[![NuGet version](https://badge.fury.io/nu/ITCC.Logging.Core.svg)](https://badge.fury.io/nu/ITCC.Logging.Core)

**Компонент работает в .Net 4.6, .Net Core, ASP.NET Core, Xamarin**  
Библиотека для логгирования в сильно многопоточной среде. Основана на `event`'ах. Работает по упрощенному принципу `pub-sub`, при этом логгер выступает брокером. Содержит следующие компоненты (более подробное описание - исходники):

### Корневые

#### `enum Loglevel`

Возможные уровни лога (серьезности сообщений). Значения:

```
None,
Critical,
Error,
Warning,
Info,
Debug,
Trace
```

#### `class LogEntryEventArgs`

Служебный класс. Аргументы события, соответствующего одной строчке лога.

#### `static class Logger`

Класс-брокер. Создает события, подписывает на них получателей лога. Ключевые методы:

```    
void LogEntry(object scope, LogLevel level, string message);                 // записать в лог сообщение 
void LogDebug(object scope, string message);                                 // записать в лог сообщение если определен DEBUG
void LogTrace(object scope, string message);                                 // записать в лог сообщение если определены DEBUG и TRACE

void LogException(object scope, LogLevel level, Exception exception);        // записать в лог исключение
void LogExceptionDebug(object scope, Exception exception);                   // записать в лог исключение если определен DEBUG
void LogExceptionTrace(object scope, Exception exception);                   // записать в лог исключение если определены DEBUG и TRACE

void AddBannedScope(object scope);                                           // убрать из вывода сообщения с заданным контекстом
bool RegisterReceiver(ILogReceiver receiver, bool mutableReceiver = false);  // подписать получателя на события лога
bool UnregisterReceiver(ILogReceiver receiver);                              // отписать получателя

Task FlushAllAsync();                                                        // асинхронный сброс очередей всех логгеров типа IFlushableLogReceiver
```

Ключевые свойства

```
LogLevel Level { get; set; } // уровень лога
```

### Interfaces

#### `interface ILogReceiver`

Интерфейс подписчика на события лога. Сигнатура:

```
LogLevel Level { get; set; }
void WriteEntry(object sender, LogEntryEventArgs args);
```

#### `interface IFlushableLogReceiver : ILogReceiver`

Интерфейс логгера с немгновенным сбросом очереди сообщений. Сигнатура:

```
Task FlushAsync();
```

### Loggers

Уже написанные варианты получателя лога

#### `class DebugLogger : ILogReceiver`

Выводит лог в дебаггер (System.Diagnostics.Debug). Основной конструктор 
```
DebugLogger(LogLevel level);
```

#### `class PortableBufferedFileLogger : FileLogger, IFlushableLogReceiver`

**НЕ РАБОТАЕТ в полном .Net**. 
Пишет лог в файл, буферизируя содержимое некоторое время (В текущей реализации через `ConcurrentQueue<LogEntryEventArgs>`). Основной конструктор 
```
PortableBufferedFileLogger(string filename,     // Имя файла с логом
    LogLevel level,                             // Уровень лога
    bool clearFile = false,                     // Очищать ли файл (если нет - писать в конец)
    double frequency = 10000,                   // Частота сброса буфера в миллисекундах
    int maxQueueSize = int.MaxValue);           // Размер очереди сообщений в памяти, после которого начинается сброс
```

#### `class PortableBufferedRotatingFileLogger : IFlushableLogReceiver`

**НЕ РАБОТАЕТ в полном .Net**. 
Пишет лог в файл, буферизируя содержимое некоторое время (В текущей реализации через `ConcurrentQueue<LogEntryEventArgs>`). Файлы ротируются при достижении определенного размера.  
**ВАЖНО: файлы ротируются только после очередного сброса очереди. Не гарантируется, что их размер не превышает `maxFileSize`**  
Основной конструктор 
```
PortableBufferedRotatingFileLogger(string filenamePrefix, LogLevel level, int filesCount = 10, long maxFileSize = 10 * 1024 * 1024, double frequency = 10000);
```

#### `class PortableFileLogger : ILogReceiver`

**НЕ РАБОТАЕТ в полном .Net**. 
Пишет лог в файл. Основной конструктор
```
PortableFileLogger(string filename, LogLevel level, bool clearFile = false);
```


