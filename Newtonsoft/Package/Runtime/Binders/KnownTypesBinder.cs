using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace CupkekGames.Newtonsoft
{
    /// <summary>
    /// Uses short <c>$type</c> names only for a fixed set of polymorphic roots (e.g. item features).
    /// Everything else defers to <see cref="DefaultSerializationBinder"/> so generics like
    /// <c>List&lt;T&gt;</c> get a full, round-trippable type name instead of ambiguous <c>List`1</c>.
    /// </summary>
    public class KnownTypesBinder : ISerializationBinder
    {
        private static readonly ISerializationBinder DefaultBinder = new DefaultSerializationBinder();

        public IList<Type> KnownTypes { get; set; }

        public Type BindToType(string assemblyName, string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            Type fromDefault = DefaultBinder.BindToType(assemblyName, typeName);
            if (fromDefault != null)
                return fromDefault;

            if (KnownTypes == null)
                return null;

            for (int i = 0; i < KnownTypes.Count; i++)
            {
                Type t = KnownTypes[i];
                if (t != null && t.Name == typeName)
                    return t;
            }

            return null;
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (serializedType == null)
            {
                assemblyName = null;
                typeName = null;
                return;
            }

            if (serializedType.IsGenericType)
            {
                DefaultBinder.BindToName(serializedType, out assemblyName, out typeName);
                return;
            }

            if (KnownTypes != null)
            {
                for (int i = 0; i < KnownTypes.Count; i++)
                {
                    Type t = KnownTypes[i];
                    if (t != null && t == serializedType)
                    {
                        assemblyName = null;
                        typeName = serializedType.Name;
                        return;
                    }
                }
            }

            DefaultBinder.BindToName(serializedType, out assemblyName, out typeName);
        }
    }
}
