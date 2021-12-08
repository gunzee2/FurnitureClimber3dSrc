using Cameras;
using Players;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;

namespace Settings
{
    public class SettingsApplier : MonoBehaviour
    {
        [SerializeField] private Volume _mainVolume;
        [SerializeField] private Volume _photoModeVolume;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private CameraMover _cameraMover;

    
        // Start is called before the first frame update
        void Start()
        {
            MessageBroker.Default.Receive<SettingsData>()
                .Subscribe(x =>
                {
                    ApplySettings(x);
                    ES3.Save<SettingsData>("settingsData", x);
                }).AddTo(this);
        
            if (!ES3.KeyExists("settingsData")) return;
        
            var settingsData = ES3.Load<SettingsData>("settingsData");
            MessageBroker.Default.Publish(settingsData);
        }

        private void ApplySettings(SettingsData settingsData)
        {
            _mainVolume.enabled = settingsData.isApplyPostEffect;
            _photoModeVolume.enabled = settingsData.isApplyPostEffect;

            _cameraMover.IsReverseY = settingsData.isReverseY;
            _cameraMover.IsReverseX = settingsData.isReverseX;
        }

    }
}