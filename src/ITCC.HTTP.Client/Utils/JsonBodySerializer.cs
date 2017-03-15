// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Text;
using ITCC.HTTP.Client.Interfaces;
using Newtonsoft.Json;

namespace ITCC.HTTP.Client.Utils
{
    public class JsonBodySerializer : IBodySerializer
    {
        #region IBodySerializer
        public Encoding Encoding => Encoding.UTF8;
        public string ContentType => "application/json";
        public string Serialize(object data)
        {
            var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Serialize };
            return JsonConvert.SerializeObject(data, settings);
        }
        #endregion
    }
}
