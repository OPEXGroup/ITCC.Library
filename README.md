# ITCC Library

## Общее описание

Библиотека общего назначения (на самом деле, несколько библиотек) для решения следующих задач:

* Быстрое написание серверов для приложений (быстрого написания, а не серверов :) )  
* Простая работа с клиентской частью HTTP  
* Простая работа с геокодерами  
* Красивые логи  
* Упрощение некоторых операций при работе с WPF  

## Компоненты

Папки, название которых не заканчивается на "Testing", являются компонентами библиотеки.
Описание интерфейсов библиотек лежит в README в папках.
Везде в описаниях опущен модификатор `public`, поскольку вещи, не входящие в интерфейс, явно не описываются.

### ITCC.Geocoding

[![NuGet version](https://badge.fury.io/nu/ITCC.Geocoding.svg)](https://badge.fury.io/nu/ITCC.Geocoding)

Классы для работы с геокодерами Яндекса и Google. Интересные классы:

* `ITCC.Geocoding.Geocoder.cs`

### ITCC.HTTP.API

[![NuGet version](https://badge.fury.io/nu/ITCC.HTTP.API.svg)](https://badge.fury.io/nu/ITCC.HTTP.API)

Библиотека для декларативного описания и автоматической проверки контрактов сетевого API.
Изначально предназначена для работы с HTTP.
Интересные классы:

* `ITCC.HTTP.API.ViewChecker`  
* `ITCC.HTTP.API.Attributes.ApiViewAttribute`  
* `ITCC.HTTP.API.Attributes.ApiContractAttribute`
* `ITCC.HTTP.API.Utils.Either<TFirst, TSecond>`

### ITCC.HTTP.API.Documentation

[![NuGet version](https://badge.fury.io/nu/ITCC.HTTP.API.Documentation.svg)](https://badge.fury.io/nu/ITCC.HTTP.API.Documentation)

Библиотека для автоматической генерации документации на основе аннотаций методов API из ITCC.HTTP.API

### ITCC.HTTP.Client

[![NuGet version](https://badge.fury.io/nu/ITCC.HTTP.Client.svg)](https://badge.fury.io/nu/ITCC.HTTP.Client)

Реализация HTTP-клиентов. Интересные классы:

* `ITCC.HTTP.Client.RegularClient`  
* `ITCC.HTTP.Client.StaticClient`  

### ITCC.HTTP.Common

[![NuGet version](https://badge.fury.io/nu/ITCC.HTTP.Common.svg)](https://badge.fury.io/nu/ITCC.HTTP.Common)

Маленькая общая часть реализации HTTP

### ITCC.HTTP.Server

[![NuGet version](https://badge.fury.io/nu/ITCC.HTTP.Server.svg)](https://badge.fury.io/nu/ITCC.HTTP.Server)

HTTP-сервер для быстрого развертывания приложений. 
Поддерживает дополнительную функциональность (работа со статичными файлами, предобработка контента).
Интересные классы:

* `ITCC.HTTP.Server.Core.StaticServer<TAccount>`  
* `ITCC.HTTP.Server.Core.HttpServerConfiguration` 

### ITCC.HTTP.Server.Testing

Пример запуска сервера для работы со статичными файлами. Поддерживает

* Произвольные директории со статикой  
* Настройка порта  
* Настройка количества рабочих потоков  
* Настройка уровня лога и вида (консоль/файлы)  
* HTTP/HTTPS (сертификат создается)  
* gzip  
* Препроцессинг изображений  
* statistics  
* ping (json/xml)  

### ITCC.HTTP.SslConfigUtil.Console

Консольное приложение для генерации и установки TLS-сертификатов  

### ITCC.HTTP.SslConfigUtil.Core

[![NuGet version](https://badge.fury.io/nu/ITCC.HTTP.SslConfigUtil.Core.svg)](https://badge.fury.io/nu/ITCC.HTTP.SslConfigUtil.Core)

Библиотека для генерации и установки TLS-сертификатов. 
Интегрирована в `ITCC.HTTP.Server`, но может использоваться и самостоятельно. 
Интересные классы:

* `ITCC.HTTP.SslConfigUtil.Core.Binder`  
* `ITCC.HTTP.SslConfigUtil.Core.CertificateController`  

### ITCC.HTTP.SslConfigUtil.GUI

WPF-приложение для генерации и установки TLS-сертификатов  

### ITCC.Logging.Core

[![NuGet version](https://badge.fury.io/nu/ITCC.Logging.Core.svg)](https://badge.fury.io/nu/ITCC.Logging.Core)

Логгирование. Система подписчик-получатель с возможностью добавления получателей. Надо просто реализовать интерфейс `ILogReceiver`. **Собирается везде.** Интересные классы:

* `ITCC.Logging.Core.Logger`

### ITCC.Logging.Reader.Core

Библиотека для парсинга логов, генерируемых `ITCC.Logging.Core.Logger`  

### ITCC.Logging.Reader.WPF

Настольное приложения для отображения логов, генерируемых `ITCC.Logging.Core.Logger`  

### ITCC.Logging.Windows

[![NuGet version](https://badge.fury.io/nu/ITCC.Logging.Windows.svg)](https://badge.fury.io/nu/ITCC.Logging.Windows)

Различные реализации логов для .Net 4.6. Интересные классы:

* `ITCC.Logging.Loggers.ColouredConsoleLogger`  
* `ITCC.Logging.Loggers.BufferedFileLogger`  
* `ITCC.Logging.Loggers.SystemEventLogger`  
* `ITCC.Logging.Loggers.EmailLogger`  

### ITCC.UI

[![NuGet version](https://badge.fury.io/nu/ITCC.UI.svg)](https://badge.fury.io/nu/ITCC.UI)

Библиотека классов для использования в GUI-приложениях на WPF, UWP и Xamarin Forms. Интересные классы:

* `ITCC.UI.Commands.AsyncCommand`  
* `ITCC.UI.Loggers.ObservableLogger`  
* `ITCC.UI.Utils.ObservableRingBuffer`  

### ITCC.WPF

[![NuGet version](https://badge.fury.io/nu/ITCC.WPF.svg)](https://badge.fury.io/nu/ITCC.WPF)

Маленькие полезные классы для работы с WPF. Включает, в том числе, работу с `ITCC.Logging` из WPF и специальное окошко `LogWindow` для вывода логов. Интересные классы:  

* `ITCC.WPF.Credentials.CredentialManager`  
* `ITCC.WPF.Windows.LogWindow`  
* `ITCC.WPF.Utils.DataGridHelper`  
* `ITCC.WPF.Utils.ScrollViewerExtensions`  

## Подключение

### Клонирование

```
git clone https://github.com/OPEXGroup/ITCC.Library.git
```

### Подключение в качестве подмодуля

```
git submodule add https://github.com/OPEXGroup/ITCC.Library.git
```

Потом, для подключения после клонирования родительского проекта

```
git submodule init
git submodule update
```
### Замечания по сборке

Собирается из Visual Studio 2017. Целевая среда:

* `.Net 4.5` для всех  
* `.Net 4.6` и `netstandard 1.3` для проектов `ITCC.Logging.Core`, `ITCC.HTTP.Common` `ITCC.HTTP.API` и `ITCC.UI`  
