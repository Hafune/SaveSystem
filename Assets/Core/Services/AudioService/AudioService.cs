using System;
using Reflex;
using UnityEngine;

namespace Core.Services
{
    public class AudioService : IInitializableService, ISerializableService
    {
        public event Action OnMasterVolumeChanged;

        public float MasterVolume => _serviceData.masterVolume;
        
        private PlayerDataService _playerDataService;
        private ServiceData _serviceData = new();
        
        public void InitializeService(Context context)
        {
            _playerDataService = context.Resolve<PlayerDataService>();
            _playerDataService.RegisterService(this, true);
        }

        public void SaveData() => _playerDataService.SerializeData(this, _serviceData);

        public void ReloadData() => _serviceData = _playerDataService.DeserializeData<ServiceData>(this);
        
        public void SetMasterVolume(float percent)
        {
            percent = Math.Clamp(percent, 0, 1);
            AudioListener.volume = percent;
            _serviceData.masterVolume = percent;
            _playerDataService.SetDirty(this);
            OnMasterVolumeChanged?.Invoke();
        }

        private class ServiceData
        {
            public float masterVolume = .5f;
        }
    }
}