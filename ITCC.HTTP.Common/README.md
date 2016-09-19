# ITCC.HTTP.Common

Общие классы и перечисления

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