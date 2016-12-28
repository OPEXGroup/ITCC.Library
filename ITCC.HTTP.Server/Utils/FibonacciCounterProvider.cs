// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.HTTP.Server.Utils
{
    internal class FibonacciCounterProvider : ConstantCounterProvider
    {
        #region override

        protected override long InnerGetNextValue()
        {
            // Already incremented in base class
            if (Iteration < 3)
                return StartValue;

            var newValue = _prevValue + _prevPrevValue;
            _prevPrevValue = _prevValue;
            _prevValue = newValue;
            return newValue;
        }

        public override void Reset()
        {
            lock (Lock)
            {
                _prevPrevValue = InnerStartValue;
                _prevValue = InnerStartValue;
                Iteration = 0;
            }
        }

        #endregion

        #region private

        private long _prevValue;
        private long _prevPrevValue;

        #endregion
    }
}
