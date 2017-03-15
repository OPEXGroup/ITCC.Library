# ITCC.Logging.Windows

Различные реализации логов для .Net 4.6. Содержит следующие компоненты (более подробное описание - исходники):

### Loggers

Уже написанные варианты получателя лога

#### `class BufferedFileLogger : FileLogger, IFlushableLogReceiver`

Пишет лог в файл, буферизируя содержимое некоторое время (В текущей реализации через `ConcurrentQueue<LogEntryEventArgs>`). Основной конструктор 
```
BufferedFileLogger(string filename,     // Имя файла с логом
    LogLevel level,                     // Уровень лога
    bool clearFile = false,             // Очищать ли файл (если нет - писать в конец)
    double frequency = 10000,           // Частота сброса буфера в миллисекундах
    int maxQueueSize = int.MaxValue);   // Размер очереди сообщений в памяти, после которого начинается сброс
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

#### `class EmailLogger : IFlushableLogReceiver`

Отправляет логи по почте Основной конструктор 
```
EmailLogger(LogLevel level, EmailLoggerConfiguration configuration);
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

#### `class SystemEventLogger : ILogReceiver, IDisposable`

Пишет лог в системные логи `Windows`. **Не поддерживает уровни логи ниже `Info`.** Основной конструктор 
```
SystemEventLogger(string source, LogLevel level);
```

### Utils

#### `class EmailLoggerConfiguration`

Конфигурация почтового логгера. Ключевые свойства:

```
string Login { get; set; }                  // Логин отправителя
string Password { get; set; }               // Пароль отправителя

string Subject { get; set; }                // Общая часть заголовка писем
string Sender { get; set; }                 // Плдпись отправителя (рекомендуется делать равной Login)
List<string> Receivers { get; set; }        // Список адресов получателей

string SmtpHost { get; set; }               // Адрес используемого SMTP-сервера
int SmptPort { get; set; }                  // Порт используемого SMTP-сервера

double ReportPeriod { get; set; }           // Частота штатной отправки отчетов (в секундах)
LogLevel FlushLevel { get; set; }           // Уровень сообщений, при котором производится немедленная отправка всей очереди
bool SendEmptyReports { get; set; }         // Надо ли слать письма о том, что очередь сообщений пуста
int MaxQueueSize { get; set; }              // Размер очереди сообщений, при котором производится немедленная отправка всей очереди
```