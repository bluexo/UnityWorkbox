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
            EditorGUI.PropertyField(position, property);
            if (property.propertyType != SerializedPropertyType.ObjectReference) return;
            if (property.objectReferenceValue == null) return;
            var go = property.objectReferenceValue as GameObject;
            if (!go) return;

            var target = attribute as RequiredAttribute;
            for (var i = 0; i < target.Components.Length; i++)
            {
                var comp = go.GetComponent(target.Components[i]);
                if (comp == null)
                {
                    Debug.LogError("This gameobject required component :" + target.Components[i].FullName);
                    property.objectReferenceValue = null;
                    break;
                }
            }
        }
    }
#endif


    public class RequiredAttribute : PropertyAttribute
    {
        public Type[] Components;

        public RequiredAttribute(params Type[] types)
        {
            Components = types;
        }
    }
}
