# ITCC.HTTP.API

[![NuGet version](https://badge.fury.io/nu/ITCC.HTTP.API.svg)](https://badge.fury.io/nu/ITCC.HTTP.API)

**Компонент работает в .Net 4.6, .Net Core, ASP.NET Core, Xamarin**  
Библиотека для декларативного описания и автоматической проверки контрактов сетевого API.
Изначально предназначена для работы с HTTP.

### Корневые

#### `static class ViewChecker`

Средство проверки контрактов API. Основные методы:

```
/*
 * Проверка соблюденя объектом контракта API
 * view ДОЛЖЕН быть помечен атрибутом ITCC.HTTP.API.Attributes.ApiViewAttribute
 *  (в противном случае будет выброшено InvalidOperationException)
 */
ViewCheckResult CheckContract(object view);
```

### Attributes

Атрибуты для маркировки классов, структур, их методов и свойств
для последующего использования при автоматической проверке контрактов API.

#### `class ApiContractAttribute : Attribute`

`[AttributeUsage(AttributeTargets.Property)]`

Атрибут, применяемый к свойствам для обозначения того, что они должны придерживаться определенного контракта.
Виды контрактов описаны в `ITCC.HTTP.API.Enums.ApiContractType`. Конструктор:

```
ApiContractAttribute(ApiContractType type, string comment = null);
```

Свойства:

```
ApiContractType Type { get; }
string Comment { get; }
```

#### `class ApiViewAttribute : Attribute`

`[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]`.

Атрибут, помечающий класс или структуру как предназначенную для API (представление данных).
Конструктор:

```
ApiViewAttribute(ApiHttpMethod httpMethod);
```

Свойства:

```
ApiHttpMethod HttpMethod { get; }
```

#### `class ApiViewCheckAttribute : Attribute`

`[AttributeUsage(AttributeTargets.Method)]`

Атрибут, помечающий instance-метод как проверку контракта API.
Метод при это должет иметь тип `Func<bool>`. Он будет вызван автоматически при проверке контракта.
Если метод вернул `false`, его имя будет использовано в качестве сообщения об ошибке.
Конструктор:

```
ApiViewCheckAttribute([CallerMemberName] string errorDescription = null);
```

Свойства:

```
string ErrorDescription { get; }
```

#### `class EnumValueInfoAttribute : Attribute`

`[AttributeUsage(AttributeTargets.Field)]`

Атрибут, добавляющий к полю имя и (опционально) описание. Применяется к элементам перечислений, в т.ч. помеченных атрибутом `Flags`.

Конструктор:

```
EnumValueInfoAttribute(string displayName, string description = null);
```

Свойства:

```
string DisplayName { get; }
string Description { get; }
```

### Enums

Используемые перечисления.

#### `enum ApiContractType`

`[Flags]`

Тип контракта API, наложенного на свойство. Основные флаги:

```
None = 0,                           // Никаких условий

// Проверки на null
NotNull = 1 << 0,                   // Свойство не равно null (только для ссылочных типов)
CanBeNull = 1 << 1,                 // Свойство может быть равно null (не вызывает явной проверки, служит явной пометкой)

// Числовые проверки
PositiveNumber = 1 << 2,            // Свойство - положительное число (целое или вещественное)
NonNegativeNumber = 1 << 3,         // Свойство - неотрицательное число (целое или вещественное)
NonPositiveNumber = 1 << 4,         // Свойство - неположительное число (целое или вещественное)
NegativeNumber = 1 << 5,            // Свойство - отрицательное число (целое или вещественное)
NotZero = 1 << 6,                   // Свойство - число, отличное от 0 (целое, НЕ вещественное)
EvenNumber = 1 << 7,                // Свойство - чётное число (целое)
OddNumber = 1 << 8,                 // Свойство - нечётное число (целое)

// Проверки перечислений
EnumValue = 1 << 9,                 // Свойство лежит в пределах значений перечисления (с учетом флагов)
StrictEnumValue = 1 << 10,          // Свойство лежит в пределах значений перечисления (БЕЗ флагов)

// Проверки строк
NonWhitespaceString = 1 << 11,      // Свойство - строка, состоящая не только из пробельных символов (ИЛИ null)
UriString = 1 << 12,                // Свойство - строка, предстваляющая корректный HTTP(S) URI (ИЛИ null)  
GuidString = 1 << 13,               // Свойство - строка, предстваляющая guid в формате aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa (ИЛИ null)  

// Проверки списков (IList)
NotEmpty = 1 << 14,                 // Коллекция содержит > 0 элементов (ИЛИ равна null)
NotSingle = 1 << 15,                // Коллекция содержит > 1 элемента (ИЛИ равна null)
CanBeEmpty = 1 << 16,               // Коллекция может быть пустой (не вызывает явной проверки, служит явной пометкой)

// Проверки элементов списков
ItemsNotNull = 1 << 17,             // Элементы списка не равны null
ItemsCanBeNull = 1 << 18,           // Элементы списка могут быть равны null (не вызывает явной проверки, служит явной пометкой)
ItemsGuidStrings = 1 << 19,         // Элементы списка - guid-строки в формате aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa (ИЛИ null)  
```

Составные типы проверок:

```
NullOrNonWhitespaceString = CanBeNull | NonWhitespaceString,
NotNullNonWhitespaceString = NotNull | NonWhitespaceString,
NullOrUriString = CanBeNull | UriString,
NotNullUriString = NotNull | UriString,
NullOrGuidString = CanBeNull | GuidString,
NotNullGuidString = NotNull | GuidString,
NotNullGuidList = NotNull | CanBeEmpty | ItemsNotNull | ItemsGuidStrings,
NullOrGuidList = CanBeNull | CanBeEmpty | ItemsNotNull | ItemsGuidStrings,
NotNullNonEmptyGuidList = NotNull | NotEmpty | ItemsNotNull | ItemsGuidStrings,
NullOrNonEmptyList = CanBeNull | NotEmpty | ItemsNotNull,
NotNullNonEmptyList = NotNull | NotEmpty | ItemsNotNull,
NullOrNonEmptyGuidList = CanBeNull | NotEmpty | ItemsNotNull | ItemsGuidStrings,
NotNullNonSingleGuidList = NotNull | NotSingle | ItemsNotNull | ItemsGuidStrings,
NullOrNonSingleGuidList = CanBeNull | NotSingle | ItemsNotNull | ItemsGuidStrings
```

#### `enum ApiErrorReason`

Типы ошибок представлений данных. Значения:

```
None,                           // Ошибок нет. Не рекомендуется к использованию
BadDatatype,                    // Неверный тип данных
ViewPropertyContractViolation,  // Свойства полученных объектов имеют неверные значения
ViewContractViolation,          // Один или более метод проверки представлений вернул false
ViewSetConflict,                // Полученный набор данных содержит внутренние противоречия
QueryParameterError,            // Параметры HTTP-запроса отсутствуют или содержат неверные значения
QueryParameterAmbiguous,        // Метод обработки запроса не может быть выбран на основании параметров HTTP-запроса
ForeignKeyError,                // Ошибка связей. Присланные данные ссылаются на недопустимые или несуществующие объекты
BusinessLogicError,             // Нарушение бизнес-логики приложения
InnerErrors,                    // Тип для нелистовых вершин дерева ошибок
Unspecified                     // Неопределенный тип ошибки (неподходящий ни к чему вышеописанному)
```

#### `enum ApiHttpMethod`

`[Flags]`

Используется в `ITCC.HTTP.API.Attributes.ApiViewAttribute` для указания методы, в котором используется представление.
Значения:

```
Default = 0,        // Значение по умолчанию. Не рекомендуется к использованию
Get = 1 << 0,       // HTTP GET
Post = 1 << 1,      // HTTP POST
Put = 1 << 2,       // HTTP PUT
Delete = 1 << 3,    // HTTP DELETE
Patch = 1 << 4,     // HTTP PATCH
```

### Extensions

Методы расширения, используемые в библиотеке (и могут быть использованы для организации собственных проверок)

#### `static class ListExtensions`

Методы для списков

```
/*
 * Получение всех подмножеств элементов списка. Типизированная версия. Данный метод:
 * 1) Сохраняет порядок элеметов: если A раньше B в исходном списке,
 *    это же верно для всякого подмножества, содержащего A и B
 * 2) Работает как генератор (через yield). Соответственно, эффективное использование - через foreach
 */
IEnumerable<List<T>> GetSubsets<T>(this IList<T> list, bool includeEmpty = true);
/*
 * Получение всех подмножеств элементов списка. Типизированная версия.
 * Вышеописанные свойства выполняются.
 */
IEnumerable<List<object>> GetSubsets(this IList list, bool includeEmpty = true);
```

#### `static class ObjectExtensions`

Методы для объектов

```
bool IsApiView(this object maybeView);          // Проверка того, что объект является представлением API (помечен ApiViewAttribute)
bool IsApiViewList(this object maybeViewList);  // Проверка того, что объект является списком представлений API
ViewCheckResult CheckAsView(this object view);  // Проверка объекта (через ViewChecker)
```

### Utils

Служебные классы

#### `class ApiErrorOr<T> : Either<ApiErrorView, T> where T : class`

Реализация монады `ITCC.HTTP.API.Utils.Either` для случая `TFirst == ApiErrorView`. Свойства:

```
ApiErrorView ErrorView { get; } // Представление ошибки при ошибке (First)
T Data { get; }                 // Представление данных при успехе (Second)
bool IsError { get; }           // ErrorView != null
bool IsSuccess { get; }         // Data != null
```

Создание:

```
static ApiErrorOr<T> Error(ApiErrorView errorView); // Создание экземпляра с ошибкой
static ApiErrorOr<T> Success(T data);               // Создание экземпляра с данными
```

#### `class ApiErrorView`

Представление ошибки API для передаче в теле ответа (с кодом 4xx).
Имеет перегруженный метод `ToString()`, выдающий человекочитаемое описание дерева ошибок.
**Конструировать такие объекты стоит только через `ApiErrorViewFactory`.**
Свойства:

```
ApiErrorReason Reason { get; set; }             // Тип ошибки
ApiContractType ViolatedContract { get; set; }  // Нарушенный контракт. Отличен от None только при Reason == ViewPropertyContractViolation
string Context { get; set; }                    // Контекст возникновения ошибки
string ViewType { get; set; }                   // Тип представления, нарушевшего контракт
string ErrorMessage { get; set; }               // Человекочитаемое сообщение об ошибке
List<ApiErrorView> InnerErrorList { get; set; } // Список дочерних ошибок. Отличен от null только при Reason == InnerErrors
```

#### `static class ApiErrorViewFactory`

Служит для создания объектов типа `ApiErrorView`. **Подробно все методы описаны в исходниках.** Методы:

```
ApiErrorView None();
ApiErrorView ViewPropertyContractViolation(object view,
                                           string propertyName,
                                           ApiContractType violatedContract);
ApiErrorView ViewContractViolation(object view, string violatedContractName);
ApiErrorView QueryParameterError(string parameterName);
ApiErrorView QueryParameterAmbiguous(IEnumerable<string> availableParamSets);
ApiErrorView ForeignKeyError(Type viewType, string keyName);
ApiErrorView BusinessLogicError(string errorMessage);
ApiErrorView InnerErrors(object view, List<ApiErrorView> innerErrorViews);
ApiErrorView Unspecified(string errorMessage = null);
```

#### `class Either<TFirst, TSecond> where TFirst : class where TSecond : class`

Представляет монаду `Either`. Содержит либо значение типа `TFirst`, либо значение типа `TSecond`.
Конструкторы: 

```
Either(TFirst first);   // Создание с First != null
Either(TSecond second); // Создание с Second != null
```

Свойства:

```
TFirst First { get; }
TSecond Second { get; }

bool HasFirst => First != null;
bool HasSecond => Second != null;
```

#### `static class EnumHelper`

Служит для преобразований (тип контракта) <=> (описание ошибки). Методы:

```
string ApiContractTypeName(ApiContractType apiContractType);
ApiContractType ApiContractTypeByName(string name);
```

#### `static class EnumInfoProvider`

Служит для локализации элементов перечислений, использующих атрибут `EnumValueInfoAttribute`.

Методы:

```
/*
Получение имени для перечисления, при использовании 
неверного типа или отсутствии атрибутов возвращает `null`
*/
string GetElementName(object element);

/*
Получение описание для перечисления, при использовании 
неверного типа или отсутствии атрибутов возвращает `null`
*/
string GetElementDescription(object element);

/*
Получение значения перечисления по строке, полученной методом 
GetElementName, при использовании неверного значения или отсутствии
атрибутов возвращает null
*/
object GetEnumElementByName(string name, Type enumType)

/*
Обобщенная версия метода GetEnumElementByName(string name, Type enumType),
при использовании неверного значения или отсутствии атрибутов 
возвращает default(TEnum)
*/
TEnum GetEnumElementByName<TEnum>(string name);

/*
Получение словаря, в котором ключамя являются элементы перечислений типа
enumType, а значениями - добавленные к ним атрибуты EnumValueInfoAttribute,
неверного типа или отсутствии атрибутов возвращает `null`
*/
Dictionary<object, EnumValueInfoAttribute> GetInfoDictionaty(Type enumType);

/*
Обобщенная версия метода
Dictionary<object, EnumValueInfoAttribute> GetInfoDictionaty(Type enumType);
*/
Dictionary<TEnum, EnumValueInfoAttribute> GetInfoDictionaty<TEnum>();

/*
Производит разделение перечисления, тип которого помечен атрибутом Flags на 
отдельные составляющие.
*/
IEnumerable<TEnum> SplitEnum<TEnum>(TEnum value);
```

#### `class ViewCheckResult`

Представление результата проверки. Свойства:

```
bool IsCorrect { get; }             // Пройдена ли проверка
ApiErrorView ApiErrorView { get; }  // Представление ошибки в виде объекта
string ErrorDescription { get; }    // Представление ошибки в виде строки
```
