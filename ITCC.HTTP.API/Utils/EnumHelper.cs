// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using System.Linq;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.API.Utils
{
    public static class EnumHelper
    {
        #region ApiContractType

        private static readonly Dictionary<ApiContractType, string> ApiContractTypeDictionary = new Dictionary<ApiContractType, string>
        {
            {ApiContractType.NotNull, "Value cannot be null" },
            {ApiContractType.PositiveNumber, "Value must be positive" },
            {ApiContractType.NonNegativeNumber, "Value cannot be negative" },
            {ApiContractType.NonPositiveNumber, "Value cannot be positive" },
            {ApiContractType.NegativeNumber, "Value must be negative" },
            {ApiContractType.NotZero, "Value cannot be zero" },
            {ApiContractType.EvenNumber, "Value must be even number" },
            {ApiContractType.OddNumber, "Value must be odd number" },
            {ApiContractType.EnumValue, "Value must be within enum values (with flags)" },
            {ApiContractType.StrictEnumValue, "Value must be within enum values" },
            {ApiContractType.NonWhitespaceString, "Value cannot be whitespace" },
            {ApiContractType.UriString, "Value must be a correct uri string" },
            {ApiContractType.GuidString, "Value must be a valid guid" },
            {ApiContractType.NotEmpty, "Collection cannot be empty" },
            {ApiContractType.NotSingle, "Collection must contain at least 2 elements" },
            {ApiContractType.ItemsNotNull, "Collection cannot contain nulls" },
            {ApiContractType.ItemsGuidStrings, "Collection elements must be valid guids" }
        };

        public static string ApiContractTypeName(ApiContractType apiContractType) => ApiContractTypeDictionary.ContainsKey(apiContractType) ? ApiContractTypeDictionary[apiContractType] : "UNKNOWN";

        public static ApiContractType ApiContractTypeByName(string name) => ApiContractTypeDictionary.Where(item => item.Value == name).Select(item => item.Key).FirstOrDefault();

        #endregion
    }
}
