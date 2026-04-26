using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CupkekGames.Newtonsoft
{
    public class Vector2IntConverter : JsonConverter<Vector2Int>
    {
        public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
        {
            JObject obj = new JObject
            {
                { "x", value.x },
                { "y", value.y }
            };
            obj.WriteTo(writer);
        }

        public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            return new Vector2Int(obj["x"].Value<int>(), obj["y"].Value<int>());
        }
    }
}