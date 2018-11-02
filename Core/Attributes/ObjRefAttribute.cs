using System;
using UnityEngine;

namespace Arthas
{

#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ObjRefAttribute))]
    public class ObjRefAttributePropertyDrawer : PropertyDrawer
    {
        private float height = 32f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer
                && property.propertyType != SerializedPropertyType.String) return;

            var labelRect = new Rect(position.x, position.y, position.width * 3 / 4, height / 2);
            var selectorRect = new Rect(position.x + position.width * 3 / 4 + 10f, position.y, position.width * 1 / 6, height / 2);

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = EditorGUI.IntField(labelRect, property.displayName, property.intValue);
            }
            else if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = EditorGUI.TextField(labelRect, property.displayName, property.stringValue);
            }

            if (GUI.Button(selectorRect, "⊙"))
            {

            }
        }
    }

#endif

    public class ObjRefAttribute : PropertyAttribute
    {
        public Type RefType { get; set; }
        public string FieldName { get; set; }

        public ObjRefAttribute(Type refType, string fieldName)
        {
            RefType = refType;
            FieldName = fieldName;
        }
    }
}
