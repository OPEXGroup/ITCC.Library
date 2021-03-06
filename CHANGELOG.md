## 3.1.0 (2018-02-09)

* **New**:  Credential, CredentialType, CredentialPersistence, CredentialManager, SecureStringHelper and SecurePasswordBindingBehavior added to ITCC.WPF

## 3.0.2 (2017-10-22)

* **New**:  `MaxConcurrentRequests`, `MaxRequestQueue` and `ConfigurationViewEnabled` added to `HttpServerConfiguration`  
* **Edit**: Dependencies updated  

## 3.0.1 (2017-09-09)

* **New** : `DebugLogsEnabled` and `RequestTracingEnabled` added to `StaticServer` (can be turned on during startup via config and changed in runtime)  
* **Fix** : Do not throw in Logger::FlushAsync if email logger is registered but not started  
* **Fix** : Pass `checkDescription` to `ApiViewCheckAttribute` via `CallerMemberName`  
* **Fix** : Thread pool usage statistics fixed in `StaticServer`

## 3.0.0 (2017-04-05)

* **New**:  Library can now be built for NuGet deploy  
* **New**:  `ITCC.HTTP.API.Documentation` added for automatic docs generation  
* **New**:  `ITCC.HTTP.API` extended (a lot of attributes for method and view descriptions)  
* **Edit**: **BREAKING CHANGE**: solution layout changed (src/tests/build)  
* **Edit**: **BREAKING CHANGE**: .NET Standard projects upgraded to new .csproj format (requires Visual Studio 2017)  
* **Edit**: Requeired .NET Framework vesrion downgraded to 4.5  
* **Edit**: pre-build scripts removed  
* **Edit**: Dependencies updated  
* **Edit**: `ITCC.HTTP.Server.Core.RequestProcessor` now uses properties instead of fields and implements `IRequestProcessor`  
* **Edit**: Dependencies updated  

## 2.9.0 (2017-03-01)

* **New**:  `EnumInfoProvider` and `EnumValueInfoAttribute` added to `ITCC.HTTP.API`   

## 2.8.3 (2017-02-22)

* **New**:  `StaticServer` now ensures all file sections exist at startup  

## 2.8.2 (2017-02-17)

* **Fix**: `StaticServer::Start` error handlers fixed  
* **Fix**: `ServerStatistics` CPU counter fixed  

## 2.8.1 (2017-02-10)

* **New**:  `EnumDescriptionTypeConverter` and `LocalizedDescriptionAttribute` added to `ITCC.WPF`  
* **Fix**:  Request logging with files turned off fixed in `StaticServer`  

## 2.8.0 (2017-02-07)

* **Edit**: **BREAKING CHANGE**: ItemsFilter removed from submodules  
* **New**:  `HEAD` requests API for `RegularClient`/`StaticClient`  
* **New**:  `StaticServer` statistics now includes CPU usage. **Now for process, but machine total**  
* **Edit**: `BufferedFileLogger`/`PortableBufferedFileLogger` now have optional `maxQueueSize` constructor parameter  
* **Edit**: `RequestResult<TResult>::Headers` now include content headers (such as `Content-Length`)  
* **Edit**: `RegularClient`/`StaticClient` logs now include content headers (such as `Content-Length`)  

## 2.7.2 (2017-01-29)

* **Edit**: Newtonsoft.Json downgrade to 9.0.1  

## 2.7.1 (2017-01-27)

* **Edit**: Prebuild scripts should go to lib\ folder  

## 2.7.0 (2017-01-23)

