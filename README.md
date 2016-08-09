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

### ITCC.HTTP

Работа с HTTP (клиент с сервер). Настройка TLS на сервере описана [здесь](http://stackoverflow.com/questions/11403333/httplistener-with-https-support). Интересные классы:

* `ITCC.HTTP.Server.StaticServer<TAccount>`  
* `ITCC.HTTP.Server.HttpServerConfiguration`  
* `ITCC.HTTP.Client.StaticClient`  

### ITCC.Logging.Core

Логгирование. Система подписчик-получатель с возможностью добавления получателей. Надо просто реализовать интерфейс `ILogReceiver`. **Собирается везде.** Интересные классы:

* `ITCC.Logging.Core.Logger`

### ITCC.Logging.Windows

Различные реализации логов для .Net 4.6. Интересные классы:

* `ITCC.Logging.Loggers.ColouredConsoleLogger`  
* `ITCC.Logging.Loggers.BufferedFileLogger`  
* `ITCC.Logging.Loggers.SystemEventLogger`  
* `ITCC.Logging.Loggers.EmailLogger`  

### ITCC.UI

Маленькие полезные классы для работы с WPF. Включает, в том числе, работу с `ITCC.Logging` из WPF и специальное окошко `LogWindow` для вывода логов. Интересные классы:  

* `ITCC.UI.Windows.LogWindow`  
* `ITCC.UI.Utils.DataGridHelper`  
* `ITCC.UI.Utils.ScrollViewerExtensions`  

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

При сборке `ITCC.HTTP` и `ITCC.Geocoding` выполняются prebuild-скрипты для установки nuget-пакетов в каталоги проектов (Prebuild.ps1).
Для того, чтобы они могливыполниться, нужно в PowerShell выполнить

```
Set-ExecutionPolicy Unrestricted
```

После сборки имеет смысл снова сделать

```
Set-ExecutionPolicy Restricted
```

Как вариант, можно вручную скачать nuget и выполнить `nuget restore` для нужных проектов, указав их директорию packages в качестве выходной.

