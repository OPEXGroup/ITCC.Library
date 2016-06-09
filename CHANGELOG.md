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
