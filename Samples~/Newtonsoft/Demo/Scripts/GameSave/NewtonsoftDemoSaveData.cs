using System;
using CupkekGames.Data;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;
using Newtonsoft.Json;
using UnityEngine;

namespace CupkekGames.Luna.Demo.Newtonsoft
{
    /// <summary>
    /// Minimal save data shape used by the Newtonsoft demo to showcase JSON serialization
    /// through the GameSave system. Intentionally simple — no inventory / no system coupling.
    /// </summary>
    [Serializable]
    public class NewtonsoftDemoSaveData : IGameSaveData, IData
    {
        [JsonProperty(Order = -100)] public GameSaveMetadata Metadata { get; set; }

        public string PlayerName;
        public int Gold;
        public int PlayCount;

        public NewtonsoftDemoSaveData()
        {
            PlayerName = "Player";
            Gold = 0;
            PlayCount = 0;
        }

        public NewtonsoftDemoSaveData(NewtonsoftDemoSaveData other)
        {
            if (other == null)
                return;
            PlayerName = other.PlayerName;
            Gold = other.Gold;
            PlayCount = other.PlayCount;
            if (other.Metadata is NewtonsoftDemoSaveMetadata metaEx)
                Metadata = new NewtonsoftDemoSaveMetadata(metaEx);
            else if (other.Metadata != null)
                Metadata = new GameSaveMetadata(other.Metadata);
        }

        public void OnNewSaveCreated() { }

        public GameSaveMetadata CreateMetadata(string saveVersion, bool isAutosave)
        {
            return new NewtonsoftDemoSaveMetadata
            {
                SaveVersion = saveVersion,
                SaveDate = DateTime.Now,
                IsAutosave = isAutosave,
                Gold = Gold
            };
        }

        public void LoadFrom(IGameSaveData other, int saveSlot)
        {
            if (other is not NewtonsoftDemoSaveData loaded)
            {
                Debug.LogError("NewtonsoftDemoSaveData.LoadFrom: incompatible source data.");
                return;
            }

            PlayerName = loaded.PlayerName;
            Gold = loaded.Gold;
            PlayCount = loaded.PlayCount;
            Metadata = loaded.Metadata;
        }

        public bool Validate() => true;

        public void OnAfterDeserialize() { }

        public IData CloneData() => new NewtonsoftDemoSaveData(this);
    }
}
