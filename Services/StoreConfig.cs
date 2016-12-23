using UnityEngine;

namespace Arthas.Common
{
#if UNITY_EDITOR

    using UnityEditor;

    [CustomEditor(typeof(StoreConfig))]
    public class StoreItemConfigEditor : Editor
    {
        private SerializedProperty property;
        private bool[] folds;

        private void OnEnable()
        {
            property = serializedObject.FindProperty("storeItems");
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
                    var id = item.FindPropertyRelative("id");
                    id.intValue = EditorGUILayout.IntSlider("Id", id.intValue, 1, 1000);
                    var name = item.FindPropertyRelative("name");
                    name.stringValue = EditorGUILayout.TextField("Name", name.stringValue);
                    var desc = item.FindPropertyRelative("description");
                    desc.stringValue = EditorGUILayout.TextField("Description", desc.stringValue);
                    var price = item.FindPropertyRelative("price");
                    price.intValue = EditorGUILayout.IntSlider("Price", price.intValue, 10, 100000);
                    var count = item.FindPropertyRelative("count");
                    count.intValue = EditorGUILayout.IntSlider("Count", count.intValue, 1, 10000);
                    EditorGUILayout.BeginHorizontal();
                    var icon = item.FindPropertyRelative("icon");
                    EditorGUILayout.PropertyField(icon);
                    var iconRect = EditorGUILayout.GetControlRect(GUILayout.Width(100), GUILayout.Height(100));
                    EditorGUI.DrawRect(iconRect, Color.black / 2);
                    var sprite = icon.objectReferenceValue as Sprite;
                    if (sprite)
                        EditorGUI.DrawTextureTransparent(iconRect, sprite.texture, ScaleMode.StretchToFill);
                    EditorGUILayout.EndHorizontal();
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
    }

#endif

    [System.Serializable]
    public class Item
    {
        public int id;
        public string name;
        public string description;
        public int price;
        public int count;
        public Sprite icon;
    }

    [CreateAssetMenu(fileName = "StoreItemConfig", menuName = "Configs/Create StoreItemConfig")]
    public class StoreConfig : ScriptableObject
    {
        public Item[] storeItems;
    }
}