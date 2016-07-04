# ITCC Library

## Подключение

Клонирование

* `git clone git@gitlab.itcc.company:OPEXGroup/ITCC.Library.git`

Подключение в качестве подмодуля

* `git submodule add git@gitlab.itcc.company:OPEXGroup/ITCC.Library.git`

Потом, для подключения после клонирования родительского проекта

* `git submodule init`
* `git submodule update --init --recursive` # у нее есть свои подмодули

Собирается из `Visual Studio`, целевая среда - `.Net 4.6 (x64)`.

## Компоненты

Везде в описаниях опущен модификатор `public`, поскольку вещи, не входящие в интерфейс, явно не описываются.

### ITCC.Geocoding

#### Корневые

##### `static class Geocoder`

Геокодер для работы с различными провайдерами данных. Ключевые методы:

```
async Task<Point> GeocodeAsync(string location, GeocodingApi apiType);
void SetApiKey(string key, GeocodingApi apiType);
```


#### Enums

Перечисления

##### `enum GeocodingApi`

Провайдер данных

#### Utils

Служебные классы

##### `class Point`

Простое представление точки на проскости. Основные свойства:

```
double Latitude { get; set; }
double Longitude { get; set; }
```

#### Yandex

Работа с API Яндекса

##### `enum Enums.KindType`

Тип объекта

##### `enum Enums.LangType`

Язык запроса (и ответа)

##### `struct GeoBound`

Представляет границу прямоугольной области. Поля:

```
GeoPoint LowerCorner; // Левый нижний угол
GeoPoint UpperCorner; // Правый верхний угол
```

##### `class GeoMetaData`

Метаданные геокодера. Поля:

```
KindType Kind = KindType.Locality;  // Тип геообъекта
string Text = string.Empty;         // Текст
```

##### `class GeoObject`

Географический объект. Свойства:

```
GeoPoint Point { get; set; }                // Точка (для точечных объектов)
GeoBound BoundedBy { get; set; }            // Граница (для объектов с размером)
GeoMetaData GeocoderMetaData { get; set; }  // Метаданные геокодера
```

##### `class GeoObjectCollection : List<GeoObject>`

Коллекция географических объектов. Конструктор:

```
GeoObjectCollection(string xml); // xml - ответа геокодера Яндекса
```

##### `struct GeoPoint`

Точка. Поля:

```
public double Latitude;   // Широта
public double Longitude;  // Долгота
```

##### `struct SearchArea`

Зоня поиска. Поля:

```
GeoPoint LongLat;   // Центр
GeoPoint Spread;    // Разброс по широте и долготе
```

##### `static class YandexGeocoder`

Основной класс проекта. Работа с API Яндека. Основные свойства:

```
string Key { get; set; } = string.Empty; // Ключ API
```

Основные методы:

```
async Task<GeoObjectCollection> GeocodeAsync(string location, short results = DefaultResultCount, LangType lang = LangType.en_US); // DefaultResultCount == 10
async Task<GeoObjectCollection> GeocodeAsync(string location, short results, LangType lang, SearchArea searchArea, bool rspn = false);
```

### ITCC.Logging

Библиотека для логгирования в сильно многопоточной среде. Основана на `event`'ах. Работает по упрощенному принципу `pub-sub`, при этом логгер выступает брокером. Содержит следующие компоненты (более подробное описание - исходники):

#### Корневые

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
void FlushAll();                                                             // сброс очередей всех логгеров типа IFlushableLogReceiver
```

Ключевые свойства

```
LogLevel Level { get; set; } // уровень лога
```

#### Interfaces

##### `interface ILogReceiver`

Интерфейс подписчика на события лога. Сигнатура:

```
LogLevel Level { get; set; }
void WriteEntry(object sender, LogEntryEventArgs args);
```

##### `interface IFlushableLogReceiver : ILogReceiver`

Интерфейс логгера с немгновенным сбросом очереди сообщений. Сигнатура:

```
Task Flush();
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

##### `class BufferedFileLogger : FileLogger, IFlushableLogReceiver`

Пишет лог в файл, буферизируя содержимое некоторое время (В текущей реализации через `ConcurrentQueue<LogEntryEventArgs>`). Основной конструктор 
```
BufferedFileLogger(string filename, LogLevel level, bool clearFile = false, double frequency = 10000);
```

##### `class BufferedRotatingFileLogger : IFlushableLogReceiver`

