// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.HTTP.Server.Utils
{
    internal class LinearCounterProvider : ConstantCounterProvider
    {
        #region override
        protected override long InnerGetNextValue() => InnerStartValue * (Iteration - 1);

        #endregion
    }
}
