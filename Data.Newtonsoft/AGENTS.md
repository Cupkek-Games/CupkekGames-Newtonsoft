# CupkekGames.Data.Newtonsoft — AI Agent Instructions

## Package Overview

**CupkekGames.Data.Newtonsoft** is a bridge layer that connects the Data package's serialization interface (`IDataSerializer`) with the Newtonsoft (JSON.NET) serialization framework.

It provides:
- **IDataSerializer implementation** for JSON serialization via Newtonsoft.Json
- **ServiceLocator integration** via `DataSerializerRegistrar`
- **Automatic type name handling** for polymorphic serialization (e.g., `IFeature` lists)
- **Custom converters** for Unity types (SerializedGuid, Vector2Int, etc.)

The package is **bridging infrastructure**: you don't use it directly. Instead, register it in the ServiceLocator, and the Data system automatically uses Newtonsoft for all serialization.

## Why a Bridge?

### The Problem
- **Data package** defines `IDataSerializer` interface (pluggable)
- **Data package** doesn't include JSON library (keeps it lightweight)
- **Games need JSON** for saves, configuration, networking
- **Newtonsoft** is powerful but requires setup (converters, type binding, etc.)

### The Solution
- **Data.Newtonsoft** implements `IDataSerializer` using Newtonsoft
- **Wraps Newtonsoft's SerializationManager** (from main Newtonsoft package)
- **Register once, use everywhere** via ServiceLocator
- Game code stays decoupled from serialization choice

## Core Components

### 1. NewtonsoftDataSerializer
Implements `IDataSerializer` by delegating to Newtonsoft's `SerializationManager`:

```csharp
public class NewtonsoftDataSerializer : IDataSerializer
{
    private static SerializationManager Manager =>
        ServiceLocator.Get<CupkekGames.Newtonsoft.SerializationManager>();

    public string Serialize<T>(T data)
        => Manager.Serialize(data);

    public T Deserialize<T>(string json)
        => Manager.Deserialize<T>(json);

    public void Populate<T>(string json, T target)
        => JsonConvert.PopulateObject(json, target, Manager.Settings);

    public T Clone<T>(T source)
        => Deserialize<T>(Serialize(source));
}
```

**Methods:**
- **Serialize** → Convert object to JSON string
- **Deserialize** → Parse JSON string to typed object
- **Populate** → Merge JSON into existing object (for patching)
- **Clone** → Deep copy via serialize → deserialize roundtrip

### 2. DataSerializerRegistrar
ScriptableObject that registers `NewtonsoftDataSerializer` in ServiceLocator:

```csharp
[CreateAssetMenu(menuName = "CupkekGames/Data/Newtonsoft Serializer Registrar")]
public class DataSerializerRegistrar : ServiceProviderSO
{
    public override void RegisterServices()
    {
        ServiceLocator.Register(
            new NewtonsoftDataSerializer(), 
            typeof(IDataSerializer)
        );
    }

    public override void UnregisterServices()
    {
        ServiceLocator.Remove<IDataSerializer>();
    }
}
```

**Usage:**
1. Create SO in Editor: `Assets/Create/CupkekGames/Data/Newtonsoft Serializer Registrar`
2. Drag into ServiceLocator's registrar list or call `RegisterServices()` at startup
3. Data system automatically uses Newtonsoft

### 3. SerializedGuidConverter
Custom Newtonsoft converter for `SerializedGuid` (UUID type from Data package):

```csharp
public class SerializedGuidConverter : JsonConverter<SerializedGuid>
{
    public override void WriteJson(JsonWriter writer, SerializedGuid value, JsonSerializer serializer)
    {
        writer.WriteValue(value.Value.ToString());
    }

    public override SerializedGuid ReadJson(JsonReader reader, Type objectType, SerializedGuid existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string guidString = (string)reader.Value;
        return new SerializedGuid(System.Guid.Parse(guidString));
    }
}
```

**Why?** `System.Guid` doesn't serialize cleanly to JSON; this converter ensures clean, readable UUID strings.

## Package Structure

```
CupkekGames.Data.Newtonsoft/
  Runtime/
    CupkekGames.Data.Newtonsoft.asmdef
    NewtonsoftDataSerializer.cs       ← IDataSerializer implementation
    DataSerializerRegistrar.cs        ← Register in ServiceLocator
    SerializedGuidConverter.cs        ← Custom converter for SerializedGuid
    DataSerializationTypeProviderSO.cs ← Optional type registry
  ScriptableObjects/
    CupkekGames Data Serializer Registrar.asset  ← Pre-made registrar
```

## Usage Patterns

### Pattern 1: Basic Setup

```csharp
// In game startup
var registrar = Resources.Load<DataSerializerRegistrar>(
    "CupkekGames Data Serializer Registrar"
);
registrar.RegisterServices();

// Alternatively, load from preset SO
ServiceLocator.Initialize(presetRegistry);  // If using preset registry
```

Then, throughout your game:
```csharp
// Data package uses IDataSerializer automatically
var itemSerializer = ServiceLocator.Get<IDataSerializer>();
string json = itemSerializer.Serialize(myItem);
var loaded = itemSerializer.Deserialize<MyItemType>(json);
```

### Pattern 2: With DataSO (Data Package Integration)

```csharp
// Define item data
[Serializable]
public class ItemData : IData
{
    public string Name = "";
    [SerializeReference]
    public List<IFeature> Features = new();  // Polymorphic
    
    public bool Validate() => !string.IsNullOrEmpty(Name);
    public void OnAfterDeserialize() { }
}

// Wrap in SO
public class ItemDataSO : DataSO<ItemData> { }

// ... in editor ...
var itemSO = ScriptableObject.CreateInstance<ItemDataSO>();
itemSO.Data = new ItemData { Name = "Sword", Features = /* ... */ };

// Serialize to JSON (uses NewtonsoftDataSerializer)
string json = itemSO.ToJson();
Debug.Log(json);  // Fully expanded with polymorphic types

// Deserialize back
itemSO.LoadFromJson(json);
```

