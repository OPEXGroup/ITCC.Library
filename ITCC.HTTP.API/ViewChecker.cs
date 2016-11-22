using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;
using ITCC.HTTP.API.Extensions;
using ITCC.HTTP.API.Utils;

namespace ITCC.HTTP.API
{
    /// <summary>
    ///     Used to check API views' contracts
    /// </summary>
    public static class ViewChecker
    {
        #region public
        /// <summary>
        ///     Checks if view complies with it's contract
        /// </summary>
        /// <param name="view">Object to check</param>
        /// <returns>Simply ok or verbose contract errors description (for reading, not serializing)</returns>
        /// <exception cref="InvalidOperationException">
        ///     Will be thrown if <see cref="view"/> does not have <see cref="ApiViewAttribute"/> attribute
        /// </exception>
        public static ViewCheckResult CheckContract(object view)
        {
            if (view == null)
                return ViewCheckResult.Ok();

            if (! view.IsApiView())
                throw new InvalidOperationException("No classes except API views can have contracts");

            try
            {
                var properties = view.GetType().GetRuntimeProperties();
                var checkResults = new List<ApiErrorView>();
                var innerCheckResults = new List<ApiErrorView>();

                innerCheckResults.AddRange(PerformSpecificChecks(view));

                foreach (var propertyInfo in properties)
                {
                    var propertyValue = propertyInfo.GetValue(view);

                    if (propertyValue != null)
                    {
                        if (propertyValue.IsApiView())
                        {
                            var checkResult = CheckContract(propertyValue);
                            if (!checkResult.IsCorrect)
                                innerCheckResults.Add(checkResult.ApiErrorView);
                        }
                        if (propertyValue.IsApiViewList())
                        {
                            var list = propertyValue as IList;
                            if (list == null)
                                continue;
                            innerCheckResults.AddRange(from object item in list
                                                       select CheckContract(item)
                                                       into checkResult
                                                       where !checkResult.IsCorrect
                                                       select checkResult.ApiErrorView);
                        }
                    }

                    var contractAttributes = propertyInfo.GetCustomAttributes<ApiContractAttribute>().ToList();
                    if (! contractAttributes.Any())
                        continue;

                    if (contractAttributes.Count != 1)
                        return ViewCheckResult.Error(ApiErrorViewFactory.Unspecified());

                    var contractAttribute = contractAttributes[0];
                    var contractType = contractAttribute.Type;
                    
                    var failedChecks = CheckerDictionary
                        .Where(kv => contractType.HasFlag(kv.Key) && !kv.Value.Invoke(propertyValue))
                        .Select(kv => kv.Key)
                        .ToList();

                    if (failedChecks.Any())
                    {
                        var propertyName = propertyInfo.Name;
                        checkResults.AddRange(
                            failedChecks.Select(fc => ApiErrorViewFactory.ViewPropertyContractViolation(view, propertyName, fc)));
                    }
                }
                return BuildCheckResult(view, checkResults, innerCheckResults);
            }
            catch (Exception ex)
            {
                return ViewCheckResult.Error(ApiErrorViewFactory.Unspecified($"Failed to check view: {ex.Message}"));
            }
        }
        #endregion

        #region private
        /// <summary>
        ///     Key is a contract type, value is a method that checks property value against it and returns true is case of success
        /// </summary>
        private static readonly Dictionary<ApiContractType, Func<object, bool>> CheckerDictionary
            = new Dictionary<ApiContractType, Func<object, bool>>
            {
                {ApiContractType.NotNull, CheckNotNull },
                {ApiContractType.CanBeNull, MockCheck },
                {ApiContractType.PositiveNumber, CheckPositiveNumber },
                {ApiContractType.NonNegativeNumber, CheckNonNegativeNumber },
                {ApiContractType.NonPositiveNumber, CheckNonPositiveNumber },
                {ApiContractType.NegativeNumber, CheckNegativeNumber },
                {ApiContractType.NotZero, CheckNotZero },
                {ApiContractType.EvenNumber, CheckEvenNumber },
                {ApiContractType.OddNumber, CheckOddNumber },
                {ApiContractType.EnumValue, CheckEnumValue },
                {ApiContractType.StrictEnumValue, CheckStrictEnumValue },
                {ApiContractType.NonWhitespaceString, CheckNonWhitespaceString },
                {ApiContractType.UriString, CheckUriString },
                {ApiContractType.GuidString, CheckGuidString },
                {ApiContractType.NotEmpty, CheckNotEmpty },
                {ApiContractType.NotSingle, CheckNotSingle },
                {ApiContractType.CanBeEmpty, MockCheck },
                {ApiContractType.ItemsNotNull, CheckItemsNotNull },
                {ApiContractType.ItemsCanBeNull, MockCheck },
                {ApiContractType.ItemsGuidStrings, CheckItemsGuidStrings },
            };

        /// <summary>
        ///     Returns list or errors
        /// </summary>
        /// <param name="view">View object</param>
        /// <returns>List of error descriptions (empty list if no contracts are violated)</returns>
        private static IEnumerable<ApiErrorView> PerformSpecificChecks(object view)
        {
            var result = new List<ApiErrorView>();
            var methodInfos = view
                .GetType()
                .GetRuntimeMethods()
                .Where(
                    mi =>
                        mi.ReturnType == typeof(bool)
                        && mi.GetParameters().Length == 0
                        && mi.GetCustomAttributes<ApiViewCheckAttribute>().Any())
                .ToList();

            foreach (var methodInfo in methodInfos)
            {
                var deleg = methodInfo.CreateDelegate(typeof(Func<bool>), view);
                var checkResult = (bool)deleg.DynamicInvoke();
                if (!checkResult)
                {
                    var apiErrorView = ApiErrorViewFactory.ViewContractViolation(view,
                        methodInfo.GetCustomAttribute<ApiViewCheckAttribute>().ErrorDescription);
                    result.Add(apiErrorView);
                }
            }
            return result;
        }

