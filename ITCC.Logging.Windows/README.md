# ITCC.Logging.Windows

Различные реализации логов для .Net 4.6. Содержит следующие компоненты (более подробное описание - исходники):

### Loggers

Уже написанные варианты получателя лога

#### `class RotatingRequestLogger`

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

#### `class EmailLogger : IFlushableLogReceiver`

Отправляет логи по почте Основной конструктор 
```
EmailLogger(LogLevel level, EmailLoggerConfiguration configuration);
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