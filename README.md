# CupkekGames Newtonsoft

Newtonsoft-Json serialization layer for CupkekGames packages. Includes a `CupkekGames.Data ↔ Newtonsoft` adapter so `IData` / `DataSO` instances serialize cleanly.

## What's inside

**Runtime** (`CupkekGames.Newtonsoft.asmdef`)

- `SerializationManager` — central façade for JSON serialize/deserialize
- Contract resolvers, custom converters, type providers, known-types binders
- `KnownTypesBinder`, `PrivateSettersContractResolver`, `GenericDictionaryConverter`, `SerializedGuidConverter`, etc.

**Data adapter** (`CupkekGames.Data.Newtonsoft.asmdef`)

- `NewtonsoftDataSerializer` — implements `IDataSerializer` from `com.cupkekgames.data`
- `DataSerializerRegistrar` — service registration helper
- `DataSerializationTypeProviderSO` — known-types ScriptableObject

**Editor** (`CupkekGames.Newtonsoft.Editor.asmdef`)

- `OpenPersistentDataPathMenuItem` — quick-access menu item for the save folder

## Dependencies

- `com.unity.nuget.newtonsoft-json` (hard dep)
- `com.cupkekgames.data` (for the Data adapter assembly)

## Sample

`Samples~/Newtonsoft/Demo` — save/load demo using Newtonsoft serialization with the GameSave system.
