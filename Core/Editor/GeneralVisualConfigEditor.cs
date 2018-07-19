using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

#if USE_JSON_NET

namespace Arthas.Common
{
    public delegate T Draw<T>(string label, T value, params GUILayoutOption[] layoutOptions);

    [CustomEditor(typeof(GeneralVisualConfig), isFallback = true)]
    public class GeneralVisualConfigEditor : VisualConfigEditor
    {
        private int currentFieldIndex = 0;
        private object currentObject = null;
        private UnityEngine.Object currentUnityObject = null;
        private string currentFieldName = "fieldName";
        private bool add, remove = false;
        private readonly Dictionary<Type, Delegate> TypeDrawDelegateMap = new Dictionary<Type, Delegate>()
        {
            {typeof(int), new Draw<int>(EditorGUILayout.IntField)},
            {typeof(long), new Draw<long>(EditorGUILayout.LongField)},
            {typeof(float), new Draw<float>(EditorGUILayout.FloatField)},
            {typeof(double), new Draw<double>(EditorGUILayout.DoubleField)},
            {typeof(bool), new Draw<bool>(EditorGUILayout.Toggle)},
            {typeof(string), new Draw<string>(EditorGUILayout.TextField)},
            {typeof(Enum), new Draw<Enum>(EditorGUILayout.EnumMaskField)},

            {typeof(Vector2), new Draw<Vector2>(EditorGUILayout.Vector2Field)},
            {typeof(Vector2Int), new Draw<Vector2Int>(EditorGUILayout.Vector2IntField)},
            {typeof(Vector3), new Draw<Vector3>(EditorGUILayout.Vector3Field)},
            {typeof(Vector3Int), new Draw<Vector3Int>(EditorGUILayout.Vector3IntField)},
            {typeof(Vector4), new Draw<Vector4>(EditorGUILayout.Vector4Field)},
            {typeof(Rect), new Draw<Rect>(EditorGUILayout.RectField)},
            {typeof(RectInt), new Draw<RectInt>(EditorGUILayout.RectIntField)},
            {typeof(Color), new Draw<Color>(EditorGUILayout.ColorField)},
            {typeof(AnimationCurve), new Draw<AnimationCurve>(EditorGUILayout.CurveField)},

            {typeof(Bounds), new Draw<Bounds>(EditorGUILayout.BoundsField)},
            {typeof(BoundsInt), new Draw<BoundsInt>(EditorGUILayout.BoundsIntField)},
            {typeof(UnityEngine.Object), null},
            {typeof(object), new Action<SerializedProperty>(DrawCustomType) }
        };
        private Dictionary<string, ObjectWrapper> templete = new Dictionary<string, ObjectWrapper>();
        private GeneralVisualConfig TargetConfig { get { return target as GeneralVisualConfig; } }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetTemplete();
        }

        private void ResetTemplete()
        {
            templete.Clear();
            var config = target as GeneralVisualConfig;
            if (config.Items.Length <= 0) return;
            var item = config.Items.FirstOrDefault();
            if (item.Fields == null) return;
            foreach (var field in item.Fields) templete.Add(field.Key, field.Value);
        }

        private void ApplyChanges()
        {
            var config = target as GeneralVisualConfig;
            foreach (var item in config.Items)
            {
                if (item.Fields == null)
                    item.Fields = new Dictionary<string, ObjectWrapper>();
                var value = item.Fields;
                foreach (var field in templete)
                {
                    if (!value.ContainsKey(field.Key)) value.Add(field.Key, field.Value);
                }
                var removeList = new List<string>();
                foreach (var field in value)
                {
                    if (!templete.ContainsKey(field.Key)) removeList.Add(field.Key);
                }
                foreach (var key in removeList)
                {
                    value.Remove(key);
                }
            }
            serializedObject.ApplyModifiedProperties();
            ResetTemplete();
        }

