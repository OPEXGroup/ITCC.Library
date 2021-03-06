﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using ITCC.HTTP.API.Enums;

namespace ITCC.HTTP.API.Utils
{
    /// <summary>
    ///     Represents view check result
    /// </summary>
    public class ViewCheckResult
    {
        #region properties
        public bool IsCorrect { get; private set; }

        public ApiErrorView ApiErrorView { get; private set; }

        public string ErrorDescription => ApiErrorView.Reason == ApiErrorReason.None
            ? null
            : ApiErrorView.ToString();
        #endregion

        #region construction
        private ViewCheckResult() { }
        public static ViewCheckResult Ok() => new ViewCheckResult { IsCorrect = true, ApiErrorView = ApiErrorViewFactory.None() };
        public static ViewCheckResult Error(ApiErrorView errorView) => new ViewCheckResult { IsCorrect = false, ApiErrorView = errorView };
        #endregion
    }
}
