using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Newtonsoft
{
  [CreateAssetMenu(menuName = "CupkekGames/Data/Newtonsoft/Default Type Provider")]
  public class NewtonsoftDefaultTypeProviderSO : SerializationTypeProviderSO
  {
    [SerializeField] private bool _vector2IntConverter = true;
    [SerializeField] private bool _genericDictionaryConverter = true;
    [SerializeField] private bool _privateSetterResolver = true;

    public override IList<JsonConverter> GetConverters()
    {
      var converters = new List<JsonConverter>();

      if (_vector2IntConverter)
        converters.Add(new Vector2IntConverter());

      if (_genericDictionaryConverter)
        converters.Add(new GenericDictionaryConverter());

      return converters;
    }

    public override IContractResolver GetContractResolver()
    {
      return _privateSetterResolver ? new PrivateSetterContractResolver() : null;
    }
  }
}
