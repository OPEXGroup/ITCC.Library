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

Классы для работы с геокодерами Яндекса и Google. Интересные классы:

* `ITCC.Geocoding.Geocoder.cs`

### ITCC.HTTP.Client

Реализация HTTP-клиентов. Интересные классы:

* `ITCC.HTTP.Client.RegularClient`  
* `ITCC.HTTP.Client.StaticClient`  

### ITCC.HTTP.Common

Маленькая общая часть реализации HTTP

### ITCC.HTTP.Server

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

Библиотека для генерации и установки TLS-сертификатов. 
Интегрирована в `ITCC.HTTP.Server`, но может использоваться и самостоятельно. 
Интересные классы:

* `ITCC.HTTP.SslConfigUtil.Core.Binder`  
* `ITCC.HTTP.SslConfigUtil.Core.CertificateController`  

### ITCC.HTTP.SslConfigUtil.GUI

WPF-приложение для генерации и установки TLS-сертификатов  

### ITCC.Logging.Core

Логгирование. Система подписчик-получатель с возможностью добавления получателей. Надо просто реализовать интерфейс `ILogReceiver`. **Собирается везде.** Интересные классы:

* `ITCC.Logging.Core.Logger`

### ITCC.Logging.Reader.Core

Библиотека для парсинга логов, генерируемых `ITCC.Logging.Core.Logger`  

### ITCC.Logging.Reader.WPF

Настольное приложения для отображения логов, генерируемых `ITCC.Logging.Core.Logger`  

### ITCC.Logging.Windows

Различные реализации логов для .Net 4.6. Интересные классы:

* `ITCC.Logging.Loggers.ColouredConsoleLogger`  
* `ITCC.Logging.Loggers.BufferedFileLogger`  
* `ITCC.Logging.Loggers.SystemEventLogger`  
* `ITCC.Logging.Loggers.EmailLogger`  

### ITCC.WPF

Маленькие полезные классы для работы с WPF. Включает, в том числе, работу с `ITCC.Logging` из WPF и специальное окошко `LogWindow` для вывода логов. Интересные классы:  

* `ITCC.WPF.Windows.LogWindow`  
* `ITCC.WPF.Utils.DataGridHelper`  
* `ITCC.WPF.Utils.ScrollViewerExtensions`  
* `ITCC.WPF.Utils.ObservableRingBuffe`  


## Подключение

### Клонирование

```
git clone git@gitlab.itcc.company:OPEXGroup/ITCC.Library.git
```

### Подключение в качестве подмодуля

```
git submodule add git@gitlab.itcc.company:OPEXGroup/ITCC.Library.git
```

Потом, для подключения после клонирования родительского проекта

```
git submodule init`
git submodule update
```

Собирается из Visual Studio 2015, целевая среда - `.Net 4.6 (x64)`.

### Замечания по сборке

#### Платформа

Библиотека писалась под Windows 10 и на Linux работать не будет

#### Prebuild

При сборке `ITCC.HTTP.Client`, `ITCC.HTTP.SslConfigUtils` и `ITCC.Geocoding` выполняются prebuild-скрипты для установки nuget-пакетов в каталоги проектов (Prebuild.ps1).
Для того, чтобы они могли выполниться, нужно в PowerShell выполнить

```
Set-ExecutionPolicy Unrestricted
```

После сборки имеет смысл снова сделать

```
Set-ExecutionPolicy Restricted
```

Как вариант, можно вручную скачать nuget и выполнить `nuget restore` для нужных проектов, указав их директорию packages в качестве выходной.

