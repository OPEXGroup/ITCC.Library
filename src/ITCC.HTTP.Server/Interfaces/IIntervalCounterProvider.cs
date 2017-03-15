// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.HTTP.Server.Interfaces
{
    /// <summary>
    ///     This interface used for memory overhead warnings
    /// </summary>
    /// <remarks>
    /// We can do the following:
    ///     1) Initialize interval provider with initial value
    ///     2) Get the first count
    ///     3) Get next counter that is greaterer than first one
    ///     4) Reset to first count
    /// 
    /// With this we can achive simple switches between memory warning strategies
    /// </remarks>
    internal interface IIntervalCounterProvider
    {
        long StartValue { get; set; }

        long GetNextCount();

        void Reset();
    }
}
