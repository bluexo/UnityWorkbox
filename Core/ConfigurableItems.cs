using UnityEngine;

namespace Arthas.Common
{
#if UNITY_EDITOR
    using UnityEditor;
    public abstract class ConfigurableItemsEditor : Editor
    {
        private SerializedProperty property;

        private bool[] folds;
       
        private void OnEnable()
        {
            property = serializedObject.FindProperty("items");
            if (property.arraySize <= 0)
                property.arraySize++;
            folds = new bool[property.arraySize];
        }

        public override void OnInspectorGUI()
        {
            var rect = EditorGUILayout.BeginVertical();
            for (var i = 0; i < property.arraySize; i++) {
                folds[i] = EditorGUILayout.Foldout(folds[i], string.Format("Item [{0}]", i));
                if (folds[i]) {
                    var item = property.GetArrayElementAtIndex(i);
                    DrawItemProperty(item);
                }
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndVertical();
            var btnRect = EditorGUILayout.GetControlRect(GUILayout.Height(30));
            if (GUI.Button(new Rect(btnRect.x, btnRect.y, btnRect.width / 2, btnRect.height), "+")) {
                ArrayUtility.Insert(ref folds, property.arraySize, false);
                property.arraySize++;
            }
            if (GUI.Button(new Rect(btnRect.x + btnRect.width / 2, btnRect.y, btnRect.width / 2, btnRect.height), "-")) {
                property.arraySize--;
                ArrayUtility.RemoveAt(ref folds, property.arraySize);
            }
        }

        public abstract void DrawItemProperty(SerializedProperty property);
    }
#endif

    public class ConfigurableItems<T> : ScriptableObject
    {
        public T[] items;
    }
}