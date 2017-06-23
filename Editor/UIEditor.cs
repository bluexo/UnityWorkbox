using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Arthas.UI
{
    [CustomEditor(typeof(BaseUI), true)]
    public class BaseUIEditor : Editor
    {
        [MenuItem("UI/Create UIManager")]
        public static void AddUICanvas()
        {
            var canvas = FindObjectOfType<UIManager>();
            if (!canvas) {
                var go = new GameObject("UIManager");
                canvas = go.AddComponent<UIManager>();
                go.SetActive(true);
            } else {
                Debug.Log("<color=yellow>You alreay have a UIManager !</color>");
            }
            Selection.activeObject = canvas;
        }

        [MenuItem("GameObject/Only Show Selection UI", false, -1)]
        public static void HideOther()
        {
            if (!Selection.activeGameObject || !Selection.activeGameObject.GetComponentInParent<UIManager>())
                return;
            var canvas = FindObjectOfType<UIManager>();
            foreach (Transform trans in canvas.transform) {
                if (trans.gameObject == Selection.activeGameObject)
                    trans.gameObject.SetActive(true);
                else
                    trans.gameObject.SetActive(false);
            }
        }

        [MenuItem("UI/Create UI Script with selection")]
        public static void CreateUIScript() { CreateUIPanel(); }

        public static void CreateUIPanel(bool start = false)
        {
            string name = "StartUI";
            string copyPath = "Assets/Scripts/UI/";
            if (!Directory.Exists(copyPath)) Directory.CreateDirectory(copyPath);
            if (!start) {
                var selected = Selection.activeObject as GameObject;
                if (!selected || selected.transform.parent != UIManager.Instance.transform) {
                    Debug.Log("<color=yellow>Selected object is null or not a UICanvas child !</color>");
                    return;
                }
                name = selected.name.Replace(" ", "_").Replace("-", "_");
                copyPath = EditorUtility.SaveFilePanel("Save Script", "Assets/Scripts/UI/", name, "cs");
            } else {
                copyPath = copyPath + "StartUI.cs";
                if (!File.Exists(copyPath)
                    && !EditorUtility.DisplayDialog("Replace", "You already has a StartUI file , \nReplace it?", "√")) {
                    return;
                }
            }
            using (StreamWriter outfile = new StreamWriter(copyPath)) {
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using UnityEngine.UI;");
                outfile.WriteLine("using Arthas.UI;");
                outfile.WriteLine("");
                if (start) outfile.WriteLine("[UIStart]");
                outfile.WriteLine("[UIExclusive]");
                outfile.WriteLine(string.Format("public class {0} : WindowUI<{0}>", name));
                outfile.WriteLine("{");
                outfile.WriteLine("    protected override void Start()");
                outfile.WriteLine("    {");
                outfile.WriteLine("         ");
                outfile.WriteLine("    }");
                outfile.WriteLine("}");
            }
            Debug.Log("Creating Classfile: " + copyPath);
            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
        }

        private bool showEvents, showGroup;
        private SerializedProperty groupProperty;

        private void OnEnable()
        {
            groupProperty = serializedObject.FindProperty("group");
            if (groupProperty.arraySize < 1) {
                groupProperty.arraySize++;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            showGroup = EditorGUILayout.Foldout(showGroup, "UIGroup");
            if (showGroup) {
                for (var i = 0; i < groupProperty.arraySize; i++) {
                    var prop = groupProperty.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();
                    prop.objectReferenceValue = EditorGUILayout.ObjectField(prop.objectReferenceValue, typeof(BaseUI), true, GUILayout.Width(160f));
                    if (prop.objectReferenceValue)
                        GUILayout.Label(string.Format("[{0}]", (prop.objectReferenceValue as BaseUI).SortOrder), GUILayout.Width(45f));
                    if (GUILayout.Button("+", GUILayout.Height(15))) groupProperty.InsertArrayElementAtIndex(i);
                    if (GUILayout.Button("-", GUILayout.Height(15))) groupProperty.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                }
            }
            showEvents = EditorGUILayout.Foldout(showEvents, "UIEvents");
            if (showEvents) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BeforeShow"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AfterShow"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BeforeHide"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AfterHide"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ScriptableBehaviourUI), isFallback = true)]
    public class LuaBehaviourUIEditor : Editor
    {
        private void OnEnable()
        {
            var types = typeof(BaseScriptableInvoker).Assembly.GetTypes();
            var invokerType = Array.Find(types, t => t.IsSubclassOf(typeof(BaseScriptableInvoker)));
            var go = (target as Component).gameObject;
            var comp = go.GetComponent(typeof(BaseScriptableInvoker));
            if (!comp && invokerType != null) go.AddComponent(invokerType);
            serializedObject.ApplyModifiedProperties();
        }
    }
}