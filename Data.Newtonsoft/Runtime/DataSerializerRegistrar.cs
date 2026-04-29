using CupkekGames.Services;
using UnityEngine;

namespace CupkekGames.Data.Newtonsoft
{
    [CreateAssetMenu(menuName = "CupkekGames/Data/Newtonsoft/Serializer Registrar")]
    public class DataSerializerRegistrar : ServiceProviderSO
    {
        public override void RegisterServices()
        {
            ServiceLocator.Register(new NewtonsoftDataSerializer(), typeof(IDataSerializer));
        }

        public override void UnregisterServices()
        {
            ServiceLocator.Remove<IDataSerializer>();
        }
    }
}
