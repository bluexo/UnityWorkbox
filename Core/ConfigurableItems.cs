﻿using System;
using System.Xml;
using UnityEngine;

namespace Arthas.Common
{
#if UNITY_EDITOR

    using UnityEditor;

    [CustomEditor(typeof(ConfigurableItems<>), true)]
    public class ConfigurableItemsEditor : Editor
    {
        protected SerializedProperty property;

        protected bool[] folds;

        protected virtual void OnEnable()
        {
            property = serializedObject.FindProperty("items");
            if (property.arraySize <= 0) property.arraySize++;
            folds = new bool[property.arraySize];
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
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

            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.yellow;
            if (GUILayout.Button(">", GUILayout.Height(25f)))
                for (var i = 0; i < folds.Length; i++) { folds[i] = false; }
            if (GUILayout.Button("∨", GUILayout.Height(25f)))
                for (var i = 0; i < folds.Length; i++) { folds[i] = true; }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        public virtual void DrawItemProperty(SerializedProperty property, int index)
        {
            EditorGUILayout.PropertyField(property);
        }
    }

#endif

    public class ConfigurableItems<T> : ScriptableObject
    {
        [SerializeField, HideInInspector]
        protected T[] items;

        public T[] Items { get { return items; } }
    }
}