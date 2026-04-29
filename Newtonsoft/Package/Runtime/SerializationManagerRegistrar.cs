using CupkekGames.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CupkekGames.Newtonsoft
{
  [CreateAssetMenu(menuName = "CupkekGames/Data/Newtonsoft/Serialization Manager Registrar")]
  public class SerializationManagerRegistrar : ServiceProviderSO
  {
    [SerializeField] private TypeNameHandling _typeNameHandling = TypeNameHandling.Auto;
    [SerializeField] private ReferenceLoopHandling _referenceLoopHandling = ReferenceLoopHandling.Ignore;
    [SerializeField] private Formatting _formatting = Formatting.Indented;
    [SerializeField] private List<SerializationTypeProviderSO> _providers = new();

    private SerializationManager _instance;

    public override void RegisterServices()
    {
      _instance = new SerializationManager();
      ServiceLocator.Register(_instance, typeof(SerializationManager));

      var allTypes = new List<Type>();
      var allConverters = new List<JsonConverter>();
      IContractResolver resolver = null;

      foreach (var provider in _providers)
      {
        if (provider == null) continue;

        var types = provider.GetKnownTypes();
        if (types != null) allTypes.AddRange(types);

        var converters = provider.GetConverters();
        if (converters != null) allConverters.AddRange(converters);

        var providerResolver = provider.GetContractResolver();
        if (providerResolver != null)
        {
          if (resolver != null)
            Debug.LogWarning($"[SerializationManagerRegistrar] Multiple contract resolvers found. " +
                             $"'{provider.name}' overrides the previously set resolver.", provider);
          resolver = providerResolver;
        }
      }

      allTypes = allTypes.Distinct().ToList();

      _instance.Initialize(allTypes, allConverters, resolver, _typeNameHandling, _referenceLoopHandling, _formatting);
    }

    public override void UnregisterServices()
    {
      ServiceLocator.Remove<SerializationManager>();
      _instance = null;
    }
  }
}
