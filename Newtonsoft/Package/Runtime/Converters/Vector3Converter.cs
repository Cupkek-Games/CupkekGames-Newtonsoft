using System;
using Newtonsoft.Json;
using UnityEngine;

namespace CupkekGames.Newtonsoft
{
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            AssertToken(reader, JsonToken.StartArray);
            var v = new Vector3
            {
                x = (float)reader.ReadAsDouble(),
                y = (float)reader.ReadAsDouble(),
                z = (float)reader.ReadAsDouble(),
            };
            reader.Read();
            AssertToken(reader, JsonToken.EndArray);
            return v;
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            var format = writer.Formatting;

            writer.WriteStartArray();
            writer.Formatting = Formatting.None;
            writer.WriteValue(value.x);
            writer.WriteValue(value.y);
            writer.WriteValue(value.z);
            writer.WriteEndArray();

            writer.Formatting = format;
        }

        private void AssertToken(JsonReader reader, JsonToken token)
        {
            if (reader.TokenType == token)
                return;

            throw new JsonSerializationException($"Expected {token}, but got {reader.TokenType}");
        }
    }
}