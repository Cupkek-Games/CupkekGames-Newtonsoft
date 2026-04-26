using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace CupkekGames.Newtonsoft
{
    public class SerializationManager
    {
        public JsonSerializerSettings Settings;

        public void Initialize(
            IList<Type> knownTypes,
            IList<JsonConverter> converters,
            IContractResolver contractResolver = null,
            TypeNameHandling typeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling referenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting formatting = Formatting.Indented
        )
        {
            Settings = new JsonSerializerSettings
            {
                TypeNameHandling = typeNameHandling,
                ReferenceLoopHandling = referenceLoopHandling,
                Formatting = formatting,
            };

            if (contractResolver != null)
            {
                Settings.ContractResolver = contractResolver;
            }

            if (knownTypes != null)
            {
                Settings.SerializationBinder = new KnownTypesBinder { KnownTypes = knownTypes };
            }

            if (converters != null)
            {
                Settings.Converters = converters;
            }
        }

        public string Serialize<T>(T data)
        {
            if (Settings == null)
            {
                throw new InvalidOperationException("SerializationManager must be initialized before use.");
            }

            return JsonConvert.SerializeObject(data, Settings);
        }

        public T Deserialize<T>(string json)
        {
            if (Settings == null)
            {
                throw new InvalidOperationException("SerializationManager must be initialized before use.");
            }

            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        public JObject ParseToJObject(string json)
        {
            return JObject.Parse(json);
        }

        public T ConvertJObjectTo<T>(JObject jObject)
        {
            if (Settings == null)
            {
                throw new InvalidOperationException("SerializationManager must be initialized before use.");
            }

            return jObject.ToObject<T>(JsonSerializer.Create(Settings));
        }
    }
}