Пишет лог в файл, буферизируя содержимое некоторое время (В текущей реализации через `ConcurrentQueue<LogEntryEventArgs>`). Файлы ротируются при достижении определенного размера.  
**ВАЖНО: файлы ротируются только после очередного сброса очереди. Не гарантируется, что их размер не превышает `maxFileSize`**  
Основной конструктор 
```
BufferedRotatingFileLogger(string filenamePrefix, LogLevel level, int filesCount = 10, long maxFileSize = 10 * 1024 * 1024, double frequency = 10000);
```

##### `class SystemEventLogger : ILogReceiver, IDisposable`

Пишет лог в системные логи `Windows`. **Не поддерживает уровни логи ниже `Info`.** Основной конструктор 
```
SystemEventLogger(string source, LogLevel level);
```

##### `class EmailLogger : IFlushableLogReceiver`

Отправляет логи по почте Основной конструктор 
```
EmailLogger(LogLevel level, EmailLoggerConfiguration configuration);
```

#### Utils

##### `class EmailLoggerConfiguration`

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

### ITCC.HTTP

Библиотека для асинхронной работы с сетью по протоколу `HTTP(S)/1.1`. Поддерживает и сторону клиента, и сторону сервера. Основана на `Griffin.Framework` в серверной части. Содержит следующие компоненты (более подробное описание - исходники):

#### Client

Работа с клиентской стороны.

##### `class RequestResult<TResult>`

Класс для представления результата запроса. Ключевые свойства:

```
TResult Result;                                     // Результат запроса (объекта)
ServerResponseStatus Status;                        // Определяется по статус-коду ответа сервера (либо невозможности его получить)
IDictionary<string, string> Headers { get; set; };  // Заголовки ответа
Exception Exception { get; set;}                    // Исключение, возникшее при выполнении запроса.
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
* Сохранение тела ответа на `GET`-запрос в файл
```
public async Task<RequestResult<string>> GetFileAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string fileName = null,
            bool allowRewrite = true,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
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
* Загрузка файла на сервер методом POST
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
* Загрузка файла на сервер методом PUT
```
Task<RequestResult<object>> PutFileAsync(string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            string filePath = null,
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
bool AllowGzipEncoding {get; set;}
```

##### `class RegularClient`

Предоставляет тот же интефейс, что и `StaticClient`, но не является статическим.

#### Common

Общие классы

##### `static class Delegates`

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
delegate Task<AuthentificationResult> Authentificator(HttpRequest request); // Метод аутентификации на сервере (применяется к запросам на /login)
delegate Task<AuthorizationResult<TAccount>> Authorizer<TAccount>(
            HttpRequest request,
            RequestProcessor<TAccount> requestProcessor)
            where TAccount : class; // Метод сервера, позволяющий определить, разрешен ли данный запрос
delegate Task<bool> StatisticsAuthorizer(HttpRequest request); // Метод сервера, позволяющий определить, разрешен ли запрос на /statistics
delegate Task<AuthorizationResult<TAccount>> FilesAuthorizer<TAccount>(
            HttpRequest request,
            FileSection section,
                  string filename)
            where TAccount : class; // Метод сервера, позволяющий определить, разрешен ли данный запрос к файлам.
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
NotRequired,        // Запрос разрешен всем (200)
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
ClientError,               // Ошибка в клиентском запросе (400, 404, 405, 409, 413)
ServerError,               // Ошибка на сервере (500, 501)
Unauthorized,              // Данные авторизации неверны или недостаточны (401)
Forbidden,                 // Доступ запрещен для данного аккаунта (403)
TooManyRequests,           // Слишком много запросов с данного аккаунта (429).
IncompehensibleResponse,   // Ответ непонятен
RequestCanceled,           // Запрос отменен клиентом до получения ответа
RequestTimeout,            // Ответ не получен за заданное время
ConnectionError            // Ошибка сетевого соединения
```

##### `enum ServerStartStatus`

Результат старта сервера. Значения:
```
Ok,                 // Сервер работает
BindingError,       // Ошибка биндинга (вероятнее всего, занят порт)
CertificateError,   // Ошибка получения сертификата (не найдет для данного subject name)
BadParameters,      // Ошибка в переданной конфигурации сервера
AlreadyStarted,     // Ошибка в переданной конфигурации сервера
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
object AccountView { get; set; }                            // Представление аккаунта
HttpStatusCode Status { get; set; }                         // Код ответа
IDictionary<string, string> AdditionalHeaders { get; set; } // Дополнительные заголовки ответа
```

##### `class AuthorizationResult<TAccount>`

Представляет результат авторизации на сервере. Ключевые свойства:
```
TAccount Account { get; set; }                              // аккаунт пользователя
AuthorizationStatus Status { get; set; }                    // статус авторизации
string ErrorDescription { get; set; }                       // описание ошибки (при ее наличии)
IDictionary<string, string> AdditionalHeaders { get; set; } // Дополнительные заголовки ответа
```

##### `class FileSection`

Представление секции файлов на сервере. Файловые секции соответствуют локальным папкам и имеют раздельные права доступа. Ключевые свойства:

```
string Name { get; set; }           // Название секции. Не имеет прямого отношения к пути
string Folder { get; set; }         // Папка (часть uri и часть пути в файловой системе
long MaxFileSize {get; set; } = -1  // Максимально разрешенный размер файлов (на попытку создать бОльший сервер ответит 413)
```


##### `class HandlerResult`

Представляет результат обработки клиентского запроса на сервере. Ключевые свойства:
```
object Body { get; set; }                       // Тело ответа
HttpStatusCode Status { get; set; }             // Код ответа
IDictionary<string, string> additionalHeaders;  // Дополнительные заголовки ответа
```

##### `class HttpServerConfiguration<TAccount>`

Конфигурация сервера. Ключевые методы:
```
bool IsEnough();     // Достаточна ли конфигурация для запуска
```

Ключевые свойства:
```
string SubjectName { get; set; }                                                  // Доменное имя сервера (главная цель - поис сертификата)
ushort Port { get; set; }                                                         // Порт, на котором принимаем соединения
Protocol Protocol { get; set; }                                                   // Используемый протокол

Delegates.CertificateProvider CertificateProvider { get; set; }                   // Метод получения сертификата
bool AllowSelfSignedCertificates { get; set; }                                    // Можно ли использовать самоподписанные сертификаты
System.Security.Authentification.SslProtocols SuitableSslProtocols { get; set; }  // Разрешенные для клиентов версии SSL/TLS 

bool FilesEnabled { get; set; }                                                   // Поддерживает ли сервер работу с файлами
string FilesLocation { get; set; }                                                // Расположение файлов на сервер
string FilesBaseUri { get; set; }                                                 // URI (частичный, уникальный) для доступа к файлам. Файлы в итоге доступны по адресу <SubjectName>:<Port>/<FilesBaseUri>/<filename>
bool FilesNeedAuthorization { get; set; }                                         // Требуется ли авторизация для доступа к файлам
/* Секции файлов, используемые на сервере. Если список пуст, на любой файловый запрос сервер будет отвечать 400 Bad Request */
List<FileSection> FileSections { get; set; } = new List<FileSection>();           

Delegates.FilesAuthorizer<TAccount> FilesAuthorizer { get; set; }                 // Метод авторизации для файлов
Delegates.Authentificator Authentificator { get; set; }                           // Метод аутентификации
Delegates.Authorizer<TAccount> Authorizer { get; set; }                           // Метод авторизации
Delegates.StatisticsAuthorizer StatisticsAuthorizer { get; set; }                 // Метод авторизации для статистики

Delegates.BodySerializer BodySerializer { get; set; }                             // Метод для сериализации тел ответов
System.Text.Encoding BodyEncoding { get; set; } = Encoding.UTF8;                  // Кодировка ответов
bool AutoGzipCompression { get; set; } = true;                                    // Используется ли клиентский заголовок Accept-Encoding: gzip

bool LogResponseBodies { get; set; } = true;                                      // Писать ли в Trace-лог тела отправляемых ответов
int ResponseBodyLogLimit { get; set; } = -1;                                      // Ограничение на длину тела логгируемого значения. Отрицательное значение - нет ограничения

bool StatisticsEnabled { get; set; }                                              // Ведется ли статистика на сервере. Если да, она доступна по <SubjectName>:<Port>/statistics

string FaviconPath { get; set; }                                                  // Где лежит favicon.ico

string ServerName { get; set; }                                                   // Имя сервера для заголовков Server:

