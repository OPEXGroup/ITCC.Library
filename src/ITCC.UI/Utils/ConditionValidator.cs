// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITCC.UI.Utils
{
    public class ConditionValidator
    {
        #region customConditions

        public void NonNull(object something, string message = null)
        {
            var condition = something != null;
            _conditions.Add(new ValidationResult(condition, message));
            _conditionsChanged = true;
        }

        public void NonWhitespaceString(string text, string message = null)
        {
            var condition = !string.IsNullOrWhiteSpace(text);
            _conditions.Add(new ValidationResult(condition, message));
            _conditionsChanged = true;
        }

        public void DoesNotThrowException(Action action, string message = null)
        {
            try
            {
                action.Invoke();
                _conditions.Add(new ValidationResult(true, message));
            }
            catch (Exception)
            {
                _conditions.Add(new ValidationResult(false, message));
            }
            _conditionsChanged = true;
        }

        public void AddSafeCondition(Func<bool> conditionMethod, string message = null)
        {
            try
            {
                var conditionCheckResult = conditionMethod.Invoke();
                _conditions.Add(new ValidationResult(conditionCheckResult, message));
            }
            catch (Exception)
            {
                _conditions.Add(new ValidationResult(false, message));
            }
            _conditionsChanged = true;
        }

        #endregion

        #region common

        public bool AddCondition(bool condition, string message = null)
        {
            var conditionCheckResult = condition;
            _conditions.Add(new ValidationResult(condition, message));
            _conditionsChanged = true;
            return conditionCheckResult;
        }

        public bool AddCondition(Func<bool> conditionMethod, string message = null)
        {
            var conditionCheckResult = conditionMethod.Invoke();
            _conditions.Add(new ValidationResult(conditionCheckResult, message));
            _conditionsChanged = true;
            return conditionCheckResult;
        }

        public async Task<bool> AddConditionAsync(Task<bool> conditionTask, string message = null)
        {
            var conditionCheckResult = await conditionTask;
            _conditions.Add(new ValidationResult(await conditionTask, message));
            _conditionsChanged = true;
            return conditionCheckResult;
        }

        public async Task<bool> AddConditionAsync(Func<Task<bool>> conditionMethod, string message = null)
        {
            var conditionCheckResult = await conditionMethod.Invoke();
            _conditions.Add(new ValidationResult(conditionCheckResult, message));
            _conditionsChanged = true;
            return conditionCheckResult;
        }

        #endregion

        #region properties

        public ValidationResult ValidationResult
        {
            get
            {
                if (!_conditionsChanged)
                    return _cachedResult;

                var firstFailed = _conditions.FirstOrDefault(c => c.Condition == false);
                _cachedResult = firstFailed ?? new ValidationResult(true, null);
                _conditionsChanged = false;
                return _cachedResult;
            }
        }

        public bool ValidationPassed => ValidationResult.Condition;

        public string ErrorMessage => ValidationResult.ErrorMessage;

        #endregion

        #region private

        private readonly List<ValidationResult> _conditions = new List<ValidationResult>();

        private bool _conditionsChanged;

        private ValidationResult _cachedResult = new ValidationResult(true, null);

        #endregion
    }
}