namespace ITCC.UI.Utils
{
    public class ValidationResult
    {
        public ValidationResult(bool condition, string errorMessage)
        {
            Condition = condition;
            ErrorMessage = errorMessage ?? "Valiadtion condition failed";
        }

        public bool Condition { get; private set; }

        public string ErrorMessage { get; private set; }
    }
}