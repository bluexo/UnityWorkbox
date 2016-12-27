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
                    EditorGUILayout.Space();
                }
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) {
                ArrayUtility.Insert(ref folds, property.arraySize, false);
                property.arraySize++;
            }
            if (GUILayout.Button("-")) {
                property.arraySize--;
                ArrayUtility.RemoveAt(ref folds, property.arraySize);
            }
            if (GUILayout.Button(">")) {
                for (var i = 0; i < folds.Length; i++) { folds[i] = false; }
            }
            if (GUILayout.Button("∨")) {
                for (var i = 0; i < folds.Length; i++) { folds[i] = true; }
            }
            EditorGUILayout.EndHorizontal();

        }

        public abstract void DrawItemProperty(SerializedProperty property);
    }
#endif

    public class ConfigurableItems<T> : ScriptableObject
    {
        [SerializeField]
        private T[] items;
        public T[] Items { get { return items; } }
    }
}