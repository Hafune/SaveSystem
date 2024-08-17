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

        public T DeserializeData<T>(object obj) where T : new()
        {
            try
            {
                var value = GetValue(obj);
                var data = (value.Length == 0 ? new () : JsonUtility.FromJson<T>(value)) ?? new ();
                return data;
            }
            catch (Exception)
            {
                return new();
            }
        }

        public void SerializeData(object obj, object data)
        {
            var json = JsonUtility.ToJson(data);
            SetBytes(obj, json);
        }
        
        private string GetValue(object obj)
        {
            int index = _keys.IndexOf(obj.GetType().FullName!);
            return index != -1 ? _values[index] : _values[index] = string.Empty;
        }

        private void SetBytes(object obj, string value)
        {
            int index = _keys.IndexOf(obj.GetType().FullName!);

            if (index == -1)
            {
                index = _keys.Count;
                _keys.Add(obj.GetType().FullName!);
                _values.Add(string.Empty);
            }
                
            _values[index] = value;
        }
    }
}