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

### Enums

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

### Utils

#### `class RequestResult<TResult>`

Класс для представления результата запроса. Ключевые свойства:

```
TResult Result;                                     // Результат запроса (объекта)
ServerResponseStatus Status;                        // Определяется по статус-коду ответа сервера (либо невозможности его получить)
IDictionary<string, string> Headers { get; set; };  // Заголовки ответа
Exception Exception { get; set;}                    // Исключение, возникшее при выполнении запроса.
```