// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Text;
using ITCC.HTTP.Server.Interfaces;

namespace ITCC.HTTP.Server.Encoders
{
    /// <summary>
    ///     Used to serve text/plain requests by simple applying ToString()
    /// </summary>
    public class PlainTextBodyEncoder : IBodyEncoder
    {
        public PlainTextBodyEncoder(bool isDefault = false)
        {
            IsDefault = isDefault;
        }

        #region IBodyEncoder

        public Encoding Encoding => Encoding.UTF8;
        public string Serialize(object body) => body.ToString();
        public string ContentType => "text/plain";
        public bool AutoGzipCompression => true;
        public bool IsDefault { get; }

        #endregion
    }
}
