using System;
using System.Collections.Generic;

using UnityEngine;
using UObject = UnityEngine.Object;

#if USE_JSON_NET
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Arthas
{
    using Arthas.Common;
    using System.Reflection;

    [Serializable]
    public class GeneralItem : ISerializationCallbackReceiver
    {
        public Dictionary<string, ObjectWrapper> fields = new Dictionary<string, ObjectWrapper>();
        [SerializeField] private string json = string.Empty;
        [SerializeField] private List<UObject> serializedObjects = new List<UObject>();

        private readonly DictionaryConverter dictionaryConverter;
        private readonly ObjectWrapperConverter wrapperConverter;

        public GeneralItem()
        {
            wrapperConverter = new ObjectWrapperConverter(serializedObjects);
            dictionaryConverter = new DictionaryConverter(serializedObjects, wrapperConverter);
        }

        public virtual void OnAfterDeserialize()
        {
            try
            {
                var converter = new DictionaryConverter(serializedObjects, wrapperConverter);
                fields = JsonConvert.DeserializeObject<Dictionary<string, ObjectWrapper>>(json, dictionaryConverter);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Json:{0},Error:{1},Detail:{2}", json, ex.Message, ex.StackTrace);
            }
        }

        public virtual void OnBeforeSerialize()
        {
            try
            {
                if (fields == null) return;
                json = JsonConvert.SerializeObject(fields, dictionaryConverter, wrapperConverter);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Json:{0},Error:{1},Detail:{2}", json, ex.Message, ex.StackTrace);
            }
            foreach (var field in fields.Values)
            {
                if (!field.IsUnityObject || !field.unityObjRef) continue;
                if (serializedObjects.TrueForAll(o => o.GetInstanceID() != field.unityObjRef.GetInstanceID()))
                    serializedObjects.Add(field.unityObjRef);
            }
        }

        public virtual T Get<T>() where T : new()
        {
            var t = new T();
            foreach (var field in fields)
            {
                var member = typeof(T).GetField(field.Key);
                if (member == null)
                {
                    Debug.Log("Cannot found field :" + field.Key);
                    continue;
                }
                try { member.SetValue(t, field.Value.GetObject()); }
                catch { continue; }
            }
            return t;
        }

        public string GetJsonString() { return json; }
    }

    [CreateAssetMenu(menuName = "Configs/Create GeneralConfig")]
    public class GeneralVisualConfig : VisualConfig<GeneralItem>
    {
        public List<T> GetItems<T>() where T : new()
        {
            var array = new List<T>();
            for (var i = 0; i < items.Length; i++)
            {
                var t = items[i].Get<T>();
                array.Add(t);
            }
            return array;
        }
    }

    [Serializable]
    public struct ObjectWrapper
    {
        public string typeName;
        public object objRef;
        public UObject unityObjRef;

        public ObjectWrapper(object obj)
        {
            objRef = null;
            unityObjRef = null;
            typeName = string.Format("{0},{1}", obj.GetType().FullName, obj.GetType().Assembly.FullName);
            if (IsUnityObject) unityObjRef = obj as UObject;
            else objRef = obj;
        }

        public bool IsUnityObject
        {
            get
            {
                var realType = Type.GetType(typeName);
                return realType == typeof(UObject) || realType.IsSubclassOf(typeof(UObject));
            }
        }

        public object GetObject()
        {
            if (objRef != null) return objRef;
            else if (IsUnityObject) return unityObjRef;
            else return null;
        }

        public Type Type { get { return Type.GetType(typeName); } }
    }

    /// <summary>
    /// 字典类型转换器
    /// </summary>
    public class DictionaryConverter : JsonConverter
    {
        private ObjectWrapperConverter WrapperConverter;
        private readonly List<UObject> Target;

        public DictionaryConverter(List<UObject> items, ObjectWrapperConverter converter)
        {
            WrapperConverter = converter;
            Target = items;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, ObjectWrapper>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var dict = new Dictionary<string, ObjectWrapper>();
            string currentKey = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    currentKey = reader.Value as string;
                }
                else if (currentKey != null)
                {
                    var jObject = JObject.Load(reader);
                    var id = jObject["unityObjRef"]["instanceID"].Value<int>();
                    var obj = Target.Find(u => u.GetInstanceID() == id);
                    var wrapper = jObject.ToObject<ObjectWrapper>();
                    if (obj != null) wrapper.unityObjRef = obj;
                    dict.Add(currentKey, wrapper);
                    currentKey = null;
                }
            }
            return dict;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dict = (Dictionary<string, ObjectWrapper>)value;
            var json = JsonConvert.SerializeObject(dict, WrapperConverter);
            writer.WriteRaw(json);
        }
    }

    public class ObjectWrapperConverter : JsonConverter
    {
        const string TypeName = "typeName", ObjRef = "objRef", UnityObjRef = "unityObjRef", InstanceId = "instanceID";
        public List<UObject> SerializedObjects { get; private set; }

        public ObjectWrapperConverter(List<UObject> objects) { SerializedObjects = objects; }

        public override bool CanConvert(Type objectType) { return objectType == typeof(ObjectWrapper); }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var target = new ObjectWrapper();
            string prevProperty = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    prevProperty = reader.Value as string;
                }
                else if (prevProperty == TypeName)
                {
                    target.typeName = reader.Value as string;
                    prevProperty = null;
                }
                else if (prevProperty == ObjRef)
                {
                    target.objRef = reader.Value;
                    target.unityObjRef = null;
                    prevProperty = null;
                }
                else if (prevProperty == InstanceId)
                {
                    if (reader.Value != null)
                    {
                        var id = Convert.ToInt32(reader.Value);
                        if (SerializedObjects != null)
                        {
                            var obj = SerializedObjects.Find(i => i != null && i.GetInstanceID() == id);
                            if (obj)
                            {
                                target.objRef = null;
                                target.unityObjRef = obj;
                            }
                        }
                    }
                    prevProperty = null;
                }
            }
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var wrapper = (ObjectWrapper)value;

            writer.WriteStartObject();

            writer.WritePropertyName("typeName");
            writer.WriteValue(wrapper.typeName);
            writer.WritePropertyName("objRef");
            var json = string.Empty;
            try
            {
                json = JsonConvert.SerializeObject(wrapper.objRef);
            }
            catch (JsonException)
            {
                json = JsonUtility.ToJson(wrapper.objRef);
            }
            writer.WriteRawValue(json);
            writer.WritePropertyName("unityObjRef");

            writer.WriteStartObject();
            writer.WritePropertyName("instanceID");
            var id = 0;
            try { id = wrapper.unityObjRef.GetInstanceID(); } catch { }
            writer.WriteValue(id);
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}
#endif
