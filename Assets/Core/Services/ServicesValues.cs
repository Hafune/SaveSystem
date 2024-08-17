using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lib
{
    [Serializable]
    public class ServicesValues
    {
        [SerializeField] private List<string> _keys = new();
        [SerializeField] private List<string> _values = new();

        public T DeserializeData<T>(object key) where T : new()
        {
            try
            {
                var value = GetValue(key);
                var data = (value.Length == 0 ? new () : JsonUtility.FromJson<T>(value)) ?? new ();
                return data;
            }
            catch (Exception)
            {
                return new();
            }
        }

        public void SerializeData(object key, object data)
        {
            var json = JsonUtility.ToJson(data);
            SetValues(key, json);
        }
        
        private string GetValue(object key)
        {
            int index = _keys.IndexOf(key.GetType().FullName!);
            return index != -1 ? _values[index] : _values[index] = string.Empty;
        }

        private void SetValues(object key, string value)
        {
            int index = _keys.IndexOf(key.GetType().FullName!);

            if (index == -1)
            {
                index = _keys.Count;
                _keys.Add(key.GetType().FullName!);
                _values.Add(string.Empty);
            }
                
            _values[index] = value;
        }
    }
}