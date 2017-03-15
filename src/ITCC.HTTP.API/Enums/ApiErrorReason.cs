// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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
        None,
        /// <summary>
        ///     Expected view type cannot be deserialized
        /// </summary>
        BadDatatype,
        /// <summary>
        ///     Simple property contract violation
        /// </summary>
        ViewPropertyContractViolation,
        /// <summary>
        ///     Method with <see cref="Attributes.ApiViewCheckAttribute"/> failed for view
        /// </summary>
        ViewContractViolation,
        /// <summary>
        ///     Several objects have been received but they contain internal contradictions
        /// </summary>
        ViewSetConflict,
        /// <summary>
        ///     One or more query params are missing or invalid. Request cannot be processed
        /// </summary>
        QueryParameterError,
        /// <summary>
        ///     One or more query params are missing or invalid. Request handler cannot be selected
        /// </summary>
        QueryParameterAmbiguous,
        /// <summary>
        ///     Received view contains bad foreign key 
        /// </summary>
        ForeignKeyError,
        /// <summary>
        ///     Dataset contains complex business logic contradictions
        /// </summary>
        BusinessLogicError,
        /// <summary>
        ///     Used for non-leaf error nodes
        /// </summary>
        InnerErrors,
        /// <summary>
        ///     All other reasons
        /// </summary>
        Unspecified
    }
}
