using CupkekGames.Newtonsoft;
using CupkekGames.Systems;
using Newtonsoft.Json;

namespace CupkekGames.Data.Newtonsoft
{
    public class NewtonsoftDataSerializer : IDataSerializer
    {
        private static SerializationManager Manager =>
            ServiceLocator.Get<CupkekGames.Newtonsoft.SerializationManager>();

        public string Serialize<T>(T data)
        {
            return Manager.Serialize(data);
        }

        public T Deserialize<T>(string json)
        {
            return Manager.Deserialize<T>(json);
        }

        public void Populate<T>(string json, T target)
        {
            JsonConvert.PopulateObject(json, target, Manager.Settings);
        }

        public T Clone<T>(T source)
        {
            return Deserialize<T>(Serialize(source));
        }
    }
}