* **New**:  `ITCC.UI` project added  
* **New**:  github pages enabled (https://opexgroup.github.io/ITCC.Library)  
* **Edit**: **BREAKING CHANGE**: A lot of classes moved from `ITCC.WPF` to `ITCC.UI`  

## 2.6.0 (2017-01-20)

* **Edit**: **BREAKING CHANGE**: `IFlushableLogReceiver` interface changed (`void Flush() => Task FlushAsync()`).  
* **Edit**: **BREAKING CHANGE**: `Logger` interface changed (`void FlushAll() => Task FlushAllAsync()`).  
* **Edit**: **BREAKING CHANGE**: `UiThreadRunner` replaced by `AsyncUiThreadRunner`.  
* **Edit**: **BREAKING CHANGE**: `ObservableLogger` now accepts `UiThreadRunner` instead of `AsyncUiThreadRunner`.  
* **New**:  Commands added to `ITCC.WPF`: `DelegateCommand`, `IAsyncCommand`, `AsyncCommand`, `AsyncCommand<TResult>`, `NotifyTaskCompletion<TResult>`, `AsyncCommandFactory`  

## 2.5.0 (2017-01-18)

* **Edit**: **BREAKING CHANGE**: `HttpServerConfiguration` now takes `IBodyEncoder` list instead of `BodyEncoder` list. Several implementations added.    
* **Edit**: `StaticServer` now requires prebuild script  
* **Edit**: `StaticServer` services refactor  
* **Edit**: `StaticServer` now adds list of supported content types with `406 Not Acceptable`  
* **Edit**: All nuget dependencies updated to last versions  
* **Fix**:  `StaticServer` now starts correctly if static files are disabled  

## 2.4.4 (2017-01-12)

* **Fix**:  Prevent files from multiple gzipping in `FilePreprocessorController`    
* **Fix**:  Unlock files correctly after processing in `FilePreprocessorThread`  

## 2.4.3 (2017-01-12)

* **Fix**:  `StaticClient` and `RegularClient` `Content-Type` fix for `POST` requests  

## 2.4.2 (2017-01-09)

* **Fix**:  `StaticClient` and `RegularClient` fix in case `TResponseSuccess` is raw string  

## 2.4.1 (2016-12-28)

* **New**:  `StaticServer` now supports background file gzipping  
* **Fix**:  Minor fixes after PVS-studio check  
* **Fix**:  Possible race conditions fixed in FilePreprocessController  

## 2.4.0 (2016-12-13)

* **New**:  `StaticServer` memory overhead warnings added  
* **New**:  `CriticalMemoryValue` and `MemoryAlarmStrategy` added to `HttpServerConfiguration`  

## 2.3.2 (2016-12-12)

* **Fix**:  `RegularClient::SerializeHttpRequestMessage` headers format fixed  

## 2.3.1 (2016-12-09)

* **Edit**: `Logger` portability improved  
* **Fix**:  Small docs fixes

## 2.3.0 (2016-11-30)

* **Edit**: **BREAKING CHANGE**: `RegularClient` and `StaticClient` API breaking changes  
* **Edit**: `Either<TFirst, TSecond>` does not accept nulls in costructors    
* **Edit**: Several minor fixes  

## 2.2.6 (2016-11-28)

* **New**:  `415 Unsupported Media Type` is now processed by `StaticServer`, `RegularClient` and `StaticClient`  
* **New**:  `ViewSetConflict` errors construction  
* **Edit**: `RequestProcessor::Method` now defaults to `GET`  

## 2.2.5 (2016-11-24)

* **Fix**:  Encoder selection fixed in `ResponseFactory`  

## 2.2.4 (2016-11-24)

* **Fix**:  Display generic types correctly in `ApiErrorView::ViewType`  

## 2.2.3 (2016-11-24)

* **New**:  `NonSerializableTypes` option added to `HttpServerConfiguration`

## 2.2.2 (2016-11-23)

* **New**:  Files HEAD requests implemented

## 2.2.1 (2016-11-23)

* **Edit**: **BREAKING CHANGE**: Loggers splitted into desktop and portable versions

## 2.2.0 (2016-11-23)

* **Edit**: **BREAKING CHANGE**: `*FileLogger`, `*ConsoleLogger`, `RotatingRequestLogger` moved to ITCC.Logging.Core
* **Edit**: `IDisposable` implemented in classes with timers  
* **Edit**: `ItemsFilter` updated

## 2.1.2 (2016-11-22)

* **Edit**: Build `ITCC.Logging.Core` and `ITCC.HTTP.API` against netstandard 1.3  
* **Edit**: Build all desktop projects against .Net 4.6

## 2.1.1 (2016-11-22)

* **New**:  API error tree generation added  
* **Edit**: Several docs fixes  

## 2.1.0 (2016-11-21)

* **New**:  `ITCC.HTTP.API` library added  
* **Edit**: Build library against .Net 4.5.2  
* **Edit**: `ITCC.Logging.Core` now can be used under Windows Phone 8.1  

## 2.0.2 (2016-11-19)

* **New**:  `RequestBodyLogLimit` added to `HttpServerConfiguration`  
* **Fix**:  Several encoder issues fixed (for multiple encoders in `StaticServer`)  

## 2.0.1 (2016-11-08)

* **New**:  `ITCC.HTTP.Server.Testing` util added  
* **Edit**: `406 Not Acceptable` is now processed in `StaticClient`  
* **Fix**:  Encoder selection and response `Content-Type` fixed (correct `Accept` parsing)  

## 2.0.0 (2016-11-02)

* **New**:  `StaticServer` now supports several body encoders  
* **Edit**: **BREAKING CHANGE**: `ITCC.UI` namespace renamed to `ITCC.WPF`  
* **Fix**:  POST requests handler fixed in `FileRequestController`  

## 2.0.0-rc7 (2016-10-24)

* **Fix**:  `SerializeHttpRequest` fixed  

## 2.0.0-rc6 (2016-10-19)

* **Fix**:  `Range:` headers parsing fix  
* **Edit**: Use larger copy buffers for large files (gives significant performance improvement)  

## 2.0.0-rc5 (2016-10-14)

* **Fix**:  Do not log binary request bodies on server  

## 2.0.0-rc4 (2016-10-11)

* **New**:  ThreadPool usage added to ServerStatistcs  
* **Edit**: Better statistics representation  

## 2.0.0-rc3 (2016-10-11)

* **Fix**:  Server debug build fixed

## 2.0.0-rc2 (2016-10-10)

* **New**:  Memory usage and internal error contexts are now displayed in server statistics  
* **New**:  `Logger` now supports output with conditional compilation  
* **Fix**:  `BufferedFileLogger` fixes: correct frequency value in all constructors + redundant line break removed  

## 2.0.0-rc1 (2016-10-05)

* **New**:  `ITCC.HTTP.SslConfigUtil` added: core API, WPF GUI, basic console  
* **New**:  Started to implement LogReader for structured log files reading  
* **New**:  `SslConfigUtil` integrated into `StaticServer`  
* **Edit**: `LogWindow` rendering improved drammatically  
* **Edit**: `Newtonsoft.Json` removed from `Server` dependencies  
* **Edit**: **BREAKING CHANGE**: `ITCC.HTTP` splitted  
* **Edit**: **BREAKING CHANGE**: `ITCC.UI` renamed to `ITCC.WPF`  
* **Edit**: `ItemsFilter` submodule added  

## 1.0.6 (2016-09-27)

* **Fix**: `LogWindow` large messages display fix  

## 1.0.5 (2016-09-27)

* **Edit**: `LogWindow` rendering optimized even more  

## 1.0.4 (2016-09-16)

* **Edit**: `LogWindow` rendering optimized (linked-list usage, internal buffer)  

## 1.0.3 (2016-09-15)

* **Edit**: Make URIs case-insensitive is `StaticServer`

## 1.0.2 (2016-09-12)

* **Fix**:  Request logging fixed in `StaticServer`.  
    * This way uses reflection and private field access. This causes possible lack of portability and backward/forward compatibility  
    * **Note**: this is a hack. Request logging will be used only if **`TRACE`** preprocessor constant is defined. It is not recommended for production usage for now  

## 1.0.1 (2016-09-08)

* **Fix**:  `StaticClient::PostRawAsync` fixed (recursion was wrong)

## 1.0.0 (2016-08-23)

* **New**:  Build configurations for different platforms (finally :) )  
* **Edit**: HTTP server core refactoring and simplification  
* **Fix**:  `ITCC.Geocoding` prebuild fixed  
* **Fix**:  HEAD requests work again  

