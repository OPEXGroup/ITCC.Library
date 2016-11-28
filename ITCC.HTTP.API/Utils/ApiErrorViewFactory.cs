using System;
using System.Collections.Generic;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.API.Utils
{
    /// <summary>
    ///     Used to construct <see cref="ApiErrorView"/> instances properly (it is not recommended to create them by hand)
    /// </summary>
    public static class ApiErrorViewFactory
    {
        #region construction

        /// <summary>
        ///     Constructs ApiErrorView with <see cref="ApiErrorReason.None"/> reason.
        ///     Means everything is ok.
        /// </summary>
        /// <returns>Constructed ApiErrorView</returns>
        public static ApiErrorView None() => new ApiErrorView
        {
            Reason = ApiErrorReason.None
        };

        /// <summary>
        ///     Constructs ApiErrorView with <see cref="ApiErrorReason.BadDatatype"/> reason.
        ///     Means received data type is wrong.
        /// </summary>
        /// <param name="expectedType">Type that was correct</param>
        /// <returns>Constructed ApiErrorView</returns>
        public static ApiErrorView BadDatatype(Type expectedType) => new ApiErrorView
        {
            Reason = ApiErrorReason.BadDatatype,
            ViewType = GetTypeName(expectedType),
            ErrorMessage = "Failed to deserialize request data"
        };

        /// <summary>
        ///     Constructs ApiErrorView with <see cref="ApiErrorReason.ViewPropertyContractViolation"/> reason.
        ///     Means one or several view;s properties have invalid values.
        /// </summary>
        /// <param name="view">View that violated contract</param>
        /// <param name="propertyName">Malformed property name</param>
        /// <param name="violatedContract">Violated contract type (simple property contract)</param>
        /// <returns>Constructed ApiErrorView</returns>
        public static ApiErrorView ViewPropertyContractViolation(object view, string propertyName, ApiContractType violatedContract) => new ApiErrorView
        {
            Reason = ApiErrorReason.ViewPropertyContractViolation,
            Context = propertyName,
            ViewType = GetTypeName(view.GetType()),
            ErrorMessage = EnumHelper.ApiContractTypeName(violatedContract),
            ViolatedContract = violatedContract
        };

        /// <summary>
        ///     Constructs ApiErrorView with <see cref="ApiErrorReason.ViewContractViolation"/> reason.
        ///     Means one of view's checks failed.
        /// </summary>
        /// <param name="view">Malformed view</param>
        /// <param name="violatedContractName">Failed check name. See <see cref="Attributes.ApiViewCheckAttribute"/></param>
        /// <returns>Constructed ApiErrorView</returns>
        public static ApiErrorView ViewContractViolation(object view, string violatedContractName) => new ApiErrorView
        {
            Reason = ApiErrorReason.ViewContractViolation,
            ViewType = GetTypeName(view.GetType()),
            ErrorMessage = violatedContractName
        };

        /// <summary>
        ///     Constructs ApiErrorView with <see cref="ApiErrorReason.ViewSetConflict"/> reason.
        ///     Means received view set conteins internal contradictions.
        /// </summary>
        /// <param name="view">Malformed view (List-type)</param>
        /// <param name="errorMessage">Error description</param>
        /// <returns>Constructed ApiErrorView</returns>
        public static ApiErrorView ViewSetConflict(object view, string errorMessage) => new ApiErrorView
        {
            Reason = ApiErrorReason.ViewSetConflict,
            ViewType = GetTypeName(view.GetType()),
            ErrorMessage = errorMessage
        };

        /// <summary>
        ///     Constructs ApiErrorView with <see cref="ApiErrorReason.QueryParameterError"/> reason.
        ///     Means one of HTTP query params is missing or has invalid value.
        /// </summary>
        /// <param name="parameterName">Invalid parameter name</param>
        /// <returns>Constructed ApiErrorView</returns>
        public static ApiErrorView QueryParameterError(string parameterName) => new ApiErrorView
        {
            Reason = ApiErrorReason.QueryParameterError,
            Context = parameterName,
            ErrorMessage = "Bad parameter value (or parameter missing)"
        };

        /// <summary>
        ///     Constructs ApiErrorView with <see cref="ApiErrorReason.QueryParameterAmbiguous"/> reason.
        ///     Means request handler cannot be selected based on given query param set.
        /// </summary>
        /// <param name="availableParamSets">Correct param sets</param>
        /// <returns>Constructed ApiErrorView</returns>
        public static ApiErrorView QueryParameterAmbiguous(IEnumerable<string> availableParamSets) => new ApiErrorView
        {
            Reason = ApiErrorReason.QueryParameterAmbiguous,
            Context = string.Join(" OR ", availableParamSets),
            ErrorMessage = "Processing method cannot be selected"
        };

        /// <summary>
        ///     Constructs ApiErrorView with <see cref="ApiErrorReason.ForeignKeyError"/> reason.
        ///     Means received views contain invalid external links.
        /// </summary>
        /// <param name="viewType">Received view type</param>
        /// <param name="keyName">Foreign key name (property name)</param>
        /// <returns>Constructed ApiErrorView</returns>
        public static ApiErrorView ForeignKeyError(Type viewType, string keyName) => new ApiErrorView
        {
            Reason = ApiErrorReason.ForeignKeyError,
            Context = keyName,
            ViewType = GetTypeName(viewType),
            ErrorMessage = "Bad foreign key value"
        };

        /// <summary>
        ///     Constructs ApiErrorView with <see cref="ApiErrorReason.BusinessLogicError"/> reason.
        ///     Means received views violate application business logic in a complex way.
        /// </summary>
        /// <param name="errorMessage">Error description</param>
        /// <returns>Constructed ApiErrorView</returns>
        public static ApiErrorView BusinessLogicError(string errorMessage) => new ApiErrorView
        {
            Reason = ApiErrorReason.BusinessLogicError,
            ErrorMessage = errorMessage
        };

        /// <summary>
        ///     Constructs ApiErrorView with <see cref="ApiErrorReason.InnerErrors"/> reason.
        ///     Used for non-leaf error nodes.
        /// </summary>
        /// <param name="view">Received view</param>
        /// <param name="innerErrorViews">Inner errors</param>
        /// <returns>Constructed ApiErrorView</returns>
        public static ApiErrorView InnerErrors(object view, List<ApiErrorView> innerErrorViews) => new ApiErrorView
        {
            ViewType = GetTypeName(view?.GetType()),
            Reason = ApiErrorReason.InnerErrors,
            ErrorMessage = "Inner errors found",
            InnerErrorList = innerErrorViews,
        };

        /// <summary>
        ///     Constructs ApiErrorView with <see cref="ApiErrorReason.Unspecified"/> reason.
        ///     Means occured error cannot be categorized under <see cref="ApiErrorReason"/> enumeration.
        /// </summary>
        /// <param name="errorMessage">Error description</param>
        /// <returns>Constructed ApiErrorView</returns>
        public static ApiErrorView Unspecified(string errorMessage = null) => new ApiErrorView
        {
            Reason = ApiErrorReason.Unspecified,
            ErrorMessage = errorMessage ?? "Unknown error occured"
        };

        #endregion

        #region private

        private static string GetTypeName(Type type) => type?.GetFriendlyName() ?? "LOGIC";

        private static string GetFriendlyName(this Type type)
        {
            var friendlyName = type.Name;
            if (!type.IsConstructedGenericType)
                return friendlyName;

            var iBacktick = friendlyName.IndexOf('`');
            if (iBacktick > 0)
            {
                friendlyName = friendlyName.Remove(iBacktick);
            }
            friendlyName += "<";
            var typeParameters = type.GenericTypeArguments;
            for (var i = 0; i < typeParameters.Length; ++i)
            {
                var typeParamName = typeParameters[i].Name;
                friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
            }
            friendlyName += ">";

            return friendlyName;
        }

        #endregion
    }
}
