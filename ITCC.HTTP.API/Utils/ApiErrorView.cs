using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.API.Utils
{
    /// <summary>
    ///     Represents 
    /// </summary>
    public class ApiErrorView
    {
        #region properties
        /// <summary>
        ///     Error reason
        /// </summary>
        public ApiErrorReason Reason { get; set; }

        /// <summary>
        ///     Used for <see cref="ApiErrorReason.ViewPropertyContractViolation"/>
        /// </summary>
        public ApiContractType ViolatedContract { get; set; }

        /// <summary>
        ///     Property name for leaves. Null if Reason is <see cref="ApiErrorReason.InnerErrors"/>
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        ///     Not null if error is about single entity
        /// </summary>
        public string ViewType { get; set; }

        /// <summary>
        ///     Error description
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     Inner error list
        /// </summary>
        public List<ApiErrorView> InnerErrorList { get; set; }
        #endregion

        #region serialization

        private const string IndentationUnit = "\t";

        private string Serialize(string indent)
        {
            const string inner = "INNER";
            var builder = new StringBuilder();
            if (Reason == ApiErrorReason.InnerErrors)
            {
                builder.AppendLine($"{indent}[{ViewType ?? inner,14}] The following inner errors found:");
                indent = indent + IndentationUnit;
                if (InnerErrorList?.Any() == true)
                {
                    foreach (var apiErrorView in InnerErrorList)
                    {
                        builder.Append(apiErrorView.Serialize(indent));
                    }
                }
            }
            else
            {
                builder.AppendLine($"{indent}[{Context ?? "LOGIC",14}]: {ErrorMessage}");
            }

            return builder.ToString();
        }

        public override string ToString() => Serialize(string.Empty);

        #endregion
    }
}
