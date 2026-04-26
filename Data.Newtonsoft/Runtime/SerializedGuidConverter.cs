using System;
using CupkekGames.Data.Primitives;
using Newtonsoft.Json;

namespace CupkekGames.Data.Newtonsoft
{
    public class SerializedGuidConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SerializedGuid);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return new SerializedGuid();

            string guidString = reader.Value?.ToString();
            return new SerializedGuid(guidString);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is SerializedGuid guid)
            {
                writer.WriteValue(guid.ValueStr);
            }
        }
    }
}
