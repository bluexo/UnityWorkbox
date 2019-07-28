using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

#if USE_JSON_NET

namespace UnityWorkbox.Common
{
    public delegate T Draw<T>(string label, T value, params GUILayoutOption[] layoutOptions);

    [CustomEditor(typeof(GeneralVisualConfig), isFallback = true)]
    public class GeneralVisualConfigEditor : VisualConfigEditor
    {
        private int currentFieldIndex = 0;
        private object currentObject = null;
        private UnityEngine.Object currentUnityObject = null;
        private bool isArrayField = false;
        private string currentFieldName = "fieldName";
        private bool add, remove = false;
        private static readonly Dictionary<Type, Delegate> TypeDrawDelegateMap = new Dictionary<Type, Delegate>()
        {
            {typeof(int), new Draw<int>(EditorGUILayout.IntField)},
            {typeof(long), new Draw<long>(EditorGUILayout.LongField)},
            {typeof(float), new Draw<float>(EditorGUILayout.FloatField)},
            {typeof(double), new Draw<double>(EditorGUILayout.DoubleField)},
            {typeof(bool), new Draw<bool>(EditorGUILayout.Toggle)},
            {typeof(string), new Draw<string>(EditorGUILayout.TextField)},
            {typeof(Enum), new Func<string, object, GUILayoutOption[],int>(DrawEnumType)},

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
            {typeof(UnityEngine.Object), new Func<UnityEngine.Object,Type,bool,UnityEngine.Object>(DrawObjectField)}
        };


        private Dictionary<string, ObjectWrapper> templete = new Dictionary<string, ObjectWrapper>();
        private GeneralVisualConfig Config { get { return target as GeneralVisualConfig; } }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetTemplete();
        }

        private void ResetTemplete()
        {
            templete.Clear();
            var config = target as GeneralVisualConfig;
            if (config.Items.Count <= 0) return;
            var item = config.Items.FirstOrDefault();
            if (item.fields == null) return;
            foreach (var field in item.fields) templete.Add(field.Key, field.Value);
        }

        private void ApplyChanges()
        {
            foreach (var item in Config.Items)
            {
                if (item.fields == null) item.fields = new Dictionary<string, ObjectWrapper>();
                var value = item.fields;
                if (value == null) continue;
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
            EditorUtility.SetDirty(Config);
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
                    isArrayField = EditorGUILayout.Toggle("IsArray", isArrayField);
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
                        if (isArrayField && currentObject.GetType() == typeof(UnityEngine.Object))
                        {
                            Debug.LogError("GeneralVisualConfig cannot support reference type array!");
                            isArrayField = false;
                            return;
                        }
                        var obj = isArrayField
                            ? new object[] { currentObject }
                            : currentObject;
                        var wrapper = new ObjectWrapper(obj);
                        templete.Add(currentFieldName, wrapper);
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
                if (templete.Count != Config.Items.Count) ResetTemplete();
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
            if (index >= Config.Items.Count) return;
            var item = Config.Items[index];
            if (item.fields == null) return;
            var keys = new List<string>(item.fields.Keys);
            for (var i = 0; i < keys.Count; i++)
            {
                GUILayout.BeginHorizontal();
                var key = keys[i];
                var value = item.fields[key];
                DrawElement(key, ref value);
                item.fields[key] = value;
                GUILayout.EndHorizontal();
            }
        }

        private void DrawElement(string name, ref ObjectWrapper wrapper)
        {
            EditorGUILayout.LabelField(name, GUILayout.Width(100));
            var type = wrapper.Type;
            if (wrapper.Type.IsArray)
            {
                var obj = wrapper.objRef as object[];
                DrawArrayType(ref obj);
                wrapper.objRef = obj;
                return;
            }
            if (!TypeDrawDelegateMap.ContainsKey(type)) return;
            var invoker = TypeDrawDelegateMap[type];
            if (wrapper.objRef != null)
            {
                wrapper.objRef = invoker.DynamicInvoke("",
                    Convert.ChangeType(wrapper.objRef, type),
                    new GUILayoutOption[] { });
            }
            else
            {
                wrapper.mark = Guid.NewGuid().ToString();
                wrapper.unityObjRef = (UnityEngine.Object)invoker.DynamicInvoke(wrapper.unityObjRef = wrapper.unityObjRef ?? new UnityEngine.Object(),
                    wrapper.unityObjRef.GetType(),
                    true);
            }
            EditorUtility.SetDirty(Config);
        }

        public static UnityEngine.Object DrawObjectField(UnityEngine.Object obj, Type type, bool sceneObject = true)
        {
            var realType = obj.GetType();
            if (realType == typeof(Sprite))
            {
                var sprite = obj as Sprite;
                return EditorGUILayout.ObjectField(obj.name,
                    sprite,
                    typeof(Sprite),
                    true);
            }
            return EditorGUILayout.ObjectField(obj, realType, sceneObject);
        }

        public static int DrawEnumType(string label, object value, params GUILayoutOption[] layoutOptions)
        {
            var intValue = Convert.ChangeType(value, typeof(int));
            return (int)intValue;
        }

        public static void DrawCustomType(SerializedProperty property)
        {

        }

        private void DrawArrayType(ref object[] list)
        {
            EditorGUILayout.BeginVertical();
            for (var i = 0; i < list.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical();
                var type = list[i].GetType();
                if (!TypeDrawDelegateMap.ContainsKey(type)) return;
                var invoker = TypeDrawDelegateMap[type];
                if (list[i] != null)
                {
                    list[i] = invoker.DynamicInvoke("",
                        Convert.ChangeType(list[i], type),
                        new GUILayoutOption[] { });
                }
                EditorUtility.SetDirty(Config);
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.Width(24)))
                    ArrayUtility.Insert(ref list, i, GetDefaultValue(type));
                if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(24)))
                    ArrayUtility.RemoveAt(ref list, i);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif