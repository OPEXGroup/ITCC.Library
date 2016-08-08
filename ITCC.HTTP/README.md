# ITCC.HTTP

Библиотека для асинхронной работы с сетью по протоколу `HTTP(S)/1.1`. Поддерживает и сторону клиента, и сторону сервера. Содержит следующие компоненты (более подробное описание - исходники):

### Client

Работа с клиентской стороны.

#### `class RequestResult<TResult>`

Класс для представления результата запроса. Ключевые свойства:

```
TResult Result;                                     // Результат запроса (объекта)
ServerResponseStatus Status;                        // Определяется по статус-коду ответа сервера (либо невозможности его получить)
IDictionary<string, string> Headers { get; set; };  // Заголовки ответа
Exception Exception { get; set;}                    // Исключение, возникшее при выполнении запроса.
```

#### `static class StaticClient`

`HTTP`-клиент. Ключевые методы: 

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
string ServerAddress {get; set;}                             // Полный адрес сервера (<proto>://<fqdn>:<port>)
Protocol ServerProtocol { get; }                             // Протокол общения с сервером (определяется на основе адреса)
bool AllowGzipEncoding {get; set;}                           // Отправляет ли клиент заголовки Accept-Encoding: gzip, deflate
/*
    Перенаправления НИКОГДА не делаются автоматически для запросов, отличных от GET и HEAD
*/
int AllowedRedirectCount { get; set; } = 1;                  // Максимальное количество автоматически обрабатываемых перенаправлений
bool AllowRedirectHostChange { get; set; } = false;          // Могут ли перенаправления вести на посторонние хосты
bool PreserveAuthorizationOnRedirect { get; set; } = true;   // Используется ли авторизационный метод после перенаправления
```

#### `class RegularClient`

Предоставляет тот же интефейс, что и `StaticClient`, но не является статическим.

### Common

Общие классы

#### `static class Delegates`

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
delegate Task<AuthentificationResult> Authentificator(HttpListenerRequest request); // Метод аутентификации на сервере (применяется к запросам на /login)
delegate Task<AuthorizationResult<TAccount>> Authorizer<TAccount>(
            HttpListenerRequest request,
            RequestProcessor<TAccount> requestProcessor)
            where TAccount : class; // Метод сервера, позволяющий определить, разрешен ли данный запрос
delegate Task<bool> StatisticsAuthorizer(HttpListenerRequest request); // Метод сервера, позволяющий определить, разрешен ли запрос на /statistics
delegate Task<AuthorizationResult<TAccount>> FilesAuthorizer<TAccount>(
            HttpListenerRequest request,
            FileSection section,
            string filename)
            where TAccount : class; // Метод сервера, позволяющий определить, разрешен ли данный запрос к файлам.
delegate Task<HandlerResult> RequestHandler<in TAccount>(TAccount account, HttpListenerRequest request); // Обработчик клиентского запроса (после авторизации)
/*
    Общие
*/
delegate string BodySerializer(object data); // Сериализация тела сообщения
```

### Enums

Используемые перечисления

#### `enum AuthorizationStatus`

Результат авторизации на сервере. Значения:

```
NotRequired,        // Запрос разрешен всем (200)
Ok,                 // Запрос разрешен (200)
Unauthorized,       // Данные авторизации неверны или недостаточны (401)
Forbidden,          // Доступ запрещен для данного аккаунта (403)
TooManyRequests     // Доступ будет разрешен позже. (429)
InternalError,      // Внутренняя ошибка сервера (500)
```

#### `enum FileOperationStatus`

Результат операции с файлом на сервере. Значения:

```
Ok,                 // Операция завершена успешно
JobQueued,          // Пока ошибок нет, но задача обработки файла поставлена в очередь
BadParameters,      // Метод вызван с неверными параметрами
NotFound,           // Файл или секция не найдены
Conflict,           // Попытка пересоздать существующий файл
FilesNotEnabled,    // Файлы отключены на сервере
Error               // Непредвиденная ошибка
``` 

#### `enum Protocol`

Протокол уровня приложения. Значения:
```
Http,
Https   // С шифрованием данных
```

#### `enum ServerResponseStatus`

Тип ответа сервера. Значения

