# ITCC.HTTP.Server

`HTTP`-сервер. Доступен один экземпляр для приложения.

### Auth

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

### Common

#### `static class Delegates`

Здесь просто хранятся объявления делегатов

```
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
```

### Core

#### `class BodyEncoder`

Класс для сериализации и кодирования тел ответов. Ключевые свойства:

```
public Encoding Encoding { get; set; } = Encoding.UTF8;                                 // Кодировка ответов
public Delegates.BodySerializer Serializer { get; set; } = JsonConvert.SerializeObject; // Метод сериализации объектов в строки
public string ContentType { get; set; } = "application/json";                           // Content-Type
bool AutoGzipCompression { get; set; } = true;                                          // Используется ли клиентский заголовок Accept-Encoding: gzip
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

public BindType CertificateBindType { get; set; } = BindType.SubjectName;         // Метод получения сервером TLS-сертификата для HTTPS-соединений
public bool AllowGeneratedCertificates { get; set; } = false;                     // Параметр, показывающий, разрешается ли сгенерировать сертификат при отсутствии доверенного
public string CertificateFilename { get; set; }                                   // Путь к файлу сертификата в случае, если способом его получения задано "из файла"
public string CertificateThumbprint { get; set; }                                 // Отпечаток сертификата в случае, если он должен быть найден по отпечатку

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

List<BodyEncoder> BodyEncoders { get; set; }                                      // Допустимые способы кодирования ответов
List<Type> NonSerializableTypes { get; set; } = new List<Type>();                 // Типы, сериализация которых производится простым ToString()

bool LogResponseBodies { get; set; } = true;                                      // Писать ли в Trace-лог тела отправляемых ответов
int ResponseBodyLogLimit { get; set; } = -1;                                      // Ограничение на длину тела логгируемого ответа сервера. Отрицательное значение - нет ограничения
int RequestBodyLogLimit { get; set; } = -1;                                       // Ограничение на длину тела логгируемого запроса клиента. Отрицательное значение - нет ограничения
List<Tuple<string, string>> LogBodyReplacePatterns { get; set; }                  // Паттерны замены (заменяемое-замена) для тел сообщений. Служит для исключения из лога секретной информации
	= new List<Tuple<string, string>>();                                          // Поддерживаются регулярные выражения
List<string> LogProhibitedHeaders { get; set; } = new List<string>();             // HTTP-заголовки, значения которых не должны попадать в лог

bool StatisticsEnabled { get; set; }                                              // Ведется ли статистика на сервере. Если да, она доступна по <SubjectName>:<Port>/statistics

string FaviconPath { get; set; }                                                  // Где лежит favicon.ico

string ServerName { get; set; }                                                   // Имя сервера для заголовков Server:

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

### Files

#### `class FileSection`

Представление секции файлов на сервере. Файловые секции соответствуют локальным папкам и имеют раздельные права доступа. Ключевые свойства:

```
string Name { get; set; }           // Название секции. Не имеет прямого отношения к пути
string Folder { get; set; }         // Папка (часть uri и часть пути в файловой системе
long MaxFileSize {get; set; } = -1  // Максимально разрешенный размер файлов (на попытку создать бОльший сервер ответит 413)
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
