namespace ITCC.HTTP.Server.Utils
{
    internal class GeometricCounterProvider : ConstantCounterProvider
    {
        #region override

        protected override long InnerGetNextValue() => StartValue*(1 << (Iteration - 1));

        #endregion
    }
}
