// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

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

        #region IBodyEncoder

        public Encoding Encoding => Encoding.UTF8;
        public string Serialize(object body) => JsonConvert.SerializeObject(body,
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        public string ContentType => "application/json";
        public bool AutoGzipCompression => true;
        public bool IsDefault { get; }

        #endregion
    }
}
