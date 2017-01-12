# ITCC.HTTP.Client

HTTP-клиенты. Не бросают исключений, сильно настраиваемы.

### Common

#### `static class Delegates`

Здесь просто хранятся объявления делегатов

```
delegate bool AuthentificationDataAdder(HttpRequestMessage request); // Метод добавления данных авторизации к клиентскому запросу
delegate TResult BodyDeserializer<out TResult>(string data); // Метод десериализации тела ответа от сервера 
delegate string BodySerializer(object data); // Сериализация тела сообщения
```

### Core

#### `static class StaticClient`

`HTTP`-клиент. Ключевые методы: 

##### GET-запросы

* Получение ответа на `GET`-запрос в виде простой строки тела

```
Task<RequestResult<string>> GetRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
```

* Получение ответа на `GET`-запрос в десериализованном виде

```
Task<RequestResult<TResult>> GetAsync<TResult>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
    where TResult : class
```

* Получение ответа на `GET`-запрос в виде успеха или ошибки

```
Task<VariadicRequestResult<TSuccess, TError>> GetVariadicAsync<TSuccess, TError>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
    where TSuccess : class
    where TError : class
```

* Получение ответа на `GET`-запрос в виде успеха или ошибки вида `ITCC.HTTP.API.Utils.ApiErrorView`

```
Task<VariadicRequestResult<TSuccess, ApiErrorView>> GetWithApiErrorAsync<TSuccess>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
    where TSuccess : class
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

##### POST-запросы

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

* Отправка объекта, сериализованного с помощью `BodySerializer` и десериализация ответа

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

* Отправка объекта, сериализованного с помощью `BodySerializer` и десериализация ответа в виде успеха или ошибки

```
Task<VariadicRequestResult<TSuccess, TError>> PostVariadicAsync<TSuccess, TError>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
    where TSuccess : class
    where TError : class
```

* Отправка объекта, сериализованного с помощью `BodySerializer`
и десериализация ответа в виде успеха или ошибки вида `ITCC.HTTP.API.Utils.ApiErrorView`

```
Task<VariadicRequestResult<TSuccess, ApiErrorView>> PostWithApiErrorAsync<TSuccess>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
    where TSuccess : class
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

##### PUT-запросы

```
Task<RequestResult<string>> PutRawAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
```

* Отправка объекта, сериализованного с помощью `BodySerializer` и десериализация ответа в виде строки или ошибки

```
Task<VariadicRequestResult<string, TError>> PutVariadicAsync<TError>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
    where TError : class
```

* Отправка объекта, сериализованного с помощью `BodySerializer`
и десериализация ответа в виде строки или ошибки вида `ITCC.HTTP.API.Utils.ApiErrorView`

```
Task<VariadicRequestResult<string, ApiErrorView>> PutWithApiErrorAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
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

##### DELETE-запросы

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

* Отправка объекта, сериализованного с помощью `BodySerializer` и десериализация ответа в виде строки или ошибки

```
Task<VariadicRequestResult<string, TError>> DeleteVariadicAsync<TError>(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
    where TError : class
```

* Отправка объекта, сериализованного с помощью `BodySerializer`
и десериализация ответа в виде строки или ошибки вида `ITCC.HTTP.API.Utils.ApiErrorView`

```
Task<VariadicRequestResult<string, ApiErrorView>> DeleteWithApiErrorAsync(
            string partialUri,
            IDictionary<string, string> parameters = null,
            IDictionary<string, string> headers = null,
            object data = null,
            Delegates.AuthentificationDataAdder authentificationProvider = null,
            CancellationToken cancellationToken = default(CancellationToken))
```

##### TLS-сертификаты

* Разрешить/запретить соединение с сервером с недоверенным сертификатом

```
void AllowUntrustedServerCertificates();
void DisallowUntrustedServerCertificates();
```

Ключевые свойства:
```
IBodySerializer BodySerializer { get; set; }                    // Объект для сериализации тел запросов
ContentType DefaultContentType { get; set; } = ContentType.Json;// Тип контента, предполагаемый при отсутствии в ответе Content-Type заголовка

string ServerAddress {get; set;}                                // Полный адрес сервера (<proto>://<fqdn>:<port>)
Protocol ServerProtocol { get; }                                // Протокол общения с сервером (определяется на основе адреса)
bool AllowGzipEncoding {get; set;}                              // Отправляет ли клиент заголовки Accept-Encoding: gzip, deflate
/*
    Перенаправления НИКОГДА не делаются автоматически для запросов, отличных от GET и HEAD
*/
int AllowedRedirectCount { get; set; } = 1;                     // Максимальное количество автоматически обрабатываемых перенаправлений
bool AllowRedirectHostChange { get; set; } = false;             // Могут ли перенаправления вести на посторонние хосты
bool PreserveAuthorizationOnRedirect { get; set; } = true;      // Используется ли авторизационный метод после перенаправления
```

#### `class RegularClient`

Предоставляет тот же интефейс, что и `StaticClient`, но не является статическим.

### Enums

#### `enum ServerResponseStatus`

Тип ответа сервера. Значения

```
Ok,                        // Все хорошо (200, 201, 202, 206)
NothingToDo,               // Данных нет (204)
Redirect,                  // Перенаправление (301, 302)  
ClientError,               // Ошибка в клиентском запросе (400, 404, 405, 409, 413, 415, 416)
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

### Interfaces

Используемые интерфейсы

#### `interface IBodySerializer`

Интерфейс сериализатора тел запросов.
Реализации, доступные по умолчанию - `JsonBodySerializer` и `XmlBodySerializer`.

```
Encoding Encoding { get; }
string ContentType { get; }
string Serialize(object data);
```

### Utils

#### `class JsonBodySerializer : IBodySerializer`

Сериализатор в JSON (UTF-8)

#### `class RequestResult<TResult>`

Класс для представления результата запроса. Ключевые свойства:

```
TResult Result { get; set; }                        // Результат запроса (объекта)
ServerResponseStatus Status { get; set; }           // Определяется по статус-коду ответа сервера (либо невозможности его получить)
IDictionary<string, string> Headers { get; set; };  // Заголовки ответа
Exception Exception { get; set;}                    // Исключение, возникшее при выполнении запроса.
```

#### `class NoError`

Заглушка для отсутствия представления ошибки. Пустой запечатанный класс.

#### `class NoError`

Заглушка для отсутствия тела ответа. Пустой запечатанный класс.

#### `class VariadicRequestResult<TSuccess, TError> where TSuccess : class where TError : class`

Класс для представления результата запроса с двумя возможными типами данных в ответе.
Реализован через `ITCC.HTTP.API.Utils.Either<TFirst, TSecond>`.
Ключевые свойства:

```
TSuccess Success { get; }                           // Результат запроса (успех)
bool IsSuccess { get; }                             // Результат запроса (успех) не null

TError Error { get; }                               // Результат запроса (успех)
bool IsError { get; }                               // Результат запроса (успех) не null

ServerResponseStatus Status { get; }                // Определяется по статус-коду ответа сервера (либо невозможности его получить)
IDictionary<string, string> Headers { get; set; };  // Заголовки ответа
Exception Exception { get; }                        // Исключение, возникшее при выполнении запроса.
```

#### `class XmlBodySerializer : IBodySerializer`

Сериализатор в XML (UTF-8)
