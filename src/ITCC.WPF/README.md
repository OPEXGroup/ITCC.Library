## ITCC.WPF

[![NuGet version](https://badge.fury.io/nu/ITCC.WPF.svg)](https://badge.fury.io/nu/ITCC.WPF)

Библиотека классов (в том числе, окон) для использования в WPF-приложениях

### Behaviors

Расширения элементов управления

#### SecurePasswordBindingBehavior

Предоставляет возможность привязки `PasswordBox.SecurePassword` к свойству модели:

```
class ViewModel : INotifyPropertyChanged
{
	public SecureString Password { ... }
}
```

```
<PasswordBox>
	<i:Interaction.Behaviors>
		<SecurePasswordBindingBehavior Password="{Binding Password, Mode=TwoWay}" />
	</i:Interaction.Behaviors>
</PasswordBox>
```


### Credentials

Классы для работы с учетными данными

#### `class Credential`

Объект учетных данных пользователя. Свойства:

```
CredentialType CredentialType { get; }  // Тип учетных данных
string ApplicationName { get; }         // Имя приложения, к которому относятся учетные данные
string UserName { get; }                // Имя пользователя
SecureString Password { get; }          // Пароль/секрет
```

#### `class CredentialManager`

Класс для работы с учетными данными (чтение/сохранение). Создание:

```
CredentialManager(string applicationName);  // Имя приложения не может быть изменено после создания
```

Основные методы:

```
Credential ReadCredential();
int WriteCredential(string userName, string secret, CredentialPersistence credentialPersistence);
IReadOnlyList<Credential> EnumerateCrendentials();
```

### Enums

Используемые перечисления

#### `enum CredentialPersistence`

Тип сохранения учетных данных

```
Session = 1,      //
LocalMachine = 2, //
Enterprise = 3    //
```

#### `enum CredentialType`

Тип учетных данных

```
Generic = 1,                   //
DomainPassword,                //
DomainCertificate,             //
DomainVisiblePassword,         //
GenericCertificate,            //
DomainExtended,                //
Maximum,                       //
MaximumEx = Maximum + 1000,    //
```

### Loggers

Уже написанные варианты получателя лога для WPF-прриложений

#### `class MessageLogger : ILogReceiver`

Выводит лог через `MessageBox`'ы. Основной конструктор 
```
MessageLogger(LogLevel level);
```

### Utils

Разнообразные служебные классы

#### `class DataGridHelper`

Служебный класс для операций с `DataGrid`'ами.

Ключевые методы:

```
/*
  Используется для генерации заголовков. Следует вызывать в обработчиках AutoGeneratingColumn.
  Использует атрибуты DisplayName, HeaderTooltip, DataGridIgnore и DatagridColumnStyle у содержимого Grid'а
*/
static void HandleAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
```


#### `class EnumDescriptionTypeConverter`

Применяется для вывода описания элемента перечисления вместо его значения в графическом интерфейсе, при этом модификация XAML не требуется. Текст описания берется либо из значение в атрибуте `System.ComponentModel.DescriptionAttribute`, либо из файла ресурсов (при использовании `ITCC.WPF.Utils.LocalizedDescriptionAttribute`).

```
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum MyEnum
{
    [Description("Описание значения № 1")]
    Value1,
    [Description("Описание значения № 2")]
    Value2
}
```


#### `class LocalizedDescriptionAttribute`

Применяется для локализации перечислений в приложениях при помощи resx-файлов. При использовании необходимо указать ключ и тип ресурса.

```
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum MyEnum
{
    [LocalizedDescription("MyEnumValue1Description", typeof(ResourceType))]
    Value1,
    [LocalizedDescription("MyEnumValue2Description", typeof(ResourceType))]
    Value2
}
```

#### `class ScrollViewerExtensions`

Предоставляет дополнительные `DependencyProperty` для `ScrollViewer`. Дополнительные свойства:

```
bool AlwaysScrollToEnd; // Автоматическая прокрутка вниз при добавлении содержимого
```

#### `static class SecureStringHelper`

Класс для работы с `SecureString`. Основные методы:

```
string GetInsecureString(SecureString secureString);
SecureString GetSecureString(string insecureString);
int GetSecureStringLength(SecureString secureString);
bool IsMatch(SecureString secureString1, SecureString secureString2);
```

### Windows

Классы окон

#### `partial class LogWindow : Window`

Окно для отобрражения и настройки лога. Основной конструктор:

```
LogWindow(ObservableLogger logger)
```
