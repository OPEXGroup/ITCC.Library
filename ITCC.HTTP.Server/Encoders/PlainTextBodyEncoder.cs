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

        public Encoding Encoding => Encoding.UTF8;
        public string Serialize(object body) => body.ToString();
        public string ContentType => "text/plain";
        public bool AutoGzipCompression => true;
        public bool IsDefault { get; }
    }
}
