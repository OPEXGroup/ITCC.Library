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

```
LogLevel Level { get; set; }
void WriteEntry(object sender, LogEntryEventArgs args);
```

##### `enum Loglevel`

Возможные уровни лога (серьезности сообщений)

##### `class LogEntryEventArgs`

Служебный класс. Аргументы события, соответствующего одной строчке лога.

##### `static class Logger`

Класс-брокер. Создает события, подписывает на них получателей лога. Ключевые методы:

```	
void LogEntry(object scope, LogLevel level, string message);                 // записать в лог сообщение 
void LogException(object scope, LogLevel level, Exception exception);        // записать в лог исключение
bool RegisterReceiver(ILogReceiver receiver, bool mutableReceiver = false);  // подписать получателя на события лога
bool UnregisterReceiver(ILogReceiver receiver);                              // отписать получателя
```

Ключевые свойства

```
LogLevel Level { get; set; } // уровень лога
```

#### Loggers

Уже написанные варианты получателя лога

##### `class ConsoleLogger : ILogReceiver`

Выводит лог в консоль. Основной конструктор 
```
ConsoleLogger(LogLevel level);
```

##### `class ColouredConsoleLogger : ConsoleLogger`

Выводит лог в консоль, раскрашивая его в зависимости от уровня. Использует дополнительные блокировки. Основной конструктор
```
ColouredConsoleLogger(LogLevel level);
```

##### `class FileLogger : ILogReceiver`

Пишет лог в файл. Основной конструктор
```
FileLogger(string filename, LogLevel level, bool clearFile = false);
```

##### `class BufferedFileLogger : FileLogger`

Пишет лог в файл, буферизируя содержимое некоторое время (В текущей реализации через `ConcurrentQueue<LogEntryEventArgs>`). Основной конструктор 
```
BufferedFileLogger(string filename, LogLevel level, bool clearFile = false, double frequency = 10000);
```

##### `class SystemEventLogger : ILogReceiver, IDisposable`

Пишет лог в системные логи `Windows`. **Не поддерживает уровни логи ниже `Info`.** Основной конструктор 
```
SystemEventLogger(string source, LogLevel level);
```

### ITCC.HTTP

Библиотека для асинхронной работы с сетью по протоколу `HTTP(S)/1.1`. Поддерживает и сторону клиента, и сторону сервера. Основана на `Griffin.Framework` в серверной части. Содержит следующие компоненты (более подробное описание - исходники):

#### Client

Работа с клиентской стороны.

##### `class RequestResult<TResult>`

Класс для представления результата запроса. Ключевые свойства:

```
TResult Result;               // Результат запроса (объекта)
ServerResponseStatus Status;  // Определяется по статус-коду ответа сервера (либо невозможности его получить)
object Userdata;              // Дополнительные данные (например, значение Retry-After при 429)
```

##### `static class StaticClient`

`HTTP`-клиент. Ключевые методы: 

* Базовый метод. Осуществляет `HTTP`-запрос. Реально почти никогда не используется.
```
async Task<RequestResult<TResult>> PerformRequestAsync<TBody, TResult>(
            HttpMethod method,
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            TBody bodyArg = default(TBody),
            Delegates.BodySerializer requestBodySerializer = null,
            Delegates.BodyDeserializer<TResult> responseBodyDeserializer = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken)) where TResult : class
```

* Получение ответа на `GET`-запрос в виде простой строки тела
```
Task<RequestResult<string>> GetRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
```
* Получения ответа на `GET`-запрос в десериализованном виде (десериализатор передается в явном виде)
```
Task<RequestResult<TResult>> GetDeserializedAsync<TResult>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.BodyDeserializer<TResult> bodyDeserializer = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken)) where TResult : class
```
* Получение ответа на `GET`-запрос в десериализованном из `JSON` виде
```
Task<RequestResult<TResult>> GetAsync<TResult>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken)) where TResult : class
```
* Получение ответа на `POST`-запрос в виде простой строки тела
```
Task<RequestResult<string>> PostRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
```
* Отправка объекта, сериализованного в `JSON` и десериализация ответа из `JSON`
```
Task<RequestResult<TResult>> PostAsync<TResult>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
            where TResult : class
```
* Загрузка файла на сервер
```
Task<RequestResult<object>> PostFileAsync(string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string filePath = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
```
* Получение ответа на `PUT`-запрос в виде простой строки тела
```
Task<RequestResult<string>> PutRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
```
* Получение ответа на `DELETE`-запрос в виде простой строки тела
```
Task<RequestResult<string>> DeleteRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
```
* Разрешить/запретить соединение с сервером с недоверенным сертификатом
```
void AllowUntrustedServerCertificates();
void DisallowUntrustedServerCertificates();
```

Ключевые свойства:
```
string ServerAddress {get; set;}
Protocol ServerProtocol { get; private set; } = Protocol.Http;
```

#### Common

Общие классы

##### `class Delegates`

Тут просто хранятся объявления делегатов библитеки. Публичные:
```
/*
	Клиент
*/
delegate bool AuthentificationDataAdder(HttpRequestMessage request); // Метод добавления данных авторизации к клиентскому запросу
delegate TResult BodyDeserializer<out TResult>(string data); // Метод десериализации тела ответа от сервера 
/*
	Сервер
*/
delegate Task<AuthorizationResult<TAccount>> Authorizer<TAccount>(
            HttpRequest request,
            RequestProcessor<TAccount> requestProcessor)
            where TAccount : class; // Метод сервера, позволяющий определить, разрешен ли данный запрос
delegate Task<AuthorizationResult<TAccount>> FilesAuthorizer<TAccount>(
            HttpRequest request,
			string filename)
            where TAccount : class; // Метод сервера, позволяющий определить, разрешен ли данный запрос к файлам
delegate X509Certificate2 CertificateProvider(string subjectName, bool allowSelfSignedCertificates); // Метод сервера для получения SSL/TLS сертификата
delegate Task<HandlerResult> RequestHandler<in TAccount>(TAccount account, HttpRequest request); // Обработчик клиентского запроса (после авторизации)
/*
	Общие
*/
delegate string BodySerializer(object data); // Сериализация тела сообщения
```

