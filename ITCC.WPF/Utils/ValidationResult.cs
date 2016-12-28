// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.WPF.Utils
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