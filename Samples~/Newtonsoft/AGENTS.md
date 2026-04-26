# CupkekGames.Newtonsoft — AI Agent Instructions

## Package Overview

**CupkekGames.Newtonsoft** is a comprehensive JSON serialization framework for Unity built on Newtonsoft.Json (JSON.NET). It provides:
- **SerializationManager** — Centralized configuration for JSON serialization settings
- **Custom Converters** — Vector2Int, GenericDictionary, and extensible user converters
- **Contract Resolvers** — PrivateSetterResolver for immutable-style data
- **Type Binding** — KnownTypesBinder for polymorphic type reconstruction
- **Migration Tools** — Schema versioning and format upgrades
- **Editor Tools** — Open persistent data path, serialize/deserialize utilities

The framework is **configuration-first**: define serialization rules once (converters, type providers, contract resolvers), and all `SerializationManager` instances share identical behavior.

## Core Concepts

### 1. SerializationManager
Central hub for all Newtonsoft settings:

```csharp
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

    public string Serialize<T>(T data)
    public T Deserialize<T>(string json)
    public JObject ParseToJObject(string json)
    public T ConvertJObjectTo<T>(JObject jObject)
}
```

**Key settings:**
- **TypeNameHandling** → How to embed type info in JSON (`Auto`, `Objects`, `All`)
- **ReferenceLoopHandling** → Circular reference strategy (`Ignore`, `Error`, `Serialize`)
- **Formatting** → `Indented` for readability, `None` for compact
- **KnownTypes** → Required types for `[SerializeReference]` and polymorphic fields
- **Converters** → Custom serialization logic for specific types
- **ContractResolver** → Field/property naming, readonly/private handling

### 2. Built-in Converters

#### Vector2IntConverter
Serializes `Vector2Int` as clean JSON:
```json
{
  "MyVector": { "x": 10, "y": 20 }
}
```

Instead of default Newtonsoft behavior (nested serialization).

#### GenericDictionaryConverter
Handles `Dictionary<TKey, TValue>` with non-string keys:
```csharp
public Dictionary<int, string> LevelData = new() { { 1, "Forest" }, { 2, "Cave" } };
```

Serializes as array of key-value pairs (JSON has no native dictionary with non-string keys).

#### Custom Converters
Extend `JsonConverter<T>` for domain-specific types:
```csharp
public class ColorConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        writer.WriteValue($"#{ColorUtility.ToHtmlStringRGBA(value)}");
    }

    public override Color ReadJson(JsonReader reader, Type type, Color existing, bool has, JsonSerializer serializer)
    {
        string hex = (string)reader.Value;
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }
}
```

### 3. Contract Resolvers

#### PrivateSetterContractResolver
Allows deserialization into properties with private setters (immutable-style data):

```csharp
public class Item : IData
{
    public string Name { get; private set; }
    public int Rarity { get; private set; }
    
    // Newtonsoft can populate these even though setters are private
    public Item() { }
    public Item(string name, int rarity)
    {
        Name = name;
        Rarity = rarity;
    }
}
```

This resolver enables polymorphic deserialization into readonly fields.

### 4. Type Binding (KnownTypesBinder)

Maps type names in JSON to actual .NET types:

```csharp
var knownTypes = new List<Type>
{
    typeof(EquipableFeature),
    typeof(PotionFeature),
    typeof(BuffFeature),
    // ...
};

var settings = new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Objects,
    SerializationBinder = new KnownTypesBinder { KnownTypes = knownTypes }
};
```

When deserializing, `{ "$type": "EquipableFeature", ... }` is mapped back to correct type.

### 5. SerializationTypeProviderSO
ScriptableObject base for configuring all serialization settings:

```csharp
public abstract class SerializationTypeProviderSO : ScriptableObject
{
    public virtual IList<Type> GetTypes() => new List<Type>();
    public virtual IList<JsonConverter> GetConverters() => new List<JsonConverter>();
    public virtual IContractResolver GetContractResolver() => null;
}
```

**Example (NewtonsoftDefaultTypeProviderSO):**
```csharp
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
```

### 6. SerializationManagerRegistrar
Registers `SerializationManager` in ServiceLocator:

```csharp
[CreateAssetMenu(menuName = "CupkekGames/Newtonsoft/Serialization Manager Registrar")]
public class SerializationManagerRegistrar : ServiceProviderSO
{
    [SerializeField] private SerializationTypeProviderSO _typeProvider;
    
    public override void RegisterServices()
    {
        var manager = new SerializationManager();
        manager.Initialize(
            _typeProvider.GetTypes(),
            _typeProvider.GetConverters(),
            _typeProvider.GetContractResolver()
        );
        ServiceLocator.Register(manager, typeof(SerializationManager));
    }
}
```

## Package Structure

