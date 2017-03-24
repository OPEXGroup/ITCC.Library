// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.HTTP.API.Attributes;

namespace ITCC.HTTP.API.Enums
{
    /// <summary>
    ///     Reasons server use to decline requests
    /// </summary>
    public enum ApiErrorReason
    {
        /// <summary>
        ///     Reserved value, should not be used for errors. Indicates everything is ok
        /// </summary>
        [EnumValueInfo(@"None", @"Reserved value, should not be used for errors. Indicates everything is ok")]
        None,
        /// <summary>
        ///    Expected view type cannot be deserialized 
        /// </summary>
        [EnumValueInfo(@"BadDatatype", @"Expected view type cannot be deserialized")]
        BadDatatype,
        /// <summary>
        ///     Simple property contract violation
        /// </summary>
        [EnumValueInfo(@"ViewPropertyContractViolation", @"Simple property contract violation")]
        ViewPropertyContractViolation,
        /// <summary>
        ///     Method with <see cref="ApiViewCheckAttribute"/> failed for view
        /// </summary>
        [EnumValueInfo(@"ViewContractViolation", @"Method with ApiViewCheckAttribute failed for view")]
        ViewContractViolation,
        /// <summary>
        ///     Several objects have been received but they contain internal contradictions
        /// </summary>
        [EnumValueInfo(@"ViewSetConflict", @"Several objects have been received but they contain internal contradictions")]
        ViewSetConflict,
        /// <summary>
        ///     One or more query params are missing or invalid. Request cannot be processed
        /// </summary>
        [EnumValueInfo(@"QueryParameterError", @"One or more query params are missing or invalid. Request cannot be processed")]
        QueryParameterError,
        /// <summary>
        ///     One or more query params are missing or invalid. Request handler cannot be selected
        /// </summary>
        [EnumValueInfo(@"QueryParameterAmbiguous", @"One or more query params are missing or invalid. Request handler cannot be selected")]
        QueryParameterAmbiguous,
        /// <summary>
        ///     Received view contains bad foreign key 
        /// </summary>
        [EnumValueInfo(@"ForeignKeyError", @"Received view contains bad foreign key")]
        ForeignKeyError,
        /// <summary>
        ///     Dataset contains complex business logic contradictions
        /// </summary>
        [EnumValueInfo(@"BusinessLogicError", @"Dataset contains complex business logic contradictions")]
        BusinessLogicError,
        /// <summary>
        ///     Used for non-leaf error nodes
        /// </summary>
        [EnumValueInfo(@"InnerErrors", @"Used for non-leaf error nodes")]
        InnerErrors,
        /// <summary>
        ///     All other reasons
        /// </summary>
        [EnumValueInfo(@"Unspecified", @"All other reasons")]
        Unspecified
    }
}
