using System;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;

namespace CupkekGames.Luna.Demo.Newtonsoft
{
    [Serializable]
    public class NewtonsoftDemoSaveMetadata : GameSaveMetadata
    {
        public int Gold;

        public NewtonsoftDemoSaveMetadata() : base()
        {
            SaveDate = DateTime.Now;
            SaveVersion = "-1";
            Gold = 0;
        }

        public NewtonsoftDemoSaveMetadata(NewtonsoftDemoSaveMetadata other) : base(other)
        {
            if (other == null)
                return;
            Gold = other.Gold;
        }
    }
}
