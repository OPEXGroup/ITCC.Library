namespace ITCC.HTTP.Server.Utils
{
    internal class LinearCounterProvider : ConstantCounterProvider
    {
        #region override
        protected override long InnerGetNextValue() => InnerStartValue * (Iteration - 1);

        #endregion
    }
}
