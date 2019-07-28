using System;
using System.Collections.Generic;

using UnityEngine;
using UObject = UnityEngine.Object;

#if USE_JSON_NET
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityWorkbox
{
    using UnityWorkbox.Common;
    using System.Reflection;

    [Serializable]
    public class GeneralItem : ISerializationCallbackReceiver
    {
        public Dictionary<string, ObjectWrapper> fields = new Dictionary<string, ObjectWrapper>();
        [SerializeField] private string json = string.Empty;
        [SerializeField] private List<UnityObjectWrapper> serializedObjects = new List<UnityObjectWrapper>();

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
            if (fields == null) return;
            var keys = new List<string>(fields.Keys);
            serializedObjects.Clear();
            for (var i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                var field = fields[key];
                if (!field.IsUnityObject || !field.unityObjRef) continue;
                var exists = serializedObjects.Exists(s => s.mark == field.mark);
                if (exists)
                {
                    var wrapper = new ObjectWrapper
                    {
                        mark = Guid.NewGuid().ToString(),
                        objRef = field.objRef,
                        typeName = field.typeName,
                        unityObjRef = field.unityObjRef,
                    };
                    fields[key] = wrapper;
                }
                var uwrapper = new UnityObjectWrapper()
                {
                    mark = fields[key].mark,
                    unityObject = field.unityObjRef
                };
                serializedObjects.Add(uwrapper);
            }
            try
            {
                json = JsonConvert.SerializeObject(fields, dictionaryConverter, wrapperConverter);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Json:{0},Error:{1},Detail:{2}", json, ex.Message, ex.StackTrace);
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

    [CreateAssetMenu(menuName = "Configs/Create GeneralConfig", order = -1)]
    public class GeneralVisualConfig : VisualConfig<GeneralItem>
    {
        public List<T> GetItems<T>() where T : new() => items.ConvertAll(c => c.Get<T>());
    }

    [Serializable]
    public class UnityObjectWrapper
    {
        public string mark;
        public UObject unityObject;
    }

    [Serializable]
    public struct ObjectWrapper
    {
        public string typeName;
        public string mark;
        public object objRef;
        public UObject unityObjRef;

        public ObjectWrapper(object obj)
        {
            objRef = null;
            unityObjRef = null;
            mark = Guid.NewGuid().ToString();
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

        public object GetObject() { return objRef ?? unityObjRef ?? null; }

        public Type Type { get { return Type.GetType(typeName); } }
    }

    /// <summary>
    /// 字典类型转换器
    /// </summary>
    public class DictionaryConverter : JsonConverter
    {
        private ObjectWrapperConverter WrapperConverter;
        private readonly List<UnityObjectWrapper> Target;

        public DictionaryConverter(List<UnityObjectWrapper> items, ObjectWrapperConverter converter)
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
                    var instanceId = jObject["unityObjRef"]["instanceID"].Value<int>();
                    if (!serializer.Converters.Contains(WrapperConverter))
                        serializer.Converters.Add(WrapperConverter);
                    var wrapper = jObject.ToObject<ObjectWrapper>(serializer);
                    var target = Target.Find(t => t.mark == wrapper.mark);
                    if (target == null)
                    {
                        var propertyType = wrapper.Type;
                        wrapper.objRef = jObject["objRef"].ToObject(propertyType);
                    }
                    else
                    {
                        wrapper.unityObjRef = target.unityObject;
                    }
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

    /// <summary>
    /// 对象包装转换器
    /// </summary>
    public class ObjectWrapperConverter : JsonConverter
    {
        private const string TypeName = "typeName",
            UnityObjRef = "unityObjRef",
            InstanceId = "instanceID",
            ObjRef = "objRef",
            Mark = "mark";
        public List<UnityObjectWrapper> SerializedObjects { get; private set; }

        public ObjectWrapperConverter(List<UnityObjectWrapper> objects) { SerializedObjects = objects; }

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
                else if (prevProperty == Mark)
                {
                    target.mark = reader.Value as string;
                    prevProperty = null;
                }
            }
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var wrapper = (ObjectWrapper)value;

            writer.WriteStartObject();

            writer.WritePropertyName(TypeName);
            writer.WriteValue(wrapper.typeName);

            writer.WritePropertyName(ObjRef);
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

            writer.WritePropertyName(Mark);
            writer.WriteValue(wrapper.mark);

            writer.WritePropertyName(UnityObjRef);
            writer.WriteStartObject();
            writer.WritePropertyName(InstanceId);
            var id = 0;
            try { id = wrapper.unityObjRef.GetInstanceID(); } catch { }
            writer.WriteValue(id);
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }

}
#endif
