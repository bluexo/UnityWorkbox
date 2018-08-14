using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            if (property.propertyType != SerializedPropertyType.ObjectReference) return;
            if (property.objectReferenceValue == null) return;
            var go = property.objectReferenceValue as GameObject;
            if (!go) return;

            var target = attribute as RequiredAttribute;
            var comp = go.GetComponent(target.Component);
            if (comp == null)
            {
                Debug.LogError("This gameobject required component :" + target.Component.FullName);
                property.objectReferenceValue = null;
            }
        }
    }
#endif


    public class RequiredAttribute : PropertyAttribute
    {
        public Type Component;

        public RequiredAttribute(Type type)
        {
            Component = type;
        }
    }
}
