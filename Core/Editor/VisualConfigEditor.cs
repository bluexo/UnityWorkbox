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
        private readonly GUIStyle GUIStyle = new GUIStyle();
        protected SerializedProperty itemsProperty, backupDirProperty, backupTagProperty;
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

        protected virtual void DrawBeforeBody(SerializedProperty serializedProperty) { }
        protected virtual void DrawAfterBody(SerializedProperty serializedProperty) { }
        protected virtual void BeforeInsertItem(int index) { }
        protected virtual void BeforeDeleteItem(int index) { }
        protected virtual void AfterInsertItem(int index) { }
        protected virtual void AfterDeleteItem(int index) { }

        private void WriteToFile(string path)
        {
            var conf = serializedObject.targetObject as IJsonSerializable;
            if (conf == null) return;
            var json = conf.ToJson();
            File.WriteAllText(path, json);
        }

        private void ReadFromFile(string path)
        {
            var conf = serializedObject.targetObject as IJsonSerializable;
            if (conf == null) return;
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
                DrawBeforeBody(itemsProperty);
                for (var i = 0; i < itemsProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical();
                    folds[i] = EditorGUILayout.Foldout(folds[i], string.Format("Item [{0}]", i));
                    if (!folds[i]) continue;
                    var item = itemsProperty.GetArrayElementAtIndex(i);
                    DrawItemProperty(item, i);
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = Color.green;
                    if (GUILayout.Button("+"))
                    {
                        BeforeInsertItem(i);
                        itemsProperty.InsertArrayElementAtIndex(i);
                        ArrayUtility.Insert(ref folds, i, false);
                        AfterInsertItem(i);
                    }
                    GUI.color = Color.red;
                    if (itemsProperty.arraySize > 1 && GUILayout.Button("-"))
                    {
                        BeforeDeleteItem(i);
                        itemsProperty.DeleteArrayElementAtIndex(i);
                        ArrayUtility.RemoveAt(ref folds, i);
                        AfterDeleteItem(i);
                    }
                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();
                }
                serializedObject.ApplyModifiedProperties();
                DrawAfterBody(itemsProperty);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(12f);
            importOption = EditorGUILayout.Foldout(importOption, "Import And Export");
            if (importOption)
            {
                var id = Mathf.Abs(serializedObject.targetObject.GetInstanceID());
                var name = EditorPrefs.GetString(id.ToString(), target.name + "_" + id.ToString());
                var fileName = EditorGUILayout.TextField("FileName", name);
                if (fileName != name) EditorPrefs.SetString(id.ToString(), fileName);

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
            var genericType = target.GetType().BaseType.GetGenericArguments().LastOrDefault();
            DrawFieldProperty(property, property.displayName, genericType);
        }

        protected virtual void DrawFieldProperty(SerializedProperty property,
            string propertyName,
            Type type = null,
            FieldInfo fieldInfo = null)
        {
            if (type == typeof(Sprite))
            {
                var name = property.objectReferenceValue ? property.objectReferenceValue.name : string.Empty;
                var sprite = property.objectReferenceValue as Sprite;
                property.objectReferenceValue = EditorGUILayout.ObjectField(name,
                    sprite,
                    typeof(Sprite),
                    true);
            }
            else if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField(property.displayName);
                DrawArrayField(property);
                GUILayout.Space(5f);
            }
            else if (property.propertyType == SerializedPropertyType.Generic)
            {
                if (type == null)
                {
                    Debug.LogErrorFormat("Unknow type {0}, cannot draw this property!", property.type);
                    return;
                }
                EditorGUILayout.LabelField(GUIContent.none, GUI.skin.horizontalSlider);
                if (fieldInfo != null)
                {
                    var attr = fieldInfo.GetCustomAttribute<HeaderAttribute>();
                    if (attr != null) GUILayout.Label(attr.header, GUILayout.ExpandWidth(false));
                }
                var fields = type.GetFields();
                if (fields.Length == 0) return;
                for (var i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    if (field.IsNotSerialized) continue;
                    var subProperty = property.FindPropertyRelative(field.Name);
                    if (subProperty == null) continue;
                    DrawFieldProperty(subProperty, field.Name, field.FieldType, field);
                }
                EditorGUILayout.Separator();
            }
            else
            {
                EditorGUILayout.PropertyField(property);
            }
        }

        protected virtual void DrawArrayField(SerializedProperty subProperty)
        {
            if (subProperty.arraySize <= 0) subProperty.arraySize++;
            Type elementType = null;
            if (subProperty.propertyType == SerializedPropertyType.Generic)
            {
                var rootItemType = target.GetType().BaseType.GetGenericArguments().LastOrDefault();
                var name = UnityEditorUtility.TrimPointerName(subProperty.arrayElementType);
                var subType = rootItemType.GetFields()
                    .Where(f => f.FieldType.IsArray && f.FieldType.GetElementType().FullName.Contains(name))
                    .FirstOrDefault();
                if (subType != null)
                    elementType = subType.FieldType.GetElementType();
            }
            for (var i = 0; i < subProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                var first = subProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical();
                DrawFieldProperty(first, string.Format("[{0}]", i), elementType);
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.Width(24)))
                    subProperty.InsertArrayElementAtIndex(i);
                if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(24)))
                    subProperty.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
