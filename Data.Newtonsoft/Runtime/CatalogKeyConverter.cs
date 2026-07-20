using System;
using CupkekGames.Data;
using Newtonsoft.Json;

namespace CupkekGames.Data.Newtonsoft
{
    /// <summary>
    /// Serializes <see cref="CatalogKey"/> as a compact "Catalog/Key" string instead of a
    /// nested object. The catalog id must not contain '/'; the key may.
    /// </summary>
    public class CatalogKeyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CatalogKey);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return new CatalogKey();

            string value = reader.Value?.ToString();
            if (string.IsNullOrEmpty(value))
                return new CatalogKey();

            int separator = value.IndexOf('/');
            if (separator < 0)
                return new CatalogKey { Key = value };

            return new CatalogKey
            {
                Catalog = value.Substring(0, separator),
                Key = value.Substring(separator + 1)
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is CatalogKey catalogKey)
            {
                if (string.IsNullOrEmpty(catalogKey.Catalog) && string.IsNullOrEmpty(catalogKey.Key))
                {
                    writer.WriteNull();
                    return;
                }

                writer.WriteValue($"{catalogKey.Catalog}/{catalogKey.Key}");
            }
        }
    }
}