#### Enums

Используемые перечисления

##### `enum AuthorizationStatus`

Результат авторизации на сервере. Значения:

```
NotRequired,		// Запрос разрешен всем (200)
Ok,                 // Запрос разрешен (200)
Unauthorized,       // Данные авторизации неверны или недостаточны (401)
Forbidden,          // Доступ запрещен для данного аккаунта (403)
TooManyRequests     // Доступ будет разрешен позже. (429)
InternalError,      // Внутренняя ошибка сервера (500)
```

##### `enum Protocol`

Протокол уровня приложения. Значения:
```
Http,
Https   // С шифрованием данных
```

##### `enum ServerResponseStatus`

Тип ответа сервера. Значения

```
Ok,                        // Все хорошо (200, 201)
NothingToDo,               // Данных нет (204)
ClientError,               // Ошибка в клиентском запросе (400, 404, 409)
ServerError,               // Ошибка на сервере (500, 501)
Unauthorized,              // Данные авторизации неверны или недостаточны (401)
Forbidden,                 // Доступ запрещен для данного аккаунта (403)
ToManyRequest,             // Слишком много запросов с данного аккаунта (429). При этом в Userdata должно храниться вреся в секундах до следующего разрешенного запроса
IncompehensibleResponse,   // Ответ непонятен
RequestCanceled,           // Запрос отменен клиентом до получения ответа
```

##### `enum ServerStartStatus`

Результат старта сервера. Значения:
```
Ok,                 // Сервер работает
BindingError,       // Ошибка биндинга (вероятнее всего, занят порт)
CertificateError,   // Ошибка получения сертификата (не найдет для данного subject name)
BadParameters,      // Ошибка в переданной конфигурации сервера
UnknownError        // Прочие ошибки
```

#### Security

##### `static class CertificateController`

Управление сертификатами сервера. Ключевой метод:
```
X509Certificate2 GetCertificate(string subjectName, bool allowSelfSigned)
```

#### Server

`HTTP`-сервер. Доступен один экземпляр для приложения.

##### `class AuthentificationResult`

Представляет результат аутентификации на сервера (получения токена авторизации). Отправляется сервером в ответ на запрос `Login`. Ключевые свойства:
```
object AccountView { get; set; }             // Представление аккауниа
HttpStatusCode Status { get; set; }          // Код ответа
object Userdata { get; set; }                // Дополнительные данные (например, значение заголовка Retry-After)
```

##### `class AuthorizationResult<TAccount>`

Представляет результат авторизации на сервере. Ключевые свойства:
```
TAccount Account { get; set; }               // аккаунт пользователя
AuthorizationStatus Status { get; set; }     // статус авторизации
string ErrorDescription { get; set; }        // описание ошибки (при ее наличии)
```

##### `class HandlerResult`

Представляет результат обработки клиентского запроса на сервере. Ключевые свойства:
```
object Body { get; set; }                     // Тело ответа
HttpStatusCode Status { get; set; }           // Код ответа
```

##### `class HttpServerConfiguration<TAccount>`

Конфигурация сервера. Ключевые методы:
```
bool IsEnough();     // Достаточна ли конфигурация для запуска
```

Ключевые свойства:
```
string SubjectName { get; set; }                                    // Доменное имя сервера (главная цель - поис сертификата)
ushort Port { get; set; }                                           // Порт, на котором принимаем соединения
Protocol Protocol { get; set; }                                     // Используемый протокол

Delegates.CertificateProvider CertificateProvider { get; set; }     // Метод получения сертификата
bool AllowSelfSignedCertificates { get; set; }                      // Можно ли использовать самоподписанные сертификаты

bool FilesEnabled { get; set; }                                     // Поддерживает ли сервер работу с файлами
string FilesLocation { get; set; }                                  // Расположение файлов на сервер
string FilesBaseUri { get; set; }                                   // URI (частичный, уникальный) для доступа к файлам. Файлы в итоге доступны по адресу <SubjectName>:<Port>/<FilesBaseUri>/<filename>
bool FilesNeedAuthorization { get; set; }                           // Требуется ли авторизация для доступа к файлам

Delegates.FilesAuthorizer<TAccount> FilesAuthorizer { get; set; }   // Метод авторизации для файлом
Delegates.Authentificator Authentificator { get; set; }             // Метод аутентификации
Delegates.Authorizer<TAccount> Authorizer { get; set; }             // Метод авторизации

Delegates.BodySerializer BodySerializer { get; set; }               // Метод для сериализации тел ответов

bool StatisticsEnabled { get; set; }                                // Ведется ли статистика на сервере. Если да, она доступна по <SubjectName>:<Port>/statistics

string FaviconPath { get; set; }                                    // Где лежит favicon.ico

string ServerName { get; set; }                                     // Имя сервера для заголовков Server:

int BufferPoolSize { get; set; } = 100;                             // Размер пула 64к-буферов для чтения из сокетов
```

##### `class RequestProcessor<TAccount>`

Представляет обработчик запроса. Ключевые поля:
```
bool AuthorizationRequired;                  // Требуется ли авторизация для выполнения запроса
Delegates.RequestHandler<TAccount> Handler;  // Функция-обработчик
HttpMethod Method;                           // Метод запроса
string SubUri;                               // URI, к которому нужно обратиться для вызова метода
```

