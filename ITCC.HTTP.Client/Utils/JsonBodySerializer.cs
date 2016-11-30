using ITCC.HTTP.Client.Interfaces;
using Newtonsoft.Json;

namespace ITCC.HTTP.Client.Utils
{
    public class JsonBodySerializer : IBodySerializer
    {
        #region properties
        public string ContentType => "application/json";
        public string Serialize(object data)
        {
            var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Serialize };
            return JsonConvert.SerializeObject(data, settings);
        }
        #endregion
    }
}
