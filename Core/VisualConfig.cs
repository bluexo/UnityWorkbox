using System;
using UnityEngine;

namespace Arthas.Common
{
#if UNITY_EDITOR
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;

    /// <summary>
    /// 可配置的列表编辑器
    /// 包含折叠的和展开列表，以及导出为Json数据或者从Json导入数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [CustomEditor(typeof(VisualConfig<>), true, isFallback = true)]
    public class VisualConfigEditor : Editor
    {
        protected SerializedProperty itemsProperty;

        protected bool[] folds;

        protected bool importOption;

        protected Type[] typesCache;

        protected virtual void OnEnable()
        {
            itemsProperty = serializedObject.FindProperty("items");
            if (itemsProperty == null)
            {
                Debug.LogError("Cannot found items ,you may be forgot add <color=cyan>[Serializable]</color> attribute to your item!");
                return;
            }
            if (typesCache == null)
                typesCache = GetType().Assembly.GetTypes();
            if (itemsProperty.arraySize <= 0)
                itemsProperty.arraySize++;
            folds = new bool[itemsProperty.arraySize];
            for (var i = 0; i < folds.Length; i++)
                folds[i] = true;
            var backupDir = serializedObject.FindProperty("backupDirectory");
            backupDir.stringValue = Application.dataPath.Replace("/Assets", "") + "/ProjectSettings";
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            if (ShowBaseInspactor)
                base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.yellow;
            if (GUILayout.Button(">", EditorStyles.miniButtonLeft, GUILayout.Height(25f)))
                for (var i = 0; i < folds.Length; i++) { folds[i] = false; }
            if (GUILayout.Button("∨", EditorStyles.miniButtonRight, GUILayout.Height(25f)))
                for (var i = 0; i < folds.Length; i++) { folds[i] = true; }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            if (itemsProperty != null)
            {
                for (var i = 0; i < itemsProperty.arraySize; i++)
                {
                    folds[i] = EditorGUILayout.Foldout(folds[i], string.Format("Item [{0}]", i));
                    if (folds[i])
                    {
                        var item = itemsProperty.GetArrayElementAtIndex(i);
                        DrawItemProperty(item, i);
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginHorizontal();
                        GUI.color = Color.green;
                        if (GUILayout.Button("+"))
                        {
                            ArrayUtility.Insert(ref folds, i, false);
                            itemsProperty.InsertArrayElementAtIndex(i);
                        }
                        GUI.color = Color.red;
                        if (itemsProperty.arraySize > 1 && GUILayout.Button("-"))
                        {
                            itemsProperty.DeleteArrayElementAtIndex(i);
                            ArrayUtility.RemoveAt(ref folds, i);
                        }
                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                    }
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(12f);
            importOption = EditorGUILayout.Foldout(importOption, "Import And Export");
            if (importOption)
            {
                var id = serializedObject.targetObject.GetInstanceID();
                var name = EditorPrefs.GetString(id.ToString(), target.name);
                var fileName = EditorGUILayout.TextField("FileName", name);
                if (fileName != name)
                    EditorPrefs.SetString(id.ToString(), fileName);

                var r = EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("To JSON", EditorStyles.miniButtonLeft, GUILayout.Height(25f)))
                {
                    var path = EditorUtility.SaveFilePanel("Save to json", "", fileName, "json");
                    var conf = serializedObject.targetObject as IJsonSerializable;
                    File.WriteAllText(path, conf.ToJson());
                }
                if (GUILayout.Button("From JSON", EditorStyles.miniButtonRight, GUILayout.Height(25f)))
                {
                    var path = EditorUtility.OpenFilePanel("Overwrite from json", "", "json");
                    var conf = serializedObject.targetObject as IJsonSerializable;
                    var json = File.ReadAllText(path);
                    conf.FromJson(json);
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.DrawRect(r, Color.blue / 3);
            }
        }

        public virtual bool ShowBaseInspactor { get { return true; } }

        public virtual void DrawItemProperty(SerializedProperty property, int index)
        {
            EditorGUILayout.PropertyField(property);
            if (typesCache == null) typesCache = GetType().Assembly.GetTypes();
            var type = Array.Find(typesCache, t => t.Name.Equals(property.type, StringComparison.CurrentCultureIgnoreCase));
            if (type == null)
            {
                Debug.LogErrorFormat("Unknow type {0} , cannot draw this property!", property.type);
                return;
            }
            var fields = type.GetFields();
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (field.IsNotSerialized)
                    continue;
                DrawPropertyField(property, field.Name, field.FieldType);
            }
        }

        protected virtual void DrawPropertyField(SerializedProperty property, string propertyName, Type type)
        {
            var subProperty = property.FindPropertyRelative(propertyName);

            if (type == typeof(Sprite))
            {
                var name = string.Format(" [{0}] ", subProperty.objectReferenceValue ? subProperty.objectReferenceValue.name : string.Empty);
                subProperty.objectReferenceValue = EditorGUILayout.ObjectField(propertyName + name, subProperty.objectReferenceValue, typeof(Sprite), true);
            }
            else
            {
                EditorGUILayout.PropertyField(subProperty);
            }
        }
    }
#endif

    public interface IJsonSerializable
    {
        string ToJson();
        void FromJson(string json);
    }

    public abstract class VisualConfig<T> : ScriptableObject, IJsonSerializable where T : new()
    {
        [SerializeField]
        protected bool autoBackup = true;

        [SerializeField, Path(Type = PathType.Folder, Relative = false)]
        protected string backupDirectory;

        [Space(30)]
        [SerializeField, HideInInspector]
        protected T[] items = { new T() };
        public virtual T[] Items { get { return items; } }

        public virtual string ToJson()
        {
            return new JsonList<T>(items).ToJson(true);
        }

        public virtual void FromJson(string json)
        {
            var jArray = new JsonList<T>().Overwrite(json, true);
            items = jArray.Value.ToArray();
        }
    }
}