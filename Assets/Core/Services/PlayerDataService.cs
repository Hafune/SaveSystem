using System;
using System.Collections;
using System.Collections.Generic;
using Core.Services;
using Lib;
using UnityEngine;

namespace Core
{
    public class PlayerDataService : MonoConstruct
    {
        private Action InitializeCallback;
        private Action _forceSaveCallback;
        private Coroutine _awaitCoroutine;
        private SdkService _sdkService;
        private ServicesValues _servicesValues = new();
        private bool _saveInProgress;
        private const float _saveDelay = 1f;
        private const float _sdkSaveDelay = 5.25f;
        private const string key = nameof(PlayerDataService);
        private float _delay;
        private float _lastSaveTime;
        private readonly HashSet<ISerializableService> _dirtyServices = new();
        private readonly HashSet<ISerializableService> _ignoreResetServices = new();
        private readonly HashSet<ISerializableService> _lazySaveServices = new();
        private readonly HashSet<ISerializableService> _totalServices = new();

        private void Awake() => enabled = false;

        private void Update()
        {
            if (_delay - Time.unscaledTime > 0)
                return;

            Save();
        }

        public void Initialize(Action callback)
        {
            _lastSaveTime = _sdkSaveDelay;
            _sdkService = Context.Resolve<SdkService>();

            InitializeCallback = callback;
            _sdkService.LoadPlayerData(OnLoadSuccess, OnLoadError);
        }

        public void RegisterService(ISerializableService service, bool ignoreReset = false, bool lazySave = false)
        {
            _totalServices.Add(service);

            if (ignoreReset)
                _ignoreResetServices.Add(service);

            if (lazySave)
                _lazySaveServices.Add(service);
        }

        public void SetDirty(ISerializableService service)
        {
            _dirtyServices.Add(service);

            if (_lazySaveServices.Contains(service))
                return;

            _delay = Time.unscaledTime + _saveDelay;
            enabled = true;
        }

        public void Save(Action forceSaveCallback = null)
        {
            if (_saveInProgress)
            {
                forceSaveCallback?.Invoke();
                return;
            }

            _saveInProgress = true;
            _lastSaveTime = Time.unscaledTime;

            if (forceSaveCallback != null)
            {
                _forceSaveCallback = forceSaveCallback;
                SaveServicesData(SaveComplete);
                return;
            }
            
            float delay = _lastSaveTime - Time.unscaledTime + _sdkSaveDelay;

            if (delay > 0)
                _awaitCoroutine ??= StartCoroutine(AwaitBeforeSave(delay, SaveComplete));
            else
                SaveServicesData(SaveComplete);
        }

        private void SaveComplete()
        {
            _forceSaveCallback?.Invoke();
            _forceSaveCallback = null;
            _delay = Time.unscaledTime + _saveDelay;
            _saveInProgress = false;
            enabled = false;
        }

        public void Reset()
        {
            _servicesValues = new();

            foreach (var service in _ignoreResetServices)
                service.SaveData();

            foreach (var service in _totalServices)
                if (!_ignoreResetServices.Contains(service))
                    service.ReloadData();

            Save();
        }

        public void SerializeData(object obj, object data) => _servicesValues.SerializeData(obj, data);

        public T DeserializeData<T>(object obj) where T : new() => _servicesValues.DeserializeData<T>(obj);

        private void SaveServicesData(Action callback)
        {
            int saved = 0;

            foreach (var service in _dirtyServices)
            {
#if UNITY_EDITOR
                Debug.Log("SAVE " + service.GetType().Name);
#endif
                service.SaveData();
                saved++;
            }

            _dirtyServices.Clear();

            if (saved != 0)
                PrivateSave(callback);
            else
                callback?.Invoke();
        }

        private void PrivateSave(Action callback)
        {
            var values = JsonUtility.ToJson(_servicesValues);
            var data = JsonUtility.ToJson(new DataWrapper { data = values });

            _sdkService.SavePlayerData(data, () => OnSaveSuccess(callback),
                errorMessage => OnSaveError(errorMessage, data, callback));
        }

        private IEnumerator AwaitBeforeSave(float seconds, Action callback)
        {
            yield return new WaitForSecondsRealtime(seconds);
            SaveServicesData(callback);
            
            _awaitCoroutine = null;
        }

        private void OnLoadSuccess(string data = null)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var wrapper = JsonUtility.FromJson<DataWrapper>(data);
                try
                {
                    _servicesValues = JsonUtility.FromJson<ServicesValues>(wrapper.data);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    _servicesValues = new();
                }
            }

            _dirtyServices.Clear();

            foreach (var service in _totalServices)
                service.ReloadData();

            InitializeCallback.Invoke();
        }

        private void OnLoadError(string error)
        {
            Debug.LogWarning("OnLoadError");
            Debug.LogWarning(error);
            OnLoadSuccess(PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null);
        }

        private void OnSaveSuccess(Action callback)
        {
            callback?.Invoke();

            _lastSaveTime = Time.unscaledTime;
            Debug.Log("DATA SAVED");
        }

        private void OnSaveError(string errorMessage, string stringData, Action callback)
        {
            Debug.LogWarning(errorMessage);
            PlayerPrefs.SetString(key, stringData);
            PlayerPrefs.Save();
            OnSaveSuccess(callback);
        }

        [Serializable]
        private class DataWrapper
        {
            public string data;
        }
    }
}