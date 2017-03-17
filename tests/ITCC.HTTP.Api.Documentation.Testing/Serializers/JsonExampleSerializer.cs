// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.HTTP.API.Interfaces;
using Newtonsoft.Json;

namespace ITCC.HTTP.Api.Documentation.Testing.Serializers
{
    internal class JsonExampleSerializer : IExampleSerializer
    {
        public string ExampleHeader => "Json example";
        public string Serialize(object example)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            return JsonConvert.SerializeObject(example, Formatting.Indented, settings);
        }
    }
}