```
CupkekGames.Newtonsoft/
  Runtime/
    CupkekGames.Newtonsoft.asmdef
    SerializationManager.cs              ← Core configuration hub
    SerializationTypeProviderSO.cs       ← Abstract config SO
    SerializationManagerRegistrar.cs     ← ServiceLocator registration
    NewtonsoftDefaultTypeProviderSO.cs   ← Default converters & resolver
    Converters/
      Vector2IntConverter.cs             ← Custom Vector2Int serialization
      GenericDictionaryConverter.cs      ← Non-string key dictionary support
      ← Add custom converters here →
    ContractResolvers/
      PrivateSetterContractResolver.cs   ← Immutable-style data support
    Binders/
      KnownTypesBinder.cs                ← Type name mapping for polymorphism
    Internal/
      ← Implementation details →
    Migration/
      ← Schema versioning & format upgrades →
  Editor/
    CupkekGames.Newtonsoft.Editor.asmdef
    OpenPersistentDataPathMenuItem.cs    ← Editor utility
```

## Usage Patterns

### Pattern 1: Basic Initialization

```csharp
// Create SerializationManager with default settings
var typeProvider = Resources.Load<NewtonsoftDefaultTypeProviderSO>("DefaultTypeProvider");
var manager = new SerializationManager();
manager.Initialize(
    typeProvider.GetTypes(),
    typeProvider.GetConverters(),
    typeProvider.GetContractResolver()
);

// Register in ServiceLocator
ServiceLocator.Register(manager, typeof(SerializationManager));

// Now all serialization uses these settings
var json = JsonConvert.SerializeObject(myObject, manager.Settings);
```

### Pattern 2: With ServiceLocator Registration

```csharp
// 1. Create TypeProvider SO
var typeProvider = ScriptableObject.CreateInstance<NewtonsoftDefaultTypeProviderSO>();
typeProvider._vector2IntConverter = true;
typeProvider._genericDictionaryConverter = true;
typeProvider._privateSetterResolver = true;

// 2. Create Registrar SO
var registrar = ScriptableObject.CreateInstance<SerializationManagerRegistrar>();
registrar._typeProvider = typeProvider;

// 3. Add to ServiceRegistry
var registry = ScriptableObject.CreateInstance<ServiceRegistry>();
registry.AddProvider(registrar);

// 4. Initialize ServiceLocator
ServiceLocator.Initialize(registry);

// 5. Use throughout game
var manager = ServiceLocator.Get<SerializationManager>();
```

### Pattern 3: With Polymorphic Data ([SerializeReference])

```csharp
// Define feature types
public interface IFeature { }
public class AttackFeature : IFeature { public int Damage; }
public class ArmorFeature : IFeature { public int Defense; }

// Data uses polymorphism
[Serializable]
public class ItemData : IData
{
    public string Name;
    [SerializeReference]
    public List<IFeature> Features = new();
}

// Configure TypeProvider to include all feature types
public class GameTypeProviderSO : SerializationTypeProviderSO
{
    public override IList<Type> GetTypes() => new List<Type>
    {
        typeof(AttackFeature),
        typeof(ArmorFeature),
        // Add all feature types
    };
}

// Serialize with type info
var itemData = new ItemData
{
    Name = "Sword",
    Features = new List<IFeature> { new AttackFeature { Damage = 10 } }
};

var manager = ServiceLocator.Get<SerializationManager>();
string json = manager.Serialize(itemData);

// JSON includes type info:
// {
//   "Name": "Sword",
//   "Features": [
//     { "$type": "AttackFeature", "Damage": 10 }
//   ]
// }

// Deserialize automatically reconstructs correct types
var loaded = manager.Deserialize<ItemData>(json);
Debug.Log($"Feature type: {loaded.Features[0].GetType()}");  // AttackFeature
```

### Pattern 4: Custom Converter for Domain Type

```csharp
// Define converter
public class DamageTypeConverter : JsonConverter<DamageType>
{
    public override void WriteJson(JsonWriter writer, DamageType value, JsonSerializer serializer)
    {
        writer.WriteValue(value.Name);
    }

    public override DamageType ReadJson(JsonReader reader, Type type, DamageType existing, bool has, JsonSerializer serializer)
    {
        string name = (string)reader.Value;
        return DamageType.GetByName(name);
    }
}

// Create custom TypeProvider
public class GameTypeProviderSO : SerializationTypeProviderSO
{
    public override IList<JsonConverter> GetConverters()
    {
        return new List<JsonConverter>
        {
            new DamageTypeConverter(),
            new Vector2IntConverter(),
            new GenericDictionaryConverter()
        };
    }
}

// Register and use
var provider = Resources.Load<GameTypeProviderSO>("GameTypeProvider");
var manager = new SerializationManager();
manager.Initialize(provider.GetTypes(), provider.GetConverters(), provider.GetContractResolver());
```

### Pattern 5: Schema Migration (Old to New Format)

