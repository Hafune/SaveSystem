using System;
using Lib;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
    public class GemInstance : MonoConstruct
    {
        [field: SerializeField] public string InstanceUuid { get; protected set; }
        private GemService _service;

        private void Awake() => _service = Context.Resolve<GemService>();

        private void OnEnable()
        {
            _service.OnDataChange += ReloadData;
            ReloadData();
        }

        private void OnDisable()
        {
            _service.OnDataChange -= ReloadData;
            InstanceUuid = string.Empty;
        }

        private void OnTriggerEnter(Collider other)
        {
            _service.AddCollected(InstanceUuid);
            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        [ContextMenu(nameof(RegenerateInstanceUuid))]
        public void RegenerateInstanceUuid()
        {
            InstanceUuid = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
#endif

        private void ReloadData()
        {
            if (_service.IsGemCollected(InstanceUuid))
                gameObject.SetActive(false);
        }
    }
}