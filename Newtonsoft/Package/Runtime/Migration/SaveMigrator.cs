using System;
using System.Collections.Generic;
using CupkekGames.Services;
using Newtonsoft.Json.Linq;

namespace CupkekGames.Newtonsoft
{
    public class SaveMigrator
    {
        private readonly Dictionary<int, Action<JObject>> _migrations;
        private readonly int _currentVersion;

        public SaveMigrator(int currentVersion, Dictionary<int, Action<JObject>> migrations)
        {
            _currentVersion = currentVersion;
            _migrations = migrations ?? throw new ArgumentNullException(nameof(migrations));
        }

        private static SerializationManager _manager;
        private static SerializationManager Manager => _manager ??= ServiceLocator.Get<SerializationManager>();

        public string Migrate(string jsonData)
        {
            JObject saveData = Manager.ParseToJObject(jsonData);
            int version = GetVersion(saveData);

            if (version > _currentVersion)
            {
                throw new InvalidOperationException(
                    $"Save data version ({version}) is newer than the supported version ({_currentVersion})"
                );
            }

            while (version < _currentVersion)
            {
                if (_migrations.TryGetValue(version, out var migration))
                {
                    try
                    {
                        migration(saveData);
                        version++;
                        SetVersion(saveData, version);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            $"Failed to migrate from version {version} to {version + 1}",
                            ex
                        );
                    }
                }
                else
                {
                    throw new Exception($"Missing migration path from version {version}");
                }
            }

            return saveData.ToString();
        }

        public T Migrate<T>(string jsonData)
        {
            string migratedJson = Migrate(jsonData);
            return Manager.Deserialize<T>(migratedJson);
        }

        private int GetVersion(JObject saveData)
        {
            // Try to get version from metadata first
            var version = saveData["metadata"]?["version"]?.Value<int>();
            if (version != null)
                return version.Value;

            // Fallback to root-level version if exists
            version = saveData["version"]?.Value<int>();
            if (version != null)
                return version.Value;

            // Default to version 1 if no version found
            return 1;
        }

        private void SetVersion(JObject saveData, int version)
        {
            // Try to set version in metadata first
            var metadata = saveData["metadata"];
            if (metadata != null)
            {
                metadata["version"] = version;
                return;
            }

            // Fallback to setting version at root level
            saveData["version"] = version;
        }
    }
}