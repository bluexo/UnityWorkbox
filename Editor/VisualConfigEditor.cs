using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Arthas.Common
{
    /// <summary>
    /// 可配置的列表编辑器
    /// 包含折叠的和展开列表，自动备份以及作为Json数据导入和导出
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [CustomEditor(typeof(VisualConfig<>), true, isFallback = true)]
    public class VisualConfigEditor : Editor
    {
        protected SerializedProperty itemsProperty, backupDirProperty, backupTagProperty;

        protected const string RuntimeObjRefNamePrefix = "PPtr";
        protected bool importOption;
        protected bool[] folds;

        protected virtual void OnEnable()
        {
            itemsProperty = serializedObject.FindProperty("items");
            if (itemsProperty == null)
            {
                Debug.LogError("Cannot found items ,you may be forgot add <color=cyan>[Serializable]</color> attribute to your item!", target);
                return;
            }
            if (itemsProperty.arraySize <= 0)
                itemsProperty.arraySize++;
            folds = new bool[itemsProperty.arraySize];
            for (var i = 0; i < folds.Length; i++) folds[i] = true;
            backupDirProperty = serializedObject.FindProperty("backupDirectory");
            backupTagProperty = serializedObject.FindProperty("backupTag");
            if (string.IsNullOrEmpty(backupDirProperty.stringValue) || !Directory.Exists(backupDirProperty.stringValue))
                backupDirProperty.stringValue = Application.dataPath.Replace("/Assets", "") + "/ProjectSettings";
            if (string.IsNullOrEmpty(backupTagProperty.stringValue))
                backupTagProperty.stringValue = Guid.NewGuid().ToString();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnDisable()
        {
            if (string.IsNullOrEmpty(backupDirProperty.stringValue) || string.IsNullOrEmpty(backupTagProperty.stringValue)) return;
            var path = string.Format("{0}/{1}.json", backupDirProperty.stringValue, backupTagProperty.stringValue);
            WriteToFile(path);
        }

        private void WriteToFile(string path)
        {
            var conf = serializedObject.targetObject as IJsonSerializable;
            var json = conf.ToJson();
            File.WriteAllText(path, json);
        }

        private void ReadFromFile(string path)
        {
            var conf = serializedObject.targetObject as IJsonSerializable;
            var json = File.ReadAllText(path);
            conf.FromJson(json);
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
            if (GUILayout.Button("Recovery", EditorStyles.miniButton, GUILayout.Width(60), GUILayout.Height(25)))
            {
                if (string.IsNullOrEmpty(backupDirProperty.stringValue)
                    || string.IsNullOrEmpty(backupTagProperty.stringValue))
                {
                    Debug.LogError("Cannot found backup file!");
                    return;
                }
                var path = string.Format("{0}/{1}.json", backupDirProperty.stringValue, backupTagProperty.stringValue);
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogErrorFormat("Backup file [{0}] not exists !", path);
                    return;
                }
                if (EditorUtility.DisplayDialog("Warning", "This operation will be overwrite current config!!!", "√"))
                    ReadFromFile(path);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical();
            if (itemsProperty != null)
            {
                for (var i = 0; i < itemsProperty.arraySize; i++)
                {
                    folds[i] = EditorGUILayout.Foldout(folds[i], string.Format("Item [{0}]", i));
                    if (!folds[i]) continue;
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
                    if (string.IsNullOrEmpty(path)) return;
                    WriteToFile(path);
                }
                if (GUILayout.Button("From JSON", EditorStyles.miniButtonRight, GUILayout.Height(25f)))
                {
                    var path = EditorUtility.OpenFilePanel("Overwrite from json", "", "json");
                    if (string.IsNullOrEmpty(path)) return;
                    ReadFromFile(path);
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
            if (property.type.Contains(RuntimeObjRefNamePrefix))
            {
                if (!property.objectReferenceValue) return;
                var so = new SerializedObject(property.objectReferenceValue);
                var fields = property
                    .objectReferenceValue
                    .GetType()
                    .GetFields()
                    .ToArray();
                for (var i = 0; i < fields.Length; i++)
                {
                    var fieldProperty = so.FindProperty(fields[i].Name);
                    if (fieldProperty == null) continue;
                    EditorGUILayout.PropertyField(fieldProperty);
                }
                so.ApplyModifiedProperties();
                return;
            }
            else
            {
                var type = target.GetType().BaseType.GetGenericArguments().LastOrDefault();
                if (type == null)
                {
                    Debug.LogErrorFormat("Unknow type {0}, cannot draw this property!", property.type);
                    return;
                }
                var fields = type.GetFields();
                if (fields.Length == 0) return;
                for (var i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    if (field.IsNotSerialized) continue;
                    DrawPropertyField(property, field.Name, field.FieldType);
                }
            }
        }

        protected virtual void DrawPropertyField(SerializedProperty property, string propertyName, Type type)
        {
            var subProperty = property.FindPropertyRelative(propertyName);
            if (subProperty == null) return;
            if (type == typeof(Sprite))
            {
                var name = subProperty.objectReferenceValue ? subProperty.objectReferenceValue.name : string.Empty;
                var sprite = subProperty.objectReferenceValue as Sprite;
                subProperty.objectReferenceValue = EditorGUILayout.ObjectField(propertyName + name,
                    sprite,
                    typeof(Sprite),
                    true);
            }
            else
            {
                EditorGUILayout.PropertyField(subProperty);
            }
        }
    }
}
