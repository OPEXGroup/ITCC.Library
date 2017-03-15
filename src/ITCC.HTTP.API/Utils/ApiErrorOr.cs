// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Diagnostics.CodeAnalysis;

namespace ITCC.HTTP.API.Utils
{
    /// <summary>
    ///     Represents Either with typeof(TFirst) == typeof(ApiErrorView)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiErrorOr<T> : Either<ApiErrorView, T>
        where T : class
    {
        #region public
        /// <summary>
        ///     Creates error instance with given error
        /// </summary>
        /// <param name="errorView">Error view</param>
        /// <exception cref="ArgumentNullException">Thrown if errorView is null</exception>
        /// <returns>Created ApiErrorOr</returns>
        public static ApiErrorOr<T> Error(ApiErrorView errorView) => new ApiErrorOr<T>(errorView);
        /// <summary>
        ///     Creates success instance with given data
        /// </summary>
        /// <param name="data">Success data</param>
        /// <exception cref="ArgumentNullException">Thrown if data is null</exception>
        /// <returns>Created ApiErrorOr</returns>
        public static ApiErrorOr<T> Success(T data) => new ApiErrorOr<T>(data);

        /// <summary> Error view </summary>
        public ApiErrorView ErrorView => First;
        /// <summary> Success data </summary>
        public T Data => Second;

        /// <summary> True iff ErrorView != null </summary>
        public bool IsError => ErrorView != null;
        /// <summary> True iff Value != null </summary>
        public bool IsSuccess => !IsError;
        #endregion

        #region private

        private ApiErrorOr(ApiErrorView errorView) : base(errorView) { }
        private ApiErrorOr(T data) : base(data) { }
        #endregion
    }
}
