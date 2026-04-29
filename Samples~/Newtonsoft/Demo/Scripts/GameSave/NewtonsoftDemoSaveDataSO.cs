using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;
using UnityEngine;

namespace CupkekGames.Luna.Demo.Newtonsoft
{
    [CreateAssetMenu(fileName = "NewtonsoftDemoSaveDataSO",
        menuName = "CupkekGames/Samples/Newtonsoft/SaveDataSO")]
    public class NewtonsoftDemoSaveDataSO : GameSaveDataSO<NewtonsoftDemoSaveData>
    {
    }
}
