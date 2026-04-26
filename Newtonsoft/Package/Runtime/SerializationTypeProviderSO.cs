using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Newtonsoft
{
  public abstract class SerializationTypeProviderSO : ScriptableObject
  {
    public virtual IList<Type> GetKnownTypes() => Array.Empty<Type>();
    public virtual IList<JsonConverter> GetConverters() => Array.Empty<JsonConverter>();
    public virtual IContractResolver GetContractResolver() => null;
  }
}