### Pattern 3: Save/Load with Polymorphic Data

```csharp
[Serializable]
public class GameSaveData : IGameSaveData, IData
{
    public GameSaveMetadata Metadata { get; set; }
    
    [SerializeReference]
    public List<IFeature> PlayerFeatures = new();  // E.g., armor, buffs
    
    public Dictionary<string, int> Inventory = new();  // Complex types need Newtonsoft
    
    public bool Validate() => Metadata != null && PlayerFeatures.Count > 0;
    public void OnAfterDeserialize() { }
    
    public void LoadFrom(IGameSaveData other, int slot) { /* ... */ }
    public GameSaveMetadata CreateMetadata(string version, bool isAutosave) { /* ... */ }
}

// Serialize to disk
var serializer = ServiceLocator.Get<IDataSerializer>();
string json = serializer.Serialize(gameSaveData);
File.WriteAllText("save.json", json);

// Load from disk
string json = File.ReadAllText("save.json");
var loaded = serializer.Deserialize<GameSaveData>(json);
```

Output JSON (Newtonsoft adds `$type` for polymorphic reconstruction):
```json
{
  "Metadata": { "SaveDate": "2025-07-19T...", "SaveVersion": "1.0.0", "IsAutosave": false },
  "PlayerFeatures": [
    { "$type": "EquipableFeature, ...", "Type": "Armor", "AttributeBonus": { /* ... */ } },
    { "$type": "BuffFeature, ...", "Stat": "Health", "Amount": 50 }
  ],
  "Inventory": {
    "Sword": 1,
    "Health Potion": 5
  }
}
```

## Integration Points

### Dependency Chain

```
ServiceLocator (CupkekGames.Systems)
    ↓
CupkekGames.Newtonsoft.SerializationManager (requires initialization)
    ↓
NewtonsoftDataSerializer (this package)
    ↓
IDataSerializer (Data package interface)
    ↓
DataSO<T>, GameSave, InventorySystem, etc.
```

### ServiceLocator Setup

Ensure ServiceLocator contains both:

```csharp
var serviceRegistry = ScriptableObject.CreateInstance<ServiceRegistry>();

// 1. Register SerializationManager from Newtonsoft package
var newtonSoftTypeProvider = Resources.Load<NewtonsoftDefaultTypeProviderSO>(...);
var serializationManager = new SerializationManager();
serializationManager.Initialize(
    newtonSoftTypeProvider.GetTypes(),  // Known types for [SerializeReference]
    newtonSoftTypeProvider.GetConverters(),
    newtonSoftTypeProvider.GetContractResolver()
);
serviceRegistry.Register(serializationManager, typeof(CupkekGames.Newtonsoft.SerializationManager));

// 2. Register DataSerializer
var dataSerializerRegistrar = Resources.Load<DataSerializerRegistrar>(...);
dataSerializerRegistrar.RegisterServices();  // Registers NewtonsoftDataSerializer

ServiceLocator.Initialize(serviceRegistry);
```

### With Newtonsoft Package

This package depends on:
- **CupkekGames.Newtonsoft** for `SerializationManager`
- **CupkekGames.Data** for `IDataSerializer`
- **CupkekGames.Systems** for `ServiceLocator`

The main Newtonsoft package provides:
- Converters for Unity types (Vector2Int, etc.)
- ContractResolvers for private setters
- TypeBinders for known type mapping
- Migration tools for format changes

## Coding Conventions

- **Register once at startup** → Don't create multiple serializers
- **Use [SerializeReference] for polymorphic types** → Newtonsoft handles `$type` mapping
- **IData + Newtonsoft = full serialization** → Data + complex types (Dictionary, etc.)
- **No direct Newtonsoft in game code** → Always use `IDataSerializer` interface
- **Custom converters go in Newtonsoft package** → Not in Data.Newtonsoft
- **SerializedGuid is handled** → Use it freely; converter included

## Common Tasks

### Task: Use Newtonsoft Instead of Default Serializer

1. Create `DataSerializerRegistrar` SO (or use preset)
2. Call `RegisterServices()` at game startup
3. Data system automatically uses Newtonsoft
4. No code changes needed in existing code using `IDataSerializer`

### Task: Add Custom Type to Serialization

In Newtonsoft package's `NewtonsoftDefaultTypeProviderSO`:
1. Enable/disable built-in converters (Vector2Int, GenericDictionary, etc.)
2. Add custom converters if needed
3. Enable private setter resolver for immutable-style data

### Task: Serialize [SerializeReference] Lists

Newtonsoft + `TypeNameHandling` automatically handles this:
```csharp
[Serializable]
public class MyData : IData
{
    [SerializeReference]
    public List<IFeature> Features = new();  // Just works!
}
```

The converter writes type info to JSON; deserialization reconstructs correctly.

### Task: Clone Deep Copy Data

```csharp
var serializer = ServiceLocator.Get<IDataSerializer>();
var deepCopy = serializer.Clone(originalData);
```

Preserves all polymorphic types, nested structures.

## Notes for AI Assistants

- **Transparent integration** → Once registered, users don't think about it; Data system "just works"
- **Type safety preserved** → No casting or reflection in user code; generics do the work
- **Polymorphic serialization automatic** → `[SerializeReference]` fields and Newtonsoft type info = seamless
- **ServiceLocator is required** → Can't access `SerializationManager` otherwise
- **Newtonsoft is optional** → Games can implement custom `IDataSerializer` if needed
- **No public API exposed** → Is entirely backend; users interact via `IDataSerializer`