        private static ViewCheckResult BuildCheckResult(object view, List<ApiErrorView> errors, List<ApiErrorView> innerErrors)
        {
            var hasErrors = errors.Any();
            var hasInnerErrors = innerErrors.Any();

            if (!hasInnerErrors && !hasErrors)
                return ViewCheckResult.Ok();

            var allErrors = new List<ApiErrorView>(errors);
            if (hasInnerErrors)
                allErrors.Add(ApiErrorViewFactory.InnerErrors(view, innerErrors));

            return ViewCheckResult.Error(ApiErrorViewFactory.InnerErrors(view, allErrors));
        }

        #region checks
        private static bool MockCheck(object propertyValue) => true;
        private static bool CheckNotNull(object propertyValue) => propertyValue != null;

        private static bool CheckPositiveNumber(object propertyValue)
        {
            double number;
            if (!TryConvertToDouble(propertyValue, out number))
                return false;

            return number > 0;
        }

        private static bool CheckNonNegativeNumber(object propertyValue)
        {
            double number;
            if (!TryConvertToDouble(propertyValue, out number))
                return false;

            return number >= 0;
        }

        private static bool CheckNonPositiveNumber(object propertyValue)
        {
            double number;
            if (!TryConvertToDouble(propertyValue, out number))
                return false;

            return number <= 0;
        }

        private static bool CheckNegativeNumber(object propertyValue)
        {
            double number;
            if (!TryConvertToDouble(propertyValue, out number))
                return false;

            return number < 0;
        }

        private static bool CheckNotZero(object propertyValue)
        {
            long number;
            if (!TryConvertToLong(propertyValue, out number))
                return false;

            return number != 0;
        }

        private static bool CheckEvenNumber(object propertyValue)
        {
            long number;
            if (!TryConvertToLong(propertyValue, out number))
                return false;

            return number % 2 == 0;
        }

        private static bool CheckOddNumber(object propertyValue)
        {
            long number;
            if (!TryConvertToLong(propertyValue, out number))
                return false;

            return number % 2 == 1;
        }

        private static bool CheckEnumValue(object propertyValue)
        {
            var type = propertyValue.GetType();
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsEnum)
                return true;

            try
            {
                if (!typeInfo.GetCustomAttributes<FlagsAttribute>().Any())
                    return Enum.IsDefined(type, propertyValue);

                var values = Enum.GetValues(type);
                foreach (var subset in values.GetSubsets())
                {
                    var subsetValueSum = subset.Select(v => (int)v).Sum();
                    if (Convert.ToInt32(propertyValue) == subsetValueSum)
                        return true;
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static bool CheckStrictEnumValue(object propertyValue)
        {
            var type = propertyValue.GetType();
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsEnum)
                return true;

            try
            {
                return Enum.IsDefined(type, propertyValue);
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static bool CheckNonWhitespaceString(object propertyValue)
        {
            if (propertyValue == null)
                return true;

            string str;
            if (!TryConvertToString(propertyValue, out str))
                return false;

            return !string.IsNullOrWhiteSpace(str);
        }

        private static bool CheckUriString(object propertyValue)
        {
            if (propertyValue == null)
                return true;

            string str;
            if (!TryConvertToString(propertyValue, out str))
                return false;

            Uri uriResult;
            return Uri.TryCreate(str, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == "http" || uriResult.Scheme == "https");
        }

        private static bool CheckGuidString(object propertyValue)
        {
            string str;
            if (!TryConvertToString(propertyValue, out str))
                return false;

            return IsGuidOrNull(str);
        }

        private static bool CheckNotEmpty(object propertyValue)
        {
            if (propertyValue == null)
                return true;

            IList list;
            if (!TryConvertToList(propertyValue, out list))
                return false;

            return list.Count > 0;
        }

        private static bool CheckNotSingle(object propertyValue)
        {
            if (propertyValue == null)
                return true;

            IList list;
            if (!TryConvertToList(propertyValue, out list))
                return false;

            return list.Count > 1;
        }

        private static bool CheckItemsNotNull(object propertyValue)
        {
            if (propertyValue == null)
                return true;

            IList list;
            if (!TryConvertToList(propertyValue, out list))
                return false;

            return list.Cast<object>().All(item => item != null);
        }

        private static bool CheckItemsGuidStrings(object propertyValue)
        {
            if (propertyValue == null)
                return true;

            List<string> list;
            if (!TryConvertToStringList(propertyValue, out list))
                return false;

            return list.All(IsGuidOrNull);
        }

        
        #endregion

        #region utils

        private static bool TryConvertToDouble(object value, out double result)
        {
            try
            {
                result = Convert.ToDouble(value);
                return true;
            }
            catch (Exception)
            {
                result = 0;
                return false;
            }
        }
        private static bool TryConvertToLong(object value, out long result)
        {
            try
            {
                result = Convert.ToInt64(value);
                return true;
            }
            catch (Exception)
            {
                result = 0;
                return false;
            }
        }

        private static bool TryConvertToString(object value, out string result)
        {
            try
            {
                result = value == null ? null : Convert.ToString(value);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        private static bool TryConvertToStringList(object value, out List<string> result)
        {
            try
            {
                result = (List<string>) value;
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        private static bool TryConvertToList(object value, out IList result)
        {
            try
            {
                result = (IList)value;
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        private static bool IsGuidOrNull(string str)
        {
            if (str == null)
                return true;

            return GuidRegEx.IsMatch(str);
        }

        private static readonly Regex GuidRegEx = new Regex(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$");
        #endregion

        #endregion
    }
}