```csharp
public class MigrationTypeProviderSO : SerializationTypeProviderSO
{
    public override IList<JsonConverter> GetConverters()
    {
        return new List<JsonConverter>
        {
            new ItemDataMigrationConverter()  // Custom converter that handles versioning
        };
    }
}

public class ItemDataMigrationConverter : JsonConverter<ItemData>
{
    public override void WriteJson(JsonWriter writer, ItemData value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }

    public override ItemData ReadJson(JsonReader reader, Type type, ItemData existing, bool has, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        
        // Check version
        string version = obj["Version"]?.Value<string>() ?? "1.0";
        
        if (version == "1.0")
        {
            // Old format: Rarity as int
            int rarityInt = obj["Rarity"]?.Value<int>() ?? 0;
            obj["Rarity"] = ((Rarity)rarityInt).ToString();
        }
        
        // Now deserialize with migrated format
        return obj.ToObject<ItemData>(JsonSerializer.Create(this));
    }
}
```

## Integration Points

### ServiceLocator Dependency

All serialization depends on `SerializationManager` in ServiceLocator:

```csharp
public class SerializationManager
{
    public string Serialize<T>(T data) { /* Uses JsonConvert with this.Settings */ }
}
```

**Setup order:**
1. Create `SerializationTypeProviderSO` (converters, types, resolvers)
2. Create `SerializationManagerRegistrar` pointing to provider
3. Add registrar to `ServiceRegistry`
4. Initialize `ServiceLocator`

### With Data.Newtonsoft

Data.Newtonsoft uses `SerializationManager` to implement `IDataSerializer`:

```
ServiceLocator
    ↓
SerializationManager (this package)
    ↓
NewtonsoftDataSerializer (Data.Newtonsoft)
    ↓
IDataSerializer (Data package)
```

### With GameSave System

GameSave uses `IDataSerializer` (which delegates to Newtonsoft):

```csharp
public override GameSaveData LoadFromFile(string fileName)
{
    var serializer = ServiceLocator.Get<IDataSerializer>();
    return serializer.Deserialize<GameSaveData>(File.ReadAllText(fileName));
}
```

## Coding Conventions

- **One SerializationManager per app** → Register in ServiceLocator, reuse everywhere
- **TypeNameHandling.Auto** → Includes type info only for polymorphic fields
- **Known types required** → All types used in `[SerializeReference]` must be in type provider
- **Custom converters for domain types** → Not Unity types (those are built-in)
- **PrivateSetterResolver for immutable data** → Enables deserialization into readonly properties
- **Circular references ignored** → Default behavior; change if deep nesting needed
- **Indented formatting for debugging** → Use `None` for production saves

## Common Tasks

### Task: Add Custom Converter

1. Extend `JsonConverter<T>`
2. Implement `WriteJson()` and `ReadJson()`
3. Add to custom `SerializationTypeProviderSO`
4. Register with manager

### Task: Support Dictionary with Custom Key Type

Use `GenericDictionaryConverter` (included), or create custom:

```csharp
public class DamageTypeDictionaryConverter : JsonConverter<Dictionary<DamageType, float>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<DamageType, float> value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        foreach (var kvp in value)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("key");
            writer.WriteValue(kvp.Key.Name);
            writer.WritePropertyName("value");
            writer.WriteValue(kvp.Value);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
    
    public override Dictionary<DamageType, float> ReadJson(JsonReader reader, Type type, Dictionary<DamageType, float> existing, bool has, JsonSerializer serializer)
    {
        JArray array = JArray.Load(reader);
        var result = new Dictionary<DamageType, float>();
        foreach (var item in array)
        {
            string key = item["key"]?.Value<string>();
            float value = item["value"]?.Value<float>() ?? 0;
            result[DamageType.GetByName(key)] = value;
        }
        return result;
    }
}
```

### Task: Migrate Old Save Format

1. Detect version in `ReadJson()`
2. Transform old fields to new names
3. Return migrated object

(See Pattern 5 above)

### Task: Serialize to JObject for Inspection

```csharp
var manager = ServiceLocator.Get<SerializationManager>();
string json = manager.Serialize(myData);
JObject jObject = manager.ParseToJObject(json);

// Inspect/modify
var name = jObject["Name"]?.Value<string>();
jObject["NewField"] = "value";

// Convert back
var migrated = manager.ConvertJObjectTo<MyData>(jObject);
```

## Notes for AI Assistants

- **Configuration is immutable** → Once manager initialized, settings don't change
- **Known types are required** → Forgetting a type causes "could not find type" error on deserialization
- **Type info adds overhead** → Only use `TypeNameHandling.Objects` or `All` if polymorphism needed
- **Converters are extensible** → Users define domain-specific serialization
- **ServiceLocator is required** → No static access to manager; must be registered
- **Circular references are safe** → Default `ReferenceLoopHandling.Ignore` prevents infinite loops
- **No public JSON schema** → Settings are internal; surface only via TypeProvider SOs
