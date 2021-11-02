using Team_Capture.Core;
using Team_Capture.Settings.Controllers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;

namespace Team_Capture.Settings
{
    /// <summary>
    ///     Setups up a <see cref="Volume"/>.
    /// </summary>
    [CreateOnInit(ObjectNameOverride = "Volume")]
    internal partial class VolumeSetup : MonoBehaviour
    {
        private const string DefaultVolumeProfilePath = "Assets/Settings/Volume.asset";
        
        private void Awake()
        {
            //No point in doing volume stuff if we are the server
            if (Game.IsHeadless)
            {
                Destroy(gameObject);
                return;
            }
            
            Volume newVolume = gameObject.AddComponent<Volume>();
            VolumeProfile defaultProfile = Addressables.LoadAssetAsync<VolumeProfile>(DefaultVolumeProfilePath)
                .WaitForCompletion();
            newVolume.profile = defaultProfile;
            gameObject.AddComponent<VolumeSettingsController>();
            Destroy(this);
        }
    }
}