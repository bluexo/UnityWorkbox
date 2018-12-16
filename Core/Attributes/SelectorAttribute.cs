using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas
{
    using System.Linq;
    using System.Reflection;
#if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(SelectorAttribute))]
    public class SelectorAttributeDrawer : PropertyDrawer
    {
        private int index;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as SelectorAttribute;
            var targetType = property.serializedObject.targetObject.GetType();
            IList<string> selectArray = null;
            var propertyMember = targetType.GetProperty(attr.propertyName);
            if (propertyMember != null)
            {
                var obj = propertyMember.GetValue(property.serializedObject.targetObject);
                selectArray = obj as IList<string>;
            }
            else
            {
                var fieldMember = targetType.GetField(attr.propertyName);
                if (fieldMember == null) return;
                var obj = fieldMember.GetValue(property.serializedObject.targetObject);
                selectArray = obj as IList<string>;
            }

            if (selectArray != null)
            {
                EditorGUILayout.Space();
                index = EditorGUILayout.Popup(property.name, index, selectArray.ToArray());
                property.stringValue = selectArray[index];
                EditorGUILayout.Space();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
            else
            {
                EditorGUILayout.PropertyField(property);
            }
        }
    }

#endif

    [AttributeUsage(AttributeTargets.Field)]
    public class SelectorAttribute : PropertyAttribute
    {
        public string propertyName { get; set; }

        public SelectorAttribute(string prop)
        {
            propertyName = prop;
        }
    }
}