## 1.0.0-rc2 (2016-08-09)

* **New**:  `ITCC.Logging.Core` extracted as portable class library.  
* **New**:  `StaticServer` files API improvements

## 1.0.0-rc1 (2016-08-08)

* **New**:  Griffin.Framework removed from dependencies  
* **Edit**: Docs splitted  
* **New**:  Server configuration breaking change  

## 0.18.0 (2016-07-28)

* **New**:  Critical information protection added for `StaticServer`, `RegularClient` and `StaticClient`  
* **New**:  `RotatingRequestLogger` added

## 0.17.1 (2016-07-28)

* **New**:  More info for server message sending  

## 0.17.0 (2016-07-27)

* **New**:  202, 206, 416, 503 codes processing added in `RegularClient` and `StaticClient`  
* **New**:  `Range` header processing for files added in `StaticServer`  
* **New**:  Image compression (on `POST` and in background) added  
* **New**:  `FilesPreprocessingEnabled`, `FilesPreprocessorThreads` and `ExistingFilesPreprocessingFrequency` options added to `HttpServerConfiguration`  
* **New**:  `TemporaryUnavailable` value added to `ServerResponseStatus` enumeration  
* **New**:  `uiThreadRunner` param recommendations added to `README.md` for `ObservableLogger`  

## 0.16.8 (2016-07-21)

