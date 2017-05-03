using System;
using System.Collections;
using System.Collections.Generic;
namespace UnityEngine
{
    [Serializable]
    public class JsonArray<T>
    {
        [SerializeField]
        private List<T> value;
        public List<T> Value { get { return value; } }

        public JsonArray(List<T> list)
        {
            value = list;
        }

        public JsonArray(T[] arr)
        {
            value = new List<T>(arr);
        }

        public JsonArray(IEnumerable<T> arr)
        {
            value = new List<T>(arr);
        }
    }

    [Serializable]
    public class JsonDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys;
        [SerializeField]
        private List<TValue> values;

        private Dictionary<TKey, TValue> dictionary;
        public Dictionary<TKey, TValue> Dictionary { get { return dictionary; } }

        public JsonDictionary(Dictionary<TKey, TValue> dict)
        {
            dictionary = dict;
        }

        public void OnAfterDeserialize()
        {
            dictionary = new Dictionary<TKey, TValue>();
            for (var i = 0; i < Math.Min(keys.Count, values.Count); i++)
            {
                dictionary.Add(keys[i], values[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            keys = new List<TKey>(dictionary.Keys);
            values = new List<TValue>(dictionary.Values);
        }
    }
}
