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

    [Serializable]
    public class GeneralItem : ISerializationCallbackReceiver
    {
        public Dictionary<string, ObjectWrapper> Fields = new Dictionary<string, ObjectWrapper>();

        [SerializeField] private string Json = string.Empty;
        [SerializeField] private List<UObject> serializedObjects = new List<UObject>();

        private readonly DictionaryConverter dictionaryConverter;
        private readonly ObjectWrapperConverter wrapperConverter;

        public GeneralItem()
        {
            wrapperConverter = new ObjectWrapperConverter(serializedObjects);
            dictionaryConverter = new DictionaryConverter(serializedObjects, wrapperConverter);
        }

        public void OnAfterDeserialize()
        {
            try
            {
                var converter = new DictionaryConverter(serializedObjects, wrapperConverter);
                Fields = JsonConvert.DeserializeObject<Dictionary<string, ObjectWrapper>>(Json, dictionaryConverter);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Json:{0},Error:{1},Detail:{2}", Json, ex.Message, ex.StackTrace);
            }
        }

        public void OnBeforeSerialize()
        {
            try
            {
                if (Fields == null || Fields.Count <= 0) return;
                Json = JsonConvert.SerializeObject(Fields, dictionaryConverter, wrapperConverter);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Json:{0},Error:{1},Detail:{2}", Json, ex.Message, ex.StackTrace);
            }
            foreach (var field in Fields.Values)
            {
                if (!field.IsUnityObject || !field.unityObjRef) continue;
                if (serializedObjects.TrueForAll(o => o.GetInstanceID() != field.unityObjRef.GetInstanceID()))
                    serializedObjects.Add(field.unityObjRef);
            }
        }
    }

    [CreateAssetMenu(menuName = "Configs/Create GeneralConfig")]
    public class GeneralVisualConfig : VisualConfig<GeneralItem>
    {
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
            if (IsUnityObject)
                unityObjRef = obj as UObject;
            else
                objRef = obj;
        }

        public bool IsUnityObject
        {
            get
            {
                var realType = Type.GetType(typeName);
                return realType == typeof(UObject) || realType.IsSubclassOf(typeof(UObject));
            }
        }

        public Type Type
        {
            get
            {
                return Type.GetType(typeName);
            }
        }
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
                            if (obj) target.unityObjRef = obj;
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
            writer.WriteValue(wrapper.IsUnityObject ? wrapper.unityObjRef.GetInstanceID() : 0);
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}
#endif
