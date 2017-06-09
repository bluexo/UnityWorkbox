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
    public abstract class ConfigurableArrayEditor<T> : Editor where T : new()
    {
        protected SerializedProperty property;

        protected bool[] folds;

        protected bool importOption;

        protected void OnEnable()
        {
            property = serializedObject.FindProperty("items");
            if (property.arraySize <= 0) property.arraySize++;
            folds = new bool[property.arraySize];
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

            var rect = EditorGUILayout.BeginVertical();
            for (var i = 0; i < property.arraySize; i++) {
                folds[i] = EditorGUILayout.Foldout(folds[i], string.Format("Item [{0}]", i));
                if (folds[i]) {
                    var item = property.GetArrayElementAtIndex(i);
                    DrawItemProperty(item, i);
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = Color.green;
                    if (GUILayout.Button("+")) {
                        ArrayUtility.Insert(ref folds, i, false);
                        property.InsertArrayElementAtIndex(i);
                    }
                    GUI.color = Color.red;
                    if (property.arraySize > 1 && GUILayout.Button("-")) {
                        property.DeleteArrayElementAtIndex(i);
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
                    var path = EditorUtility.SaveFilePanel("Save to json", "", "Json", "json");
                    var stream = File.Open(path, FileMode.OpenOrCreate);
                    using (var writer = new StreamWriter(stream)) {
                        var conf = serializedObject.targetObject as ConfigurableArray<T>;
                        writer.Write(conf.ToJson());
                        writer.Flush();
                    }
                }
                if (GUILayout.Button("From JSON", EditorStyles.miniButtonRight, GUILayout.Height(25f))) {
                    var path = EditorUtility.OpenFilePanel("Overwrite from json", "", "json");
                    var stream = File.OpenRead(path);
                    using (var reader = new StreamReader(stream)) {
                        var conf = serializedObject.targetObject as ConfigurableArray<T>;
                        var json = reader.ReadToEnd();
                        conf.FromJson(json);
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.DrawRect(r, Color.blue / 3);
            }
        }

        public virtual bool ShowBaseInspactor { get { return false; } }

        public abstract void DrawItemProperty(SerializedProperty property, int index);
    }
#endif

    public abstract class ConfigurableArray<T> : ScriptableObject where T : new()
    {
        [SerializeField]
        protected T[] items = { new T() };

        public T[] Items { get { return items; } }

        public virtual string ToJson()
        {
            return new JArray<T>(items).ToJson(true);
        }

        public virtual void FromJson(string json)
        {
            var jArray = new JArray<T>().Overwrite(json, true);
            items = jArray.Value.ToArray();
        }
    }
}