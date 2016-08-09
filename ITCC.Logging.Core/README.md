# ITCC.Logging.Core

**Компонент работает в .Net 4.6, .Net Core**  
Библиотека для логгирования в сильно многопоточной среде. Основана на `event`'ах. Работает по упрощенному принципу `pub-sub`, при этом логгер выступает брокером. Содержит следующие компоненты (более подробное описание - исходники):

### Корневые

#### `enum Loglevel`

Возможные уровни лога (серьезности сообщений)

#### `class LogEntryEventArgs`

Служебный класс. Аргументы события, соответствующего одной строчке лога.

#### `static class Logger`

Класс-брокер. Создает события, подписывает на них получателей лога. Ключевые методы:

```    
void LogEntry(object scope, LogLevel level, string message);                 // записать в лог сообщение 
void LogException(object scope, LogLevel level, Exception exception);        // записать в лог исключение
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

#### `class DebugLogger : ILogReceiver`

Выводит лог в дебаггер (System.Diagnostics.Debug). Основной конструктор 
```
DebugLogger(LogLevel level);
```
