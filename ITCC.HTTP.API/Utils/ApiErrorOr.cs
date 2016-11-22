namespace ITCC.HTTP.API.Utils
{
    public class ApiErrorOr<T> : Either<ApiErrorView, T>
        where T : class
    {
        #region public
        public static ApiErrorOr<T> Error(ApiErrorView errorView) => new ApiErrorOr<T>(errorView);
        public static ApiErrorOr<T> Success(T data) => new ApiErrorOr<T>(data);

        public ApiErrorView ErrorView => First;
        public bool IsError => ErrorView != null;
        public T Value => Second;
        public bool IsSuccess => !IsError;
        #endregion

        #region private
        private ApiErrorOr(ApiErrorView errorView) : base(errorView) { }
        private ApiErrorOr(T data) : base(data) { }
        #endregion
    }
}
