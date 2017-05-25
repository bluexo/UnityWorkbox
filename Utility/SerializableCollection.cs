using System;
using System.Collections;
using System.Collections.Generic;
namespace UnityEngine
{
    [Serializable]
    public class JArray<T>
    {
        [SerializeField]
        private List<T> value;
        public List<T> Value { get { return value; } }

        public JArray() { }

        public JArray(List<T> list)
        {
            value = list;
        }

        public JArray(T[] arr)
        {
            value = new List<T>(arr);
        }

        public JArray(IEnumerable<T> arr)
        {
            value = new List<T>(arr);
        }

        public JArray<T> Overwrite(string json)
        {
            var arr = JsonUtility.FromJson<JArray<T>>(json);
            value = arr.value;
            return this;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(value);
        }
    }

    [Serializable]
    public class JHash<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys;
        [SerializeField]
        private List<TValue> values;

        private Dictionary<TKey, TValue> dictionary;
        public Dictionary<TKey, TValue> Dictionary { get { return dictionary; } }

        public JHash(Dictionary<TKey, TValue> dict)
        {
            dictionary = dict;
        }

        public void OnAfterDeserialize()
        {
            dictionary = new Dictionary<TKey, TValue>();
            for (var i = 0; i < Math.Min(keys.Count, values.Count); i++) {
                dictionary.Add(keys[i], values[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            keys = new List<TKey>(dictionary.Keys);
            values = new List<TValue>(dictionary.Values);
        }

        public JHash<TKey, TValue> Overwrite(string json)
        {
            var hash = JsonUtility.FromJson<JHash<TKey, TValue>>(json);
            dictionary = hash.dictionary;
            return this;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