        protected override void DrawBeforeBody(SerializedProperty itemsProperty)
        {
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Field", EditorStyles.miniButtonLeft, GUILayout.Height(20f)))
            {
                add = true;
                remove = false;
            }
            if (GUILayout.Button("Remove Field", EditorStyles.miniButtonRight, GUILayout.Height(20f)))
            {
                add = false;
                remove = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
            if (add)
            {
                using (var scope = new GUILayout.VerticalScope())
                {
                    var array = TypeDrawDelegateMap.Keys.ToArray();
                    currentFieldName = EditorGUILayout.TextField("Name", currentFieldName ?? array[currentFieldIndex].FullName);
                    currentFieldIndex = EditorGUILayout.Popup("Type", currentFieldIndex, Array.ConvertAll(array, t => t.FullName));
                    var currentType = array[currentFieldIndex];
                    var invoker = TypeDrawDelegateMap[currentType];
                    var isUnityObject = currentType == typeof(UnityEngine.Object);
                    if (currentObject == null || currentObject.GetType() != currentType)
                        currentObject = GetDefaultValue(currentType);
                    currentObject = isUnityObject
                        ? EditorGUILayout.ObjectField(currentObject as UnityEngine.Object, currentType, true)
                        : invoker.DynamicInvoke(" ", currentObject, new GUILayoutOption[] { });
                }
                GUILayout.Space(5f);
                if (GUILayout.Button("OK"))
                {
                    if (!templete.ContainsKey(currentFieldName))
                    {
                        templete.Add(currentFieldName, new ObjectWrapper(currentObject));
                        ApplyChanges();
                    }
                    else
                    {
                        Debug.LogErrorFormat("Cannot add field , the name {0} existed!", currentFieldName);
                    }
                    add = false;
                }
            }
            else if (remove)
            {
                if (templete.Count != TargetConfig.Items.Length) ResetTemplete();
                var keys = templete.Keys.ToArray();
                for (var i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];
                    using (var horz = new GUILayout.HorizontalScope())
                    {
                        GUI.color = Color.red;
                        var remove = GUILayout.Button("X", GUILayout.Width(45f));
                        GUI.color = Color.white;
                        GUILayout.Label(key);
                        GUILayout.Label(templete[key].Type.FullName);
                        if (remove)
                        {
                            templete.Remove(key);
                            ApplyChanges();
                        }
                    }
                }
            }
            GUILayout.Space(5f);
        }

        private object GetDefaultValue(Type type)
        {
            object value = null;
            if (type == typeof(string))
                value = string.Empty;
            else if (type == typeof(Enum))
                value = 0;
            else if (type == typeof(UnityEngine.Object))
                value = new UnityEngine.Object();
            else
                value = Activator.CreateInstance(type);
            return value;
        }

        public override void DrawItemProperty(SerializedProperty itemProperty, int index)
        {
            var config = target as GeneralVisualConfig;
            if (index >= config.Items.Length) return;
            var item = config.Items[index];
            if (item.Fields == null) return;
            var keys = new List<string>(item.Fields.Keys);
            for (var i = 0; i < keys.Count; i++)
            {
                GUILayout.BeginHorizontal();
                var key = keys[i];
                var value = item.Fields[key];
                try
                {
                    DrawElement(key, ref value);
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("FieleName: {0} | FieldType:{1} | FieldValue: {2}]", key, value.Type.FullName, value.objRef ?? value.unityObjRef);
                    Debug.LogError("JSON:" + config.ToJson());
                    Debug.LogErrorFormat("{0}\n{1}", ex.Message, ex.StackTrace);
                }
                item.Fields[key] = value;
                GUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawElement(string name, ref ObjectWrapper wrapper)
        {
            EditorGUILayout.LabelField(name, GUILayout.Width(100));
            var type = wrapper.Type;
            if (!TypeDrawDelegateMap.ContainsKey(type)) return;
            var invoker = TypeDrawDelegateMap[type];
            if (wrapper.objRef != null)
                wrapper.objRef = invoker.DynamicInvoke("",
                    wrapper.objRef,
                    new GUILayoutOption[] { });
            else
                wrapper.unityObjRef = EditorGUILayout.ObjectField(wrapper.unityObjRef,
                    wrapper.unityObjRef.GetType(),
                    true);
        }

        public static void DrawCustomType(SerializedProperty property)
        {

        }
    }
}
#endif