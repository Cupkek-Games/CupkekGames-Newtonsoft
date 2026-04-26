using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CupkekGames.Newtonsoft
{
    public class GenericDictionaryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));
            return objectType.IsGenericType && typeof(Dictionary<,>) == objectType.GetGenericTypeDefinition();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                var array = JToken.ReadFrom(reader) as JArray;
                if (array == null)
                    throw new JsonSerializationException("Expected JArray but got null or different type.");

                var dictGenericTypes = objectType.GetGenericArguments();
                var dictKeyType = dictGenericTypes[0];
                var dictValueType = dictGenericTypes[1];

                IDictionary dictionary = existingValue as IDictionary ??
                    (IDictionary)Activator.CreateInstance(objectType);

                foreach (var item in array)
                {
                    if (!(item is JObject keyValuePair))
                        throw new JsonSerializationException("Expected key-value pair object.");

                    var keyToken = keyValuePair["key"];
                    var valueToken = keyValuePair["value"];

                    if (keyToken == null || valueToken == null)
                        throw new JsonSerializationException("Key-value pair must contain both 'key' and 'value' properties.");

                    var key = keyToken.ToObject(dictKeyType, serializer);
                    if (key == null)
                        throw new JsonSerializationException($"Failed to deserialize key of type {dictKeyType}");

                    var value = valueToken.ToObject(dictValueType, serializer);
                    dictionary.Add(key, value);
                }

                return dictionary;
            }
            catch (Exception ex) when (!(ex is JsonSerializationException))
            {
                throw new JsonSerializationException($"Error deserializing dictionary of type {objectType}", ex);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                var dict = (IDictionary)value;
                var dictGenericTypes = value.GetType().GetGenericArguments();
                var dictKeyType = dictGenericTypes[0];
                var dictValueType = dictGenericTypes[1];

                writer.WriteStartArray();

                foreach (DictionaryEntry entry in dict)
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("key");
                    serializer.Serialize(writer, entry.Key, dictKeyType);

                    writer.WritePropertyName("value");
                    serializer.Serialize(writer, entry.Value, dictValueType);

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }
            catch (Exception ex) when (!(ex is JsonSerializationException))
            {
                throw new JsonSerializationException($"Error serializing dictionary of type {value.GetType()}", ex);
            }
        }
    }
}