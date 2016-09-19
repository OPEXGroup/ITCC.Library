# ITCC.HTTP.Client

HTTP-клиенты. Не бросают исключений, сильно настраиваемы.

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