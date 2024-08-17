using System;
using System.Collections.Generic;
using Core.Services;
using Reflex;
using UnityEngine;

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

        public bool TryChangeValue(int value)
        {
            if (_serviceData.count + value < 0)
                return false;

            _serviceData.count += value;
            _playerDataService.SetDirty(this);
            OnCountChange?.Invoke();

            return true;
        }

        public void AddCollected(in string instanceUuid)
        {
            if (string.IsNullOrEmpty(instanceUuid))
                return;

            Debug.Log(instanceUuid);
            _serviceData.collectedGems.Add(instanceUuid);
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