# ITCC.HTTP.API

Библиотека для декларативного описания и автоматической проверки контрактов сетевого API.

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

#### class ApiContractAttribute : Attribute

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

#### class ApiViewAttribute : Attribute

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

#### class ApiViewCheckAttribute : Attribute

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

### Enums

Используемые перечисления.

#### ApiContractType

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

#### enum ApiHttpMethod

[Flags]

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

#### static class ListExtensions

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

### static class ObjectExtensions

Методы для объектов

```
bool IsApiView(this object maybeView);          // Проверка того, что объект является представлением API (помечен ApiViewAttribute)
bool IsApiViewList(this object maybeViewList);  // Проверка того, что объект является списком представлений API
ViewCheckResult CheckAsView(this object view);  // Проверка объекта (через ViewChecker)
```