// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.HTTP.Server.Utils
{
    internal class GeometricCounterProvider : ConstantCounterProvider
    {
        #region override

        protected override long InnerGetNextValue() => StartValue*(1 << (Iteration - 1));

        #endregion
    }
}
