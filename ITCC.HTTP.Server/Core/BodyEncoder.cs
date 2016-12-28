// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Text;

namespace ITCC.HTTP.Server.Core
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
        public Func<object, string> Serializer { get; set; } = o => o.ToString();

        /// <summary>
        ///     Used for Content-Type header
        /// </summary>
        public string ContentType { get; set; } = "text/plain";

        /// <summary>
        ///     If true, gzip will be used every time client sends Accept-Encoding: gzip
        /// </summary>
        public bool AutoGzipCompression { get; set; } = true;

        /// <summary>
        ///     If true, this encoder will be used for request with Accept header missng
        /// </summary>
        public bool IsDefault { get; set; } = true;
    }
}
