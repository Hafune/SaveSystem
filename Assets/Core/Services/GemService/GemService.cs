using System;
using System.Collections.Generic;
using Core.Services;
using Reflex;

namespace Core
{
    public class GemService : IInitializableService, ISerializableService
    {
        public event Action OnDataChange;
        public event Action OnCountChange;

        private ServiceData _serviceData = new();
        private PlayerDataService _playerDataService;

        public int Count => _serviceData.count;

        public void InitializeService(Context context)
        {
            _playerDataService = context.Resolve<PlayerDataService>();
            _playerDataService.RegisterService(this);
        }

        public void AddCollected(in string instanceUuid)
        {
            if (string.IsNullOrEmpty(instanceUuid))
                return;

            _serviceData.collectedGems.Add(instanceUuid);
            _serviceData.count++;
            OnCountChange?.Invoke();
            _playerDataService.SetDirty(this);
        }

        public bool IsGemCollected(in string instanceUuid) => _serviceData.collectedGems.Contains(instanceUuid);

        public void SaveData() => _playerDataService.SerializeData(this, _serviceData);

        public void ReloadData()
        {
            _serviceData = _playerDataService.DeserializeData<ServiceData>(this);
            OnCountChange?.Invoke();
            OnDataChange?.Invoke();
        }

        [Serializable]
        private class ServiceData
        {
            public int count;
            public List<string> collectedGems = new();
        }
    }
}