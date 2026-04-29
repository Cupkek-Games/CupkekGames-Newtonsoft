using CupkekGames.Newtonsoft;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Luna.Demo.Newtonsoft
{
    [CreateAssetMenu(menuName = "CupkekGames/Samples/Newtonsoft/Serialization Type Provider")]
    public class DemoSerializationTypesSO : SerializationTypeProviderSO
    {
        public override IList<Type> GetKnownTypes()
        {
            return new Type[]
            {
                typeof(GameSaveMetadata),
                typeof(NewtonsoftDemoSaveMetadata),
                typeof(NewtonsoftDemoSaveData),
            };
        }
    }
}
