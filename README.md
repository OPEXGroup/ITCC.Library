# ITCC Library

## Подключение

Клонирование

* `git clone git@gitlab.itcc.company:OPEXGroup/ITCC.Library.git`

Подключение в качестве подмодуля

* `git submodule add git@gitlab.itcc.company:OPEXGroup/ITCC.Library.git`

Потом, для подключения после клонирования родительского проекта

* `git submodule init`
* `git submodule update --init -- recursive` # у нее есть свои подмодули

Собирается из `Visual Studio`, целевая среда - `.Net 4.6 (x64)`.

## Компоненты

Везде в описаниях опущен модификатор `public`, поскольку вещи, не входящие в интерфейс, явно не описываются.

### ITCC.Logging

Библиотека для логгирования в сильно многопоточной среде. Основана на `event`'ах. Работает по упрощенному принципу `pub-sub`, при этом логгер выступает брокером. Содержит следующие компоненты (более подробное описание - исходники):

#### Корневые

##### `interface ILogReceiver`

Интерфейс подписчика на события лога. Сигнатура:

* `LogLevel Level { get; set; }`  
* `void WriteEntry(object sender, LogEntryEventArgs args);`

##### `enum Loglevel`

Возможные уровни лога (серьезности сообщений)

##### `class LogEntryEventArgs`

Служебный класс. Аргументы события, соответствующего одной строчке лога.

##### `static class Logger`

Класс-брокер. Создает события, подписывает на них получателей лога. Ключевые методы:
	
* `void LogEntry(object scope, LogLevel level, string message)` - записать в лог сообщение 
* `void LogException(object scope, LogLevel level, Exception exception)` - записать в лог исключение
* `bool RegisterReceiver(ILogReceiver receiver, bool mutableReceiver = false)` - подписать получателя на события лога
* `bool UnregisterReceiver(ILogReceiver receiver)`- отписать получателя

Ключевые свойства

* `LogLevel Level { get; set; }` - уровень лога

#### Loggers

Уже написанные варианты получателя лога

##### `class ConsoleLogger : ILogReceiver`

Выводит лог в консоль. Основной конструктор `ConsoleLogger(LogLevel level)`

##### `class ColouredConsoleLogger : ConsoleLogger`

Выводит лог в консоль, раскрашивая его в зависимости от уровня. Использует дополнительные блокировки. Основной конструктор `ColouredConsoleLogger(LogLevel level)`

##### `class FileLogger : ILogReceiver`

Пишет лог в файл. Основной конструктор `FileLogger(string filename, LogLevel level, bool clearFile = false)`

##### `class BufferedFileLogger : FileLogger`

Пишет лог в файл, буферизируя содержимое некоторое время (В текущей реализации через `ConcurrentQueue<LogEntryEventArgs>`). Основной конструктор `BufferedFileLogger(string filename, LogLevel level, bool clearFile = false, double frequency = 10000)`

##### `class SystemEventLogger : ILogReceiver, IDisposable`

Пишет лог в системные логи `Windows`. **Не поддерживает уровни логи ниже `Info`.** Основной конструктор `SystemEventLogger(string source, LogLevel level)`

### ITCC.HTTP

Библиотека для асинхронной работы с сетью по протоколу `HTTP(S)/1.1`. Поддерживает и сторону клиента, и сторону сервера. Основана на `Griffin.Framework` в серверной части. Содержит следующие компоненты (более подробное описание - исходники):

#### Client

Работа с клиентской стороны.

##### `class RequestResult<TResult>`

Класс для представления результата запроса. Ключевые свойства:

* `TResult Result;` - Результат запроса (объекта)
* `ServerResponseStatus Status;` - Определяется по статус-коду ответа сервера (либо невозможности его получить)

##### `static class StaticClient`

`HTTP`-клиент. Ключевые методы: 

* `async Task<RequestResult<TResult>> PerformRequestAsync<TBody, TResult>(HttpMethod method, string partialUri,       IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, TBody bodyArg = default(TBody), Delegates.BodySerializer requestBodySerializer = null, Delegates.BodyDeserializer<TResult> responseBodyDeserializer = null, Delegates.AuthentificationDataAdder authentificationProvider = null,        CancellationToken cancellationToken = default(CancellationToken)) where TResult : class` - Базовый метод. Осуществляет `HTTP`-запрос. Реально почти никогда не используется.
* `Task<RequestResult<string>> GetRawAsync(string partialUri, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, Delegates.AuthentificationDataAdder authentificationProvider = null, CancellationToken cancellationToken = default(CancellationToken))` - Получение ответа на `GET`-запрос в виде простой строки тела
* `Task<RequestResult<TResult>> GetDeserializedAsync<TResult>(string partialUri, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, Delegates.BodyDeserializer<TResult> bodyDeserializer = null, Delegates.AuthentificationDataAdder authentificationProvider = null, CancellationToken cancellationToken = default(CancellationToken)) where TResult : class` - Получения ответа на `GET`-запрос в десериализованном виде (десериализатор передается в явном виде)
* `GetAsync<TResult>(string partialUri, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, Delegates.AuthentificationDataAdder authentificationProvider = null, CancellationToken cancellationToken = default(CancellationToken)) where TResult : class` - Получение ответа на `GET`-запрос в десериализованном из `JSON` виде
* `Task<RequestResult<string>> PostRawAsync(string partialUri, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, string data = null, Delegates.AuthentificationDataAdder authentificationProvider = null, CancellationToken cancellationToken = default(CancellationToken))` - Получение ответа на `POST`-запрос в виде простой строки тела
* `Task<RequestResult<TResult>> PostAsync<TResult>(string partialUri, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null, object data = null, Delegates.AuthentificationDataAdder authentificationProvider = null, CancellationToken cancellationToken = default(CancellationToken)) where TResult : class` - Отправка объекта, сериализованного в `JSON` и десериализация ответа из `JSON`