* **Edit**: `Newtonsoft.Json` updated to 9.0.1  
* **Edit**: Set vertical text alignment to `Top` for text-wrapped DataGrid cells  

## 0.16.7 (2016-07-13)

* **Fix**:  Do not log binary files' data on receiving

## 0.16.6 (2016-07-08)

* **Edit**: Changing log level in `LogWindow` does not need confirmation  

## 0.16.5 (2016-07-08)

* **New**:  `DebugLogger` added to `ITCC.Logging`  
* **New**:  `Logger::Level` can be changed in `LogWindow` GUI 

## 0.16.4 (2016-07-07)

* **Fix**:  Fix in `ResponseFactory` 

## 0.16.3 (2016-07-07)

* **Fix**:  Response logging fix in `StaticServer` 

## 0.16.2 (2016-07-06)

* **Fix**:  Request logging fix in `StaticServer`  

## 0.16.1 (2016-07-06)

* **Edit**: Better log for servers with gzip enabled  
* **Fix**:  Authentification fix for servers with gzip enabled

## 0.16.0 (2016-07-04)

* **New**:  Full redirect (301, 302) support added in `StaticServer`, `StaticClient` and `RegularClient`  

## 0.15.1 (2016-07-04)

* **Fix**:  Decompression method selection fixed

## 0.15.0 (2016-07-04)

* **New**:  GZip compression is implemented and enabled by default in `StaticServer`, `StaticClient` and `RegularClient`  

## 0.14.1 (2016-07-01)

* **Fix**: `GetFileAsync`, `PutFileAsync` and `PostFileAsync` file sharing fix for `StaticClient` and `RegularClient`  

## 0.14.0 (2016-06-29)

* **New**: `GetFileAsync` and `PutFileAsync` added for `StaticClient` and `RegularClient`  
* **New**: `outputStream` param added for `RegularClient::PerformRequestAsync`  

## 0.13.0 (2016-06-29)

* **Edit**: `RequestProcessor::LegacyName` removed  

## 0.12.3 (2016-06-29)

* **Fix**:  `AuthentificationResult` constructor fix  

## 0.12.3 (2016-06-29)

* **Edit**: `AuthentificationResult` now has `AdditionalHeaders` instead of `Userdata`  

## 0.12.2 (2016-06-28)

* **Edit**: `DataGridHelper::HandleAutogeneratinColumn` does not use `lastColumnName`
* **New**:  `DataGridColumnStyleAttribute` added

## 0.12.1 (2016-06-27)

* **Fix**:  Google geocoder fix  

## 0.12.0 (2016-06-27)

* **New**:  Google maps API support  

## 0.11.1 (2016-06-27)

* **Fix**:  `Logger` made static  

## 0.11.0 (2016-06-27)

* **New**:  `IFlushableLogReceiver` interface added  
* **New**:  `Logger::FlushAll(void)` method added  
* **New**:  `BufferedRotatingFileLogger` added  
* **New**:  `EmailLogger` new implements `IFlushableLogReceiver`  

## 0.10.0 (2016-06-24)

* **New**:  `ITCC.Geocoding` project added  
* **New**:  `MaxFileSize` added to `FileSection`. `413 Request Entity Too Large` support added  
* **Edit**: General HTTP refactor (See `HandlerResult` and `RequestResult`)  

## 0.9.4 (2016-06-23)

* **New**:  `EmailLogger` will now report about it's start

## 0.9.3 (2016-06-23)

* **New**:  `MaxQueueSize` parameter added to `EmailLogger` configuration

## 0.9.2 (2016-06-23)

* **Edit**: More accurate time counters in `ServerStatistics`  
* **Fix**:  Fixed service requests performance measuring in `StaticServer`  

## 0.9.1 (2016-06-22)

* **New**:  `DataGridIgnoreAttribute` added

## 0.9.0 (2016-06-21)

* **New**:  Email logger added
* **New**:  `RequestMaxServeTime` added to `HttpServerConfiguration`

## 0.8.0 (2016-06-19)

* **New**:  ITCC.UI added

## 0.7.1 (2016-06-16)

* **Fix**:  Statistic fixed for file requests
* **Fix**:  Respond with 400 instead of 500 for bad file requests

## 0.7.0 (2016-06-15)

* **New**:  Server asynchronous stop supported  
* **New**:  Use reserved uris and methods set  
* **Edit**: More thorough server configuration check  

## 0.6.10 (2016-06-15)

* **Fix**:  Correct timeout/cancellation conditions in `RegularClient`  
* **Edit**: Enum element renamed from `RequestCanceled` to `RequestCancelled`
* **New**:  Test project added

## 0.6.9 (2016-06-15)

* **Fix**:  Log entries' time fixed  

## 0.6.8 (2016-06-09)

* **Fix**:  Statistics improved

## 0.6.7 (2016-06-09)

* **Fix**:  Use ling in processor selection

## 0.6.6 (2016-06-09)

* **Fix**:  `PostFileAsync` returns `ServerResponseStatus.Ok` on good codes
* **Edit**: `POST` for files returns `201 Created` instread of `200 OK`

## 0.6.5 (2016-06-09)

* **Fix**:  Request processor selection fixed

## 0.6.4 (2016-06-09)

* **Fix**:  Wrong `Method Not Allowed` condition fixed

## 0.6.3 (2016-06-07)

* **Edit**: Griffin.Framework updated

## 0.6.2 (2016-06-07)

* **Fix**:  ResponseFactory type initialization fixed

## 0.6.1 (2016-06-02)

* **New**: `Method Not Allowed` responses added
* **Edit**: Do not log full or partial response bodies if TRACE constant is not defined  

## 0.6.0 (2016-05-26)

* **New**:  `BodyEncoding`, `LogResponseBodies` and `ResponseBodyLogLimit` added to `HttpServerConfiguration`
* **New**:  Correct request timeout processing added to client (`ServerResponseStatus.RequestTimeout`) status
* **Edit**: Critical log colour changed in `ColouredConsoleLogger`
* **Edit**: Changelog format changed
* **Fix**:  Possible `Logger` concurrency issues fixed
* **Fix**:  Possible `ServerStatistics` concurrency issues fixed

## 0.5.1 (2016-05-25)

* **Edit**: Performans stats output enhanced
* **Edit**: Datetime format changed for `ITCC.Logging`

## 0.5.0 (2016-05-25)

* **New**:  Performance measuring added

## 0.4.2 (2016-05-24)

* **Fix**:  File sections fix

## 0.4.1 (2016-05-20)

* **Fix**:  Statistics authorization mechanism fix

## 0.4.0 (2016-05-20)

* **New**:  `SuitableSslProtocols` added to `HttpServerConfiguration`
* **New**:  Client SSL cipher suite information is avaible in logs (per connection) and in /statistics section

## 0.3.2 (2016-05-19)

* **New**:  `StatisticsAuthorizer` added to `HttpServerConfiguration`

## 0.3.1 (2016-05-18)

* **Edit**: GET for files uses actual content types (`MimeTypes` added)

## 0.3.0 (2016-05-18)

* **New**:  File sections with separate access rights added (`FileSections` added to `HttpServerConfiguration`)
* **Edit**: `FilesAuthorizer` signature changed (now it also takes requested `FileSection`)

## 0.2.3 (2016-05-18)

* **Edit**: Small statistics enhancement

## 0.2.2 (2016-05-15)

* **Fix**:  `IsFilesRequest` fixed for servers with files disabled

## 0.2.1 (2016-05-14)

* **Edit**: Better thread info in logger output

## 0.2.0 (2016-05-03)

* **New**:  Non-static HTTP client (`RegularClient`) added

## 0.1.2 (2016-05-03)

* **Fix**:  Correct prebuild

## 0.1.1 (2016-05-03)

* **Fix**:  Prebuild added (correct submodule build)

## 0.1.0 (2016-05-01)

* **New**:  Library created (HTTP and Logging modules)
