# ITCC.HTTP.Common

[![NuGet version](https://badge.fury.io/nu/ITCC.HTTP.Common.svg)](https://badge.fury.io/nu/ITCC.HTTP.Common)

Общие классы и перечисления

### `static class Constants`

Просто набор констант

### Enums

Используемые перечисления

#### `enum ContentType`

Типы контента, поддерживаемые сетевыми библиотеками без дополнительной настройки
Значения:

```
Json,
Xml
```


#### `enum Protocol`

Протокол уровня приложения. Значения:
```
Http,
Https   // С шифрованием данных
```

### Interfaces

Используемые интерфейсы

#### `interface IRequestProcessor`

Интерфейс обработчика `HTTP`-запросов

```
bool AuthorizationRequired { get; } // Требует ли вызов метода какой-либо авторизации
HttpMethod Method { get; }          // HTTP-метод запроса
string SubUri { get; }              // URI, на который должен быть отправлен запрос
```