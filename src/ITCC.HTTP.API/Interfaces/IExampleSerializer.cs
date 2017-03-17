// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace ITCC.HTTP.API.Interfaces
{
    /// <summary>
    ///     API request/response example serializer interface
    /// </summary>
    public interface IExampleSerializer
    {
        /// <summary>
        ///     Common example header
        /// </summary>
        string ExampleHeader { get; }
        /// <summary>
        ///     Method used to serialize body example
        /// </summary>
        /// <param name="example">Example object</param>
        /// <returns>Serialized string</returns>
        /// <remarks>
        ///     Should not throw
        /// </remarks>
        string Serialize(object example);
    }
}
