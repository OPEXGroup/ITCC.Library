using System;
using Newtonsoft.Json;

namespace ITCC.HTTP.Utils
{
    public class PingJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var pingResponse = value as PingResponse;
            if (pingResponse == null)
                throw new ArgumentNullException(nameof(pingResponse));

            writer.WriteStartObject();
            
            writer.WritePropertyName("Time");
            writer.WriteValue(pingResponse.Time.ToString("s"));
            writer.WritePropertyName("Request");
            writer.WriteValue(pingResponse.Request);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (PingResponse);
        }

        public override bool CanRead => false;
    }
}
