using System.Text;

namespace ITCC.HTTP.Server.Interfaces
{
    /// <summary>
    ///     Used for server response body serializations. IMPORTANT: implementations MUST be thraed-safe
    /// </summary>
    public interface IBodyEncoder
    {
        /// <summary>
        ///     Character stream encoding
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        ///     Body serializer
        /// </summary>
        string Serialize(object body);

        /// <summary>
        ///     Used for Content-Type header and selection based on client Accept header
        /// </summary>
        string ContentType { get; }

        /// <summary>
        ///     If true, gzip will be used every time client sends Accept-Encoding: gzip (useful for formats like XML but takes some CPU time)
        /// </summary>
        bool AutoGzipCompression { get; }

        /// <summary>
        ///     If true, this encoder will be used for request with Accept header missng
        /// </summary>
        bool IsDefault { get; }
    }
}
