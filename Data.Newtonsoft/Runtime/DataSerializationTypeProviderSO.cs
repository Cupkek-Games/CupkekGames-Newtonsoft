using CupkekGames.Newtonsoft;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Data.Newtonsoft
{
    [CreateAssetMenu(menuName = "CupkekGames/Data/Newtonsoft/Serialization Type Provider")]
    public class DataSerializationTypeProviderSO : SerializationTypeProviderSO
    {
        [SerializeField] private bool _serializedGuidConverter = true;

        public override IList<JsonConverter> GetConverters()
        {
            var converters = new List<JsonConverter>();

            if (_serializedGuidConverter)
                converters.Add(new SerializedGuidConverter());

            return converters;
        }
    }
}
