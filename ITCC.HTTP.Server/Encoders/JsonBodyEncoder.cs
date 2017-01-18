using System.Text;
using ITCC.HTTP.Server.Interfaces;
using Newtonsoft.Json;

namespace ITCC.HTTP.Server.Encoders
{
    /// <summary>
    ///     Used to serve application/xml requests
    /// </summary>
    public class JsonBodyEncoder : IBodyEncoder
    {
        public JsonBodyEncoder(bool isDefault = false)
        {
            IsDefault = isDefault;
        }

        public Encoding Encoding => Encoding.UTF8;
        public string Serialize(object body) => JsonConvert.SerializeObject(body,
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        public string ContentType => "application/json";
        public bool AutoGzipCompression => true;
        public bool IsDefault { get; }
    }
}
