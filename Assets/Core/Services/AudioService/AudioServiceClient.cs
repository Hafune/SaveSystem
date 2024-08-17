using Core.Services;
using Lib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class AudioServiceClient : MonoConstruct
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Slider _slider;
        private AudioService _service;

        private void Awake() => _service = Context.Resolve<AudioService>();

        private void OnEnable()
        {
            _service.OnMasterVolumeChanged += ReloadData;
            ReloadData();
        }
        
        private void OnDisable() => _service.OnMasterVolumeChanged -= ReloadData;
        
        public void OnChange(float value) => _service.SetMasterVolume(value);

        private void ReloadData()
        {
            _slider.SetValueWithoutNotify(_service.MasterVolume);
            _text.text = ((int)(_service.MasterVolume * 100)).ToString();
        }
    }
}