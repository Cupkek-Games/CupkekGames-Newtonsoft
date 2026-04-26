using System.Collections.Generic;
using System.Text;
using CupkekGames.Systems;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Luna.Demo.Newtonsoft
{
    /// <summary>
    /// Newtonsoft JSON save/load demo. Minimal save shape (PlayerName / Gold / PlayCount) — focuses
    /// on demonstrating Newtonsoft serialization through the GameSave system without inventory/system coupling.
    /// </summary>
    public class NewtonsoftDemo : UIViewComponent
    {
        private NewtonsoftDemoSaveManager _saveManager;
        private Button _save;
        private Button _load;
        private Label _current;
        private Label _saved;
        private int _nextSaveSlot = 0;

        // Data edit buttons
        private Button _addGold;
        private Button _removeGold;
        private Button _incrementPlayCount;
        private Button _resetPlayCount;

        protected override void Awake()
        {
            base.Awake();

            _saveManager = ServiceLocator.Get<NewtonsoftDemoSaveManager>();

            _save = ParentElement.Q<Button>("Save");
            _save.text = "Save to slot " + _nextSaveSlot;
            _load = ParentElement.Q<Button>("Load");
            _load.text = "Load latest save file";

            _save.clicked += OnSave;
            _load.clicked += OnLoad;

            _current = ParentElement.Q<Label>("Current");
            _saved = ParentElement.Q<Label>("Saved");

            _addGold = ParentElement.Q<Button>("AddGold");
            _removeGold = ParentElement.Q<Button>("RemoveGold");
            _incrementPlayCount = ParentElement.Q<Button>("AddRandomItem");      // reused UXML button id
            _resetPlayCount = ParentElement.Q<Button>("RemoveRandomItem");       // reused UXML button id

            if (_addGold != null) _addGold.clicked += OnAddGold;
            if (_removeGold != null) _removeGold.clicked += OnRemoveGold;
            if (_incrementPlayCount != null) _incrementPlayCount.clicked += OnIncrementPlayCount;
            if (_resetPlayCount != null) _resetPlayCount.clicked += OnResetPlayCount;
        }

        private void Start()
        {
            UpdateCurrentData();
            UpdateSavedData();
        }

        private string[] GetSaveInfo(NewtonsoftDemoSaveData save)
        {
            return new[]
            {
                "Player Name: " + save.PlayerName,
                "Gold: " + save.Gold,
                "Play Count: " + save.PlayCount
            };
        }

        private string BuildLabelText(string[] values)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                sb.Append(values[i]);
                if (i < values.Length - 1)
                    sb.AppendLine();
            }
            return sb.ToString();
        }

        private void OnSave()
        {
            NewtonsoftDemoSaveData current = _saveManager.CurrentSave.Data;
            _saveManager.SaveToFile(_nextSaveSlot, current);
            _nextSaveSlot++;
            _save.text = "Save to slot " + _nextSaveSlot;

            UpdateSavedData();
        }

        private void OnLoad()
        {
            GameSaveMetadataWithSlot<NewtonsoftDemoSaveMetadata> metadata = _saveManager.GetLastMetadata();
            if (metadata.Metadata != null)
            {
                _saveManager.CurrentSave.Data = _saveManager.GetSave(metadata.SaveSlot);
                UpdateCurrentData();
            }
        }

        private void UpdateCurrentData()
        {
            _current.text = "Current Data: \n" + BuildLabelText(GetSaveInfo(_saveManager.CurrentSave.Data));
        }

        private void UpdateSavedData()
        {
            List<GameSaveMetadataWithSlot<NewtonsoftDemoSaveMetadata>> metadata = _saveManager.GetAllMetadata(true);
            var saveInfo = new List<string>
            {
                "Save File Amount: " + metadata.Count
            };

            if (metadata.Count > 0)
            {
                saveInfo.Add("Last Save Date: " + metadata[0].Metadata.SaveDate);
                saveInfo.Add("Last Save Slot: " + metadata[0].SaveSlot);
                saveInfo.Add("Last Save Data: \n" + BuildLabelText(GetSaveInfo(_saveManager.GetSave(metadata[0].SaveSlot))));
            }

            _saved.text = BuildLabelText(saveInfo.ToArray());
            if (metadata.Count > 0)
                _load.text = "Load last save file: " + metadata[0].SaveSlot;
        }

        private void OnDestroy()
        {
            if (_save != null) _save.clicked -= OnSave;
            if (_load != null) _load.clicked -= OnLoad;
        }

        private void OnAddGold()
        {
            _saveManager.CurrentSave.Data.Gold += 100;
            UpdateCurrentData();
        }

        private void OnRemoveGold()
        {
            NewtonsoftDemoSaveData current = _saveManager.CurrentSave.Data;
            current.Gold = Mathf.Max(0, current.Gold - 100);
            UpdateCurrentData();
        }

        private void OnIncrementPlayCount()
        {
            _saveManager.CurrentSave.Data.PlayCount += 1;
            UpdateCurrentData();
        }

        private void OnResetPlayCount()
        {
            _saveManager.CurrentSave.Data.PlayCount = 0;
            UpdateCurrentData();
        }
    }
}