```
Ok,                        // Все хорошо (200, 201, 202, 206)
NothingToDo,               // Данных нет (204)
Redirect,                  // Перенаправление (301, 302)  
ClientError,               // Ошибка в клиентском запросе (400, 404, 405, 409, 413, 416)
ServerError,               // Ошибка на сервере (500, 501)
Unauthorized,              // Данные авторизации неверны или недостаточны (401)
Forbidden,                 // Доступ запрещен для данного аккаунта (403)
TooManyRequests,           // Слишком много запросов с данного аккаунта (429).
IncompehensibleResponse,   // Ответ непонятен
RequestCanceled,           // Запрос отменен клиентом до получения ответа
RequestTimeout,            // Ответ не получен за заданное время
ConnectionError,           // Ошибка сетевого соединения
TemporaryUnavailable,      // Ресурс временно недоступен (503)
```

#### `enum ServerStartStatus`

Результат старта сервера. Значения:
```
Ok,                 // Сервер работает
BindingError,       // Ошибка биндинга (вероятнее всего, занят порт)
CertificateError,   // Ошибка получения сертификата (не найдет для данного subject name)
BadParameters,      // Ошибка в переданной конфигурации сервера
AlreadyStarted,     // Ошибка в переданной конфигурации сервера
UnknownError        // Прочие ошибки
```

### Server

`HTTP`-сервер. Доступен один экземпляр для приложения.

#### `class AuthentificationResult`

Представляет результат аутентификации на сервера (получения токена авторизации). Отправляется сервером в ответ на запрос `Login`. Ключевые свойства:
```
object AccountView { get; set; }                            // Представление аккаунта
HttpStatusCode Status { get; set; }                         // Код ответа
IDictionary<string, string> AdditionalHeaders { get; set; } // Дополнительные заголовки ответа
```

#### `class AuthorizationResult<TAccount>`

Представляет результат авторизации на сервере. Ключевые свойства:
```
TAccount Account { get; set; }                              // аккаунт пользователя
AuthorizationStatus Status { get; set; }                    // статус авторизации
string ErrorDescription { get; set; }                       // описание ошибки (при ее наличии)
IDictionary<string, string> AdditionalHeaders { get; set; } // Дополнительные заголовки ответа
```

#### `class BodyEncoder`

Класс для сериализации и кодирования тел ответов. Ключевые свойства:

```
public Encoding Encoding { get; set; } = Encoding.UTF8;                                 // Кодировка ответов
public Delegates.BodySerializer Serializer { get; set; } = JsonConvert.SerializeObject; // Метод сериализации объектов в строки
public string ContentType { get; set; } = "application/json";                           // Content-Type
bool AutoGzipCompression { get; set; } = true;                                          // Используется ли клиентский заголовок Accept-Encoding: gzip
```

#### `class FileSection`

Представление секции файлов на сервере. Файловые секции соответствуют локальным папкам и имеют раздельные права доступа. Ключевые свойства:

```
string Name { get; set; }           // Название секции. Не имеет прямого отношения к пути
string Folder { get; set; }         // Папка (часть uri и часть пути в файловой системе
long MaxFileSize {get; set; } = -1  // Максимально разрешенный размер файлов (на попытку создать бОльший сервер ответит 413)
```


#### `class HandlerResult`

Представляет результат обработки клиентского запроса на сервере. Ключевые свойства:
```
object Body { get; set; }                       // Тело ответа
HttpStatusCode Status { get; set; }             // Код ответа
IDictionary<string, string> additionalHeaders;  // Дополнительные заголовки ответа
```

#### `class HttpServerConfiguration<TAccount>`

Конфигурация сервера. Ключевые методы:
```
bool IsEnough();     // Достаточна ли конфигурация для запуска
```

Ключевые свойства:

