using System;
using System.Collections;
using UnityEngine;

namespace Core
{
    public class SdkService : MonoBehaviour
    {
        private Action<string> _onSuccessLoadPlayerData;
        private Action<string> _onErrorCallback;
        private float _lastSaveTime;
        private string _lastPlayerData;
        private const float _sdkCallDelay = 5;

        public void Initialize(Action callback) => StartCoroutine(InitializePrivate(callback));

        private IEnumerator InitializePrivate(Action callback)
        {
            IEnumerator enumerator = null;

            try
            {
#if SOME_SDK
                enumerator = SomeSdk.Initialize(callback);
#else
                callback();
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning("SDK INIT ERROR");
                Debug.LogWarning(e.Message);
                callback();
            }

            if (enumerator is not null)
                yield return enumerator;
        }

        public void LoadPlayerData(Action<string> onSuccess, Action<string> onError)
        {
            _onSuccessLoadPlayerData = onSuccess;

            try
            {
#if SOME_SDK
                SomeSdk.GetCloudData(Success, onError);
#else
                onError?.Invoke("No selected sdk defenition");
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning("CLOUD LOAD ERROR");
                Debug.LogWarning(e.Message);
                onError?.Invoke(e.Message);
            }
        }

        public void SavePlayerData(string data, Action onSuccess, Action<string> onError)
        {
            _lastPlayerData = data;

            if (Time.unscaledTime - _lastSaveTime < _sdkCallDelay)
            {
                onSuccess();
                return;
            }

            _lastSaveTime = Time.unscaledTime;

            try
            {
#if SOME_SDK
                SomeSdk.SetCloudData(data, onSuccess, onError);
#else
                onError("No selected sdk defenition");
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning("CLOUD SAVE ERROR");
                Debug.LogWarning(e.Message);
                onError(e.Message);
            }
        }

        private void Success(string playerData)
        {
            _lastPlayerData = playerData;
            _onSuccessLoadPlayerData(_lastPlayerData);
        }
    }
}