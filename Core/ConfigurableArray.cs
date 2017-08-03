using System;
using UnityEngine;

namespace Arthas.Common
{
#if UNITY_EDITOR
    using System.IO;
    using UnityEditor;

    /// <summary>
    /// 可配置的列表编辑器
    /// 包含折叠的和展开列表，以及导出为Json数据或者从Json导入数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [CustomEditor(typeof(ConfigurableArray<>), true)]
    public class ConfigurableArrayEditor<T> : Editor where T : new()
    {
        protected SerializedProperty itemsProperty;

        protected bool[] folds;

        protected bool importOption;

        protected virtual void OnEnable()
        {
            itemsProperty = serializedObject.FindProperty("items");
            if (itemsProperty.arraySize <= 0) itemsProperty.arraySize++;
            folds = new bool[itemsProperty.arraySize];
            var backupDir = serializedObject.FindProperty("backupDirectory");
            backupDir.stringValue = Application.dataPath.Replace("/Assets", "") + "/ProjectSettings";
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            if (ShowBaseInspactor) base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.yellow;
            if (GUILayout.Button(">", EditorStyles.miniButtonLeft, GUILayout.Height(25f)))
                for (var i = 0; i < folds.Length; i++) { folds[i] = false; }
            if (GUILayout.Button("∨", EditorStyles.miniButtonRight, GUILayout.Height(25f)))
                for (var i = 0; i < folds.Length; i++) { folds[i] = true; }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            for (var i = 0; i < itemsProperty.arraySize; i++) {
                folds[i] = EditorGUILayout.Foldout(folds[i], string.Format("Item [{0}]", i));
                if (folds[i]) {
                    var item = itemsProperty.GetArrayElementAtIndex(i);
                    DrawItemProperty(item, i);
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = Color.green;
                    if (GUILayout.Button("+")) {
                        ArrayUtility.Insert(ref folds, i, false);
                        itemsProperty.InsertArrayElementAtIndex(i);
                    }
                    GUI.color = Color.red;
                    if (itemsProperty.arraySize > 1 && GUILayout.Button("-")) {
                        itemsProperty.DeleteArrayElementAtIndex(i);
                        ArrayUtility.RemoveAt(ref folds, i);
                    }
                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(12f);
            importOption = EditorGUILayout.Foldout(importOption, "Import And Export");
            if (importOption) {
                var id = serializedObject.targetObject.GetInstanceID();
                var name = EditorPrefs.GetString(id.ToString(), typeof(T).Name);
                var fileName = EditorGUILayout.TextField("FileName", name);
                if (fileName != name) EditorPrefs.SetString(id.ToString(), fileName);

                var r = EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("To JSON", EditorStyles.miniButtonLeft, GUILayout.Height(25f))) {
                    var path = EditorUtility.SaveFilePanel("Save to json", "", fileName, "json");
                    var conf = serializedObject.targetObject as ConfigurableArray<T>;
                    File.WriteAllText(path, conf.ToJson());
                }
                if (GUILayout.Button("From JSON", EditorStyles.miniButtonRight, GUILayout.Height(25f))) {
                    var path = EditorUtility.OpenFilePanel("Overwrite from json", "", "json");
                    var conf = serializedObject.targetObject as ConfigurableArray<T>;
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
        }
    }
#endif

    public abstract class ConfigurableArray<T> : ScriptableObject where T : new()
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