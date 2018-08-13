using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ValidateComponentAttribute))]
    public class ValidateComponentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            if (property.propertyType != SerializedPropertyType.ObjectReference) return;
            if (property.objectReferenceValue == null) return;
            var go = property.objectReferenceValue as GameObject;
            if (!go) return;

            var target = attribute as ValidateComponentAttribute;
            var comp = go.GetComponent(target.Component);
            if (comp == null)
            {
                Debug.LogError("This gameobject required component :" + target.Component.FullName);
                property.objectReferenceValue = null;
            }
        }
    }
#endif


    public class ValidateComponentAttribute : PropertyAttribute
    {
        public Type Component;

        public ValidateComponentAttribute(Type type)
        {
            Component = type;
        }
    }
}
