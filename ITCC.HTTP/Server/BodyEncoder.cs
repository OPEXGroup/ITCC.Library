using System.Text;
using ITCC.HTTP.Common;
using Newtonsoft.Json;

namespace ITCC.HTTP.Server
{
    public class BodyEncoder
    {
        /// <summary>
        ///     Character stream encoding
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        ///     Body serializer
        /// </summary>
        public Delegates.BodySerializer Serializer { get; set; } = JsonConvert.SerializeObject;

        /// <summary>
        ///     Used for Content-Type header
        /// </summary>
        public string ContentType { get; set; } = "application/json";

        /// <summary>
        ///     If true, gzip will be used every time client sends Accept-Encoding: gzip
        /// </summary>
        public bool AutoGzipCompression { get; set; } = true;
    }
}
