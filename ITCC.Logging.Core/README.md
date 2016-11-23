# ITCC.Logging.Core

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

void FlushAll();                                                             // сброс очередей всех логгеров типа IFlushableLogReceiver
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
Task Flush();
```

### Loggers

Уже написанные варианты получателя лога

#### `class BufferedFileLogger : FileLogger, IFlushableLogReceiver`

Пишет лог в файл, буферизируя содержимое некоторое время (В текущей реализации через `ConcurrentQueue<LogEntryEventArgs>`). Основной конструктор 
```
BufferedFileLogger(string filename, LogLevel level, bool clearFile = false, double frequency = 10000);
```

#### `class BufferedRotatingFileLogger : IFlushableLogReceiver`

Пишет лог в файл, буферизируя содержимое некоторое время (В текущей реализации через `ConcurrentQueue<LogEntryEventArgs>`). Файлы ротируются при достижении определенного размера.  
**ВАЖНО: файлы ротируются только после очередного сброса очереди. Не гарантируется, что их размер не превышает `maxFileSize`**  
Основной конструктор 
```
BufferedRotatingFileLogger(string filenamePrefix, LogLevel level, int filesCount = 10, long maxFileSize = 10 * 1024 * 1024, double frequency = 10000);
```

#### `class ColouredConsoleLogger : ConsoleLogger`

Выводит лог в консоль, раскрашивая его в зависимости от уровня. Использует дополнительные блокировки. Основной конструктор
```
ColouredConsoleLogger(LogLevel level);
```

#### `class ConsoleLogger : ILogReceiver`

Выводит лог в консоль. Основной конструктор 
```
ConsoleLogger(LogLevel level);
```

#### `class DebugLogger : ILogReceiver`

Выводит лог в дебаггер (System.Diagnostics.Debug). Основной конструктор 
```
DebugLogger(LogLevel level);
```

#### `class FileLogger : ILogReceiver`

Пишет лог в файл. Основной конструктор
```
FileLogger(string filename, LogLevel level, bool clearFile = false);
```

#### `class RotatingRequestLogger ; ILogReceiver`

Логгер, который собирает сообщения с список и выдает последние несколько сообщений по запросу. **Использует блокировки на очередь**

Основной конструктор

```
RotatingRequestLogger(int capacity, LogLevel level);
```

Ключевые методы:

```
List<LogEntryEventArgs> GetEntries();          // Получение всех хранимых записей
List<LogEntryEventArgs> GetEntries(int count); // Полученного заданного количества хранимых записей
void Flush();                                  // Очистка очереди записей
```
