---
permalink: /ITCC.Geocoding/
---

# ITCC.Geocoding

Библиотека небольших классов, полезных при работе с геокодерами

### Корневые

#### `static class Geocoder`

Геокодер для работы с различными провайдерами данных. Ключевые методы:

```
async Task<Point> GeocodeAsync(string location, GeocodingApi apiType);
void SetApiKey(string key, GeocodingApi apiType);
```

### Enums

Перечисления

#### `enum GeocodingApi`

Провайдер данных

### Utils

Служебные классы

#### `class Point`

Простое представление точки на проскости. Основные свойства:

```
double Latitude { get; set; }
double Longitude { get; set; }
```

### Yandex

Работа с API Яндекса

#### `enum Enums.KindType`

Тип объекта

#### `enum Enums.LangType`

Язык запроса (и ответа)

#### `struct GeoBound`

Представляет границу прямоугольной области. Поля:

```
GeoPoint LowerCorner; // Левый нижний угол
GeoPoint UpperCorner; // Правый верхний угол
```

#### `class GeoMetaData`

Метаданные геокодера. Поля:

```
KindType Kind = KindType.Locality;  // Тип геообъекта
string Text = string.Empty;         // Текст
```

#### `class GeoObject`

Географический объект. Свойства:

```
GeoPoint Point { get; set; }                // Точка (для точечных объектов)
GeoBound BoundedBy { get; set; }            // Граница (для объектов с размером)
GeoMetaData GeocoderMetaData { get; set; }  // Метаданные геокодера
```

#### `class GeoObjectCollection : List<GeoObject>`

Коллекция географических объектов. Конструктор:

```
GeoObjectCollection(string xml); // xml - ответа геокодера Яндекса
```

#### `struct GeoPoint`

Точка. Поля:

```
public double Latitude;   // Широта
public double Longitude;  // Долгота
```

#### `struct SearchArea`

Зоня поиска. Поля:

```
GeoPoint LongLat;   // Центр
GeoPoint Spread;    // Разброс по широте и долготе
```

#### `static class YandexGeocoder`

Основной класс проекта. Работа с API Яндека. Основные свойства:

```
string Key { get; set; } = string.Empty; // Ключ API
```

Основные методы:

```
async Task<GeoObjectCollection> GeocodeAsync(string location, short results = DefaultResultCount, LangType lang = LangType.en_US); // DefaultResultCount == 10
async Task<GeoObjectCollection> GeocodeAsync(string location, short results, LangType lang, SearchArea searchArea, bool rspn = false);
```