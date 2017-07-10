using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    [Serializable]
    public class JsonList<T>
    {
        [SerializeField]
        private List<T> value;
        public List<T> Value { get { return value; } }

        public JsonList() { }

        public JsonList(List<T> list)
        {
            value = list;
        }

        public JsonList(T[] arr)
        {
            value = new List<T>(arr);
        }

        public JsonList(IEnumerable<T> arr)
        {
            value = new List<T>(arr);
        }

        public JsonList<T> Overwrite(string json, bool pureArray = false)
        {
            if (pureArray) json = '{' + string.Format("\"value\":{0}", json) + '}';
            var arr = JsonUtility.FromJson<JsonList<T>>(json);
            value = arr.value;
            return this;
        }

        public string ToJson(bool pureArray = false)
        {
            var json = JsonUtility.ToJson(this);
            return pureArray ? json.TrimStart('{').Substring("\"value:\"".Length).TrimEnd('}') : json;
        }

        public static implicit operator JsonList<T> (List<T> list)
        {
            return new JsonList<T>(list);
        }

        public static implicit operator List<T>(JsonList<T> jsonList)
        {
            return jsonList.value;
        }
    }

    [Serializable]
    public class JsonDict<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keyList;
        [SerializeField]
        private List<TValue> valueList;

        private Dictionary<TKey, TValue> value;
        public Dictionary<TKey, TValue> Value { get { return value; } }

        public JsonDict(Dictionary<TKey, TValue> dict)
        {
            value = dict;
        }

        public void OnAfterDeserialize()
        {
            value = new Dictionary<TKey, TValue>();
            for (var i = 0; i < Math.Min(keyList.Count, valueList.Count); i++) {
                value.Add(keyList[i], valueList[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            keyList = new List<TKey>(value.Keys);
            valueList = new List<TValue>(value.Values);
        }

        public JsonDict<TKey, TValue> Overwrite(string json)
        {
            var hash = JsonUtility.FromJson<JsonDict<TKey, TValue>>(json);
            value = hash.value;
            return this;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static implicit operator JsonDict<TKey,TValue>(Dictionary<TKey,TValue> dict)
        {
            return new JsonDict<TKey, TValue>(dict);
        }

        public static implicit operator Dictionary<TKey,TValue>(JsonDict<TKey,TValue> jsonDict)
        {
            return jsonDict.value;
        }
    }
}
