﻿using System.Collections;

namespace UnityEngine
{
    using System;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(EnumMaskFieldAttribute))]
    public class MaskAttrbuteDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!fieldInfo.FieldType.IsEnum) {
                base.OnGUI(position, property, label);
                return;
            }
            property.intValue = EditorGUILayout.MaskField("PointerEvent", property.intValue, Enum.GetNames(fieldInfo.FieldType));
        }
    }

#endif

    public class EnumMaskFieldAttribute : PropertyAttribute { }
}
