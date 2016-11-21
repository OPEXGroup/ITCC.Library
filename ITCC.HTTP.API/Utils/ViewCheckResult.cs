namespace ITCC.HTTP.API.Utils
{
    public class ViewCheckResult
    {
        #region properties
        public bool IsCorrect { get; private set; }

        public string ErrorDescription => _errorDescription == null
            ? null
            : $"VIEW ERROR: {_errorDescription}";
        #endregion

        #region construction
        private ViewCheckResult() { }
        public static ViewCheckResult Ok() => new ViewCheckResult {IsCorrect = true};
        public static ViewCheckResult Error(string errorDescription) => new ViewCheckResult {IsCorrect = false, _errorDescription = errorDescription};
        #endregion

        #region private
        private string _errorDescription;
        #endregion
    }
}
