using UnityEngine;

namespace UnityWorkbox.Common
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(RenameAttribute))]
    public class RenameAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect p, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as RenameAttribute;
            EditorGUI.PropertyField(p, property, new GUIContent(attr.Name), true);
        }
    }
#endif

    public class RenameAttribute : PropertyAttribute
    {
        public string Name { get; private set; }

        public RenameAttribute(string name)
        {
            Name = name;
        }
    }
}
