// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Text;
using ITCC.HTTP.Server.Interfaces;

namespace ITCC.HTTP.Server.Encoders
{
    /// <summary>
    ///     Used for dynamic encoder creation
    /// </summary>
    public class CustomBodyEncoder : IBodyEncoder
    {
        #region IBodyEncoder
        public Encoding Encoding { get; set; }
        public string Serialize(object body) => Serializer?.Invoke(body);
        public string ContentType { get; set; }
        public bool AutoGzipCompression { get; set; }
        public bool IsDefault { get; set; }
        #endregion

        public Func<object, string> Serializer { get; set; }
    }
}