```
string SubjectName { get; set; }                                                  // Доменное имя сервера (главная цель - поис сертификата)
ushort Port { get; set; }                                                         // Порт, на котором принимаем соединения
Protocol Protocol { get; set; }                                                   // Используемый протокол

bool FilesEnabled { get; set; }                                                   // Поддерживает ли сервер работу с файлами
string FilesLocation { get; set; }                                                // Расположение файлов на сервер
string FilesBaseUri { get; set; }                                                 // URI (частичный, уникальный) для доступа к файлам. Файлы в итоге доступны по адресу <SubjectName>:<Port>/<FilesBaseUri>/<filename>
bool FilesNeedAuthorization { get; set; }                                         // Требуется ли авторизация для доступа к файлам
bool FilesPreprocessingEnabled { get; set; }                                      // Включен ли препроцессинг файлов (изменение размеров)
int FilesPreprocessorThreads { get; set; }                                        // Количество потоков, используемых для препроцессинга файлов. Отрицательные значения - использовать все ядра
double ExistingFilesPreprocessingFrequency { get; set; } = 60;                    // Как часто обрабатываются существующие файлы. Отрицательные значения - никогда
/* Секции файлов, используемые на сервере. Если список пуст, на любой файловый запрос сервер будет отвечать 400 Bad Request */
List<FileSection> FileSections { get; set; } = new List<FileSection>();           

Delegates.FilesAuthorizer<TAccount> FilesAuthorizer { get; set; }                 // Метод авторизации для файлов
Delegates.Authentificator Authentificator { get; set; }                           // Метод аутентификации
Delegates.Authorizer<TAccount> Authorizer { get; set; }                           // Метод авторизации
Delegates.StatisticsAuthorizer StatisticsAuthorizer { get; set; }                 // Метод авторизации для статистики

BodyEncoder BodyEncoder { get; set; }                                             // Способ кодирования ответов                            

bool LogResponseBodies { get; set; } = true;                                      // Писать ли в Trace-лог тела отправляемых ответов
int ResponseBodyLogLimit { get; set; } = -1;                                      // Ограничение на длину тела логгируемого значения. Отрицательное значение - нет ограничения
List<Tuple<string, string>> LogBodyReplacePatterns { get; set; }                  // Паттерны замены (заменяемое-замена) для тел сообщений. Служит для исключения из лога секретной информации
	= new List<Tuple<string, string>>();                                          // Поддерживаются регулярные выражения
List<string> LogProhibitedHeaders { get; set; } = new List<string>();             // HTTP-заголовки, значения которых не должны попадать в лог

bool StatisticsEnabled { get; set; }                                              // Ведется ли статистика на сервере. Если да, она доступна по <SubjectName>:<Port>/statistics

string FaviconPath { get; set; }                                                  // Где лежит favicon.ico

string ServerName { get; set; }                                                   // Имя сервера для заголовков Server:

int BufferPoolSize { get; set; } = 100;                                           // Размер пула 64к-буферов для чтения из сокетов
double RequestMaxServeTime { get; set; } = 1;                                     // Допустимое время обработки запроса (после него кидается предупреждение)
```

#### `static class MimeTypes`

Класс для работы с `Content-Type` заголовками в ответах сервера. Ключевые методы:

```
string GetTypeByExtenstion(string extension); // Получение стандартного значения Content-Type по расширению файла
```

#### `class RequestProcessor<TAccount>`

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

#### `static class StaticServer<TAccount> where TAccount : class`

Представляет `HTTP(S)`-сервер. Доступен один на приложение. Основные возможности:

* Работа со статичными файлами  
* Произвольные схемы аутентификации и авторизации  
* Добавление собственных обработчиков запросов  
* Автоматических сбор статистики ответов и производительности (доступна либо всем, либо только после определенной авторизации)  
* Сборка ответа на OPTIONS  
* Ответы в gzip по запросу  

Ключевые методы:

```
ServerStartStatus Start(HttpServerConfiguration<TAccount> configuration);                 // Запуск сервера
void Stop();                                                                              // Синхронная остановка сервера (с очисткой списка обработчиков)

bool AddRequestProcessor(RequestProcessor<TAccount> requestProcessor);                    // Добавление обработчика запросов
bool AddRequestProcessorRange(IEnumerable<RequestProcessor<TAccount>> requestProcessors); // Добавление нескольких обработчиков запросов
bool AddStaticRedirect(string fromUri, string toUri);                                     // Добавление статичного (302) перенаправления
bool AddStaticRedirectRange(IDictionary<string, string> uriTable);                        // Добавление нескольких статичных (302) перенаправлений

bool FileExists(string sectionName, string filename);                                     // Есть ли файл (ложь в случае неправильных параметров или выключенных файлов)
Stream GetFileStream(string sectionName, string filename);                                // Получение содержимого файла в виде открытого на чтение потока (надо не забыть потом вызвать у него Dispose)
Task<string> GetFileString(string sectionName, string filename);                          // Получение содержимого файла в виде строки.
Task<FileOperationStatus> AddFile(string sectionName, string filename, Stream content);   // Добавление файла на сервер
FileOperationStatus DeleteFile(string sectionName, string filename);                      // Удаление файла
```

### Utils

Разнообразные служебные классы

#### `static class CommonHelper`

Класс для простых общих методов. Ключевые методы:
```
bool IsGoodServerResponseStatus(ServerResponseStatus status);
bool IsBadServerResponseStatus(ServerResponseStatus status);
HttpMethod HttpMethodToEnum(string methodName);
```

