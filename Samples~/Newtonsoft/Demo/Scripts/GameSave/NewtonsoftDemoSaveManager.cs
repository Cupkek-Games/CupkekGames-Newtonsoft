using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CupkekGames.Systems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CupkekGames.Luna.Demo.Newtonsoft
{
    /// <summary>
    /// Minimal save manager used by the Newtonsoft demo to showcase JSON file persistence
    /// through the GameSave system.
    /// </summary>
    [CreateAssetMenu(fileName = "NewtonsoftDemoSaveManager",
        menuName = "CupkekGames/Samples/Newtonsoft/SaveManager")]
    public class NewtonsoftDemoSaveManager : GameSaveManager<NewtonsoftDemoSaveData, NewtonsoftDemoSaveMetadata>
    {
        public const string SUBFOLDER = "saves_newtonsoft_demo";
        public const string FILE_EXTENSION = "json";
        public const int SAVE_VERSION = 1;

        private string SaveDirectory => Path.Combine(Application.persistentDataPath, SUBFOLDER);

        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);
        }

        protected override List<string> GetAllFileNames()
        {
            try
            {
                EnsureSaveDirectoryExists();
                return Directory.GetFiles(SaveDirectory, $"*.{FILE_EXTENSION}")
                    .Select(Path.GetFileName)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.LogError($"NewtonsoftDemoSaveManager: error listing save files: {ex.Message}");
                return new List<string>();
            }
        }

        protected override NewtonsoftDemoSaveData GetNewSave(string saveVersion)
        {
            var save = new NewtonsoftDemoSaveData();
            save.Metadata = save.CreateMetadata(saveVersion, false);
            save.OnNewSaveCreated();
            return save;
        }

        protected override void OnDeleteRequest(int saveSlot, string fileName)
        {
            try
            {
                string fullPath = Path.Combine(SaveDirectory, fileName);
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"NewtonsoftDemoSaveManager: error deleting {fileName}: {ex.Message}");
            }
        }

        protected override void OnSaveRequest(int saveSlot, string fileName, NewtonsoftDemoSaveData data, bool autosave)
        {
            try
            {
                if (autosave) GameSaveEvents.AutosaveStart?.Invoke();

                EnsureSaveDirectoryExists();

                data.Metadata = data.CreateMetadata(GetSaveVersion(), autosave);
                if (data.Metadata is NewtonsoftDemoSaveMetadata meta)
                    meta.Gold = data.Gold;

                string fullPath = Path.Combine(SaveDirectory, fileName);
                string json = ServiceLocator.Get<CupkekGames.Newtonsoft.SerializationManager>().Serialize(data);
                File.WriteAllText(fullPath, json);

                if (autosave) GameSaveEvents.AutosaveComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"NewtonsoftDemoSaveManager: error saving {fileName}: {ex.Message}");
                throw;
            }
        }

        protected override string GetFileExtenstion() => FILE_EXTENSION;

        protected override NewtonsoftDemoSaveData LoadFromFile(string fileName)
        {
            string fullPath = Path.Combine(SaveDirectory, fileName);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Save file not found: {fileName}");

            string json = File.ReadAllText(fullPath);
            return ServiceLocator.Get<CupkekGames.Newtonsoft.SerializationManager>().Deserialize<NewtonsoftDemoSaveData>(json);
        }

        protected override NewtonsoftDemoSaveMetadata LoadMetadataFromFile(string fileName)
        {
            string fullPath = Path.Combine(SaveDirectory, fileName);
            if (!File.Exists(fullPath))
                return null;

            try
            {
                using var streamReader = File.OpenText(fullPath);
                using var jsonReader = new JsonTextReader(streamReader);

                jsonReader.Read(); // StartObject
                jsonReader.Read(); // PropertyName

                if (jsonReader.TokenType == JsonToken.PropertyName &&
                    jsonReader.Value?.ToString() == "Metadata")
                {
                    jsonReader.Read();
                    var metaJson = JObject.Load(jsonReader);
                    return ServiceLocator.Get<CupkekGames.Newtonsoft.SerializationManager>()
                        .Deserialize<NewtonsoftDemoSaveMetadata>(metaJson.ToString());
                }

                string json = File.ReadAllText(fullPath);
                var jsonObject = JObject.Parse(json);
                var metadataJson = jsonObject["Metadata"];
                return metadataJson == null
                    ? null
                    : ServiceLocator.Get<CupkekGames.Newtonsoft.SerializationManager>()
                        .Deserialize<NewtonsoftDemoSaveMetadata>(metadataJson.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError($"NewtonsoftDemoSaveManager: error reading metadata for {fileName}: {ex.Message}");
                return null;
            }
        }

        protected override string GetSaveVersion() => SAVE_VERSION.ToString();
    }
}
