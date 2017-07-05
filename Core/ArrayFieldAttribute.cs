using System.Collections;

namespace UnityEngine
{
    using System;
    using System.Collections.Generic;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(ArrayFieldAttribute))]
    public class ArrayFieldAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label);
            var parent = property.serializedObject.FindProperty(property.propertyPath.Split('.')[0]);
            if (SerializedProperty.EqualContents(parent.GetArrayElementAtIndex(parent.arraySize - 1), property)) {
                if (Event.current.type == EventType.dragPerform) return;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+")) parent.arraySize++;
                if (GUILayout.Button("-")) parent.arraySize--;
                EditorGUILayout.EndHorizontal();
            }
        }
    }

#endif

    public class ArrayFieldAttribute : PropertyAttribute { }
}
