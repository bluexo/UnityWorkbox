
using UnityEngine;

namespace UnityWorkbox.Common
{

#if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(ArrayFieldAttribute), true)]
    public class ArrayFieldAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, true);
            var parent = property.serializedObject.FindProperty(property.propertyPath.Split('.')[0]);
            if (SerializedProperty.EqualContents(parent.GetArrayElementAtIndex(parent.arraySize - 1), property))
            {
                if (Event.current.type == EventType.DragPerform) return;
                EditorGUILayout.BeginHorizontal();
                GUI.color = Color.green;
                if (GUILayout.Button("+")) parent.arraySize++;
                GUI.color = Color.red;
                if (GUILayout.Button("-")) parent.arraySize--;
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            //for (var i = 0; i < property.arraySize; i++)
            //{
            //    EditorGUILayout.BeginHorizontal();
            //    var first = property.GetArrayElementAtIndex(i);
            //    EditorGUILayout.PropertyField(property, label ?? new GUIContent(property.displayName));
            //    if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.Width(24)))
            //        property.InsertArrayElementAtIndex(i);
            //    if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(24)))
            //        property.DeleteArrayElementAtIndex(i);
            //    EditorGUILayout.EndHorizontal();
            //}
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.CountInProperty() * 18f;
        }
    }

#endif

    public class ArrayFieldAttribute : PropertyAttribute { }
}
