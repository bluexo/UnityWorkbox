using System.Collections;

namespace UnityEngine
{
    using System;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(EnumMaskFieldAttribute))]
    public class EnumMaskFieldAttrbuteDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EnumMaskFieldAttribute;
            if (!fieldInfo.FieldType.IsEnum) {
                base.OnGUI(position, property, label);
            } else
                property.intValue = EditorGUI.MaskField(position, attr.Label ?? label.text, property.intValue, Enum.GetNames(fieldInfo.FieldType));
        }
    }

#endif

    public class EnumMaskFieldAttribute : PropertyAttribute
    {
        public string Label { get; set; }
    }
}
