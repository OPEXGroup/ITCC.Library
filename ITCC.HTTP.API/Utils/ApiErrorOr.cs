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
        /// <returns>Created ApiErrorOr</returns>
        public static ApiErrorOr<T> Error(ApiErrorView errorView) => new ApiErrorOr<T>(errorView);
        /// <summary>
        ///     Creates success instance with given data
        /// </summary>
        /// <param name="data">Success data</param>
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