int BufferPoolSize { get; set; } = 100;                                           // Размер пула 64к-буферов для чтения из сокетов
double RequestMaxServeTime { get; set; } = 1;                                     // Допустимое время обработки запроса (после него кидается предупреждение)
```

##### `static class MimeTypes`

Класс для работы с `Content-Type` заголовками в ответах сервера. Ключевые методы:

```
string GetTypeByExtenstion(string extension); // Получение стандартного значения Content-Type по расширению файла
```

##### `class RequestProcessor<TAccount>`

Представляет обработчик запроса. Ключевые поля:
```
bool AuthorizationRequired;                  // Требуется ли авторизация для выполнения запроса
Delegates.RequestHandler<TAccount> Handler;  // Функция-обработчик
/*
  **ВАЖНО**: `StaticServer` не поддерживает регистрацию обработчиков с методами `HEAD` и `OPTIONS`. Запросы с этими методами обрабатываются в соответствием со стандартом.
*/
HttpMethod Method;                           // Метод запроса. 
string SubUri;                               // URI, к которому нужно обратиться для вызова метода
```

##### `static class StaticServer<TAccount> where TAccount : class`

Представляет `HTTP(S)`-сервер. Доступен один на приложение. Ключевые методы:
```
ServerStartStatus Start(HttpServerConfiguration<TAccount> configuration);                 // Запуск сервера
void Stop();                                                                              // Синхронная остановка сервера (с очисткой списка обработчиков)
bool AddRequestProcessor(RequestProcessor<TAccount> requestProcessor);                    // Добавление обработчика запросов
bool AddRequestProcessorRange(IEnumerable<RequestProcessor<TAccount>> requestProcessors); // Добавление нескольких обработчиков запросов
```

#### Utils

Разнообразные служебные классы

##### `static class CommonHelper`

Класс для простых общих методов. Ключевые методы:
```
bool IsGoodServerResponseStatus(ServerResponseStatus status);
bool IsBadServerResponseStatus(ServerResponseStatus status);
HttpMethod HttpMethodToEnum(string methodName);
```

### ITCC.UI

Библиотека классов (в том числе, окон) для использования в WPF-приложениях

#### Attributes

Классы используемых атрибутов

##### `class DataGridColumnStyleAttribute`

Используется `DataGridHelper` для задания размера и стиля колонки. Используется при создании `ViewModel`'ов. Конструктор:

```
DatagridColumnStyleAttribute(bool wrappedText = false, double columnPreferredWidth = -1, DataGridLengthUnitType columnWidthUnitType = DataGridLengthUnitType.Auto);
```

##### `class DataGridIgnoreAttribute`

Используется `DataGridHelper` для опционального игнорирования столбца:

```
DataGridIgnoreAttribute(bool ignoreColumn = true)
```

##### `class HeaderTooltipAttribute`

Используется `DataGridHelper` для генерации пояснений к заголовкам в `Datagrid`. Используется при создании `ViewModel`'ов. Конструктор:

```
HeaderTooltipAttribute(string tooltipContent, bool isLongTooltip = false)
```

#### Common

Общие классы

##### `static class Delegates`

Тут просто хранятся объявления делегатов библитеки. Публичные:

```
/*
  Используется в ObservableLogger
*/
delegate void UiThreadRunner(Action action);
```

#### Loggers

Уже написанные варианты получателя лога для WPF-прриложений

##### `class MessageLogger : ILogReceiver`

Выводит лог через `MessageBox`'ы. Основной конструктор 
```
MessageLogger(LogLevel level);
```

##### `class ObservableLogger : ILogReceiver`

Предоставляет `ObservableCollection` для отображения в интерфейсе. Используется с `LogWindow`.  Основной конструктор
```
ObservableLogger(LogLevel level, int capacity, UiThreadRunner uiThreadRunner)
```

#### Utils

Разнообразные служебные классы

##### `class BoundedObservableCollection<TItem> : ObservableCollection<TItem>`

Представляет наблюдаемую коллекцию с ограниченным размером. При добавлении новых элементов в заполненную коллекцию ротация идет из конца в начало.

Основной конструктор:

```
BoundedObservableCollection(int capacity)
```

Ключевые свойства:

```
int Capacity { get; }
```

##### `class ConditionValidator`

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

##### `class DataGridHelper`

Служебный класс для операций с `DataGrid`'ами.

Ключевые методы:

```
/*
  Используется для генерации заголовков. Следует вызывать в обработчиках AutoGeneratingColumn.
  Использует атрибуты DisplayName, HeaderTooltip, DataGridIgnore и DatagridColumnStyle у содержимого Grid'а
*/
static void HandleAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
```

##### `class ScrollViewerExtensions`

Предоставляет дополнительные `DependencyProperty` для `ScrollViewer`. Дополнительные свойства:

```
bool AlwaysScrollToEnd; // Автоматическая прокрутка вниз при добавлении содержимого
```

##### `class ValidationResult`

Используется в `ConditionValidator`. Ключевые свойства:

```
bool Condition { get; }
string ErrorMessage { get; }
```

#### ViewModels

Прослойка между данными и окнами.

##### `class LogEntryEventArgsViewModel`

Представление строки лога. Основной конструктор:

```
LogEntryEventArgsViewModel(LogEntryEventArgs subject);
```

 Ключевые свойства:

```
public string Time { get; }
public string Level { get; }
public string Scope { get; }
public int ThreadId { get; }
public string Message { get; }
```

#### Windows

Классы окон

##### `partial class LogWindow : Window`

Окно для отобрражения лога. Основной конструктор:

```
LogWindow(ObservableLogger logger)
```
