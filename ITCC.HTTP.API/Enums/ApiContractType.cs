using System;
using ITCC.HTTP.API.Extensions;

namespace ITCC.HTTP.API.Enums
{
    [Flags]
    public enum ApiContractType
    {
        /// <summary>
        ///     No explicit restrictions
        /// </summary>
        None,
        /// <summary>
        ///     Property MUST NOT be null
        /// </summary>
        NotNull = 1 << 0,
        /// <summary>
        ///     Property can be null
        /// </summary>
        CanBeNull = 1 << 1,
        /// <summary>
        ///     Property MUST be positive number
        /// </summary>
        PositiveNumber = 1 << 2,
        /// <summary>
        ///     Property MUST NOT be negative number
        /// </summary>
        NonNegativeNumber = 1 << 3,
        /// <summary>
        ///     Property MUST NOT be positive number
        /// </summary>
        NonPositiveNumber = 1 << 4,
        /// <summary>
        ///     Property MUST be negative number
        /// </summary>
        NegativeNumber = 1 << 5,
        /// <summary>
        ///     Property is != 0 number. MUST NOT be used with floating-point number
        /// </summary>
        NotZero = 1 << 6,
        /// <summary>
        ///     Property is even interger
        /// </summary>
        EvenNumber = 1 << 7,
        /// <summary>
        ///     Property is odd integer
        /// </summary>
        OddNumber = 1 << 8,
        /// <summary>
        ///     Property is in predefined enum values set. Flags ARE allowed. See <see cref="EnumExtensions"/>
        /// </summary>
        EnumValue = 1 << 9,
        /// <summary>
        ///     Property is in predefined enum values set. Flags ARE NOT allowed
        /// </summary>
        StrictEnumValue = 1 << 10,
        /// <summary>
        ///     Property is nonempty AND nonwhitespace string OR NULL
        /// </summary>
        NonWhitespaceString = 1 << 11,
        /// <summary>
        ///     Property is http(s) uri string OR NULL
        /// </summary>
        UriString = 1 << 12,
        /// <summary>
        ///     Property is guid-formatted string OR NULL
        /// </summary>
        GuidString = 1 << 13,
        /// <summary>
        ///     Property is non-empty collection OR NULL
        /// </summary>
        NotEmpty = 1 << 14,
        /// <summary>
        ///     Property is a collection that contains at least 2 elements OR NULL
        /// </summary>
        NotSingle = 1 << 15,
        /// <summary>
        ///     Property can be empty collection OR NULL
        /// </summary>
        CanBeEmpty = 1 << 16,
        /// <summary>
        ///     Property is a collection with not null elements OR NULL
        /// </summary>
        ItemsNotNull = 1 << 17,
        /// <summary>
        ///     Property is a collection with maybe null items OR NULL
        /// </summary>
        ItemsCanBeNull = 1 << 18,
        /// <summary>
        ///    Property is a collection of guid-like strings OR NULL
        /// </summary>
        ItemsGuidStrings = 1 << 19,


        /* These are common use cases */
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
    }
}
