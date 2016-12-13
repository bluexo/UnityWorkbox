using System.IO;
using UnityEditor;
using UnityEngine;

namespace Arthas.Client.UI
{
    public class UIEditor : Editor
    {
        [MenuItem("UI/Create UICanvas")]
        public static void AddUICanvas()
        {
            var canvas = FindObjectOfType<UICanvas>();
            if (!canvas) {
                var go = new GameObject("UICanvas");
                canvas = go.AddComponent<UICanvas>();
                go.SetActive(true);
            }
            else {
                Debug.Log("<color=yellow>You alreay have a UICanvas !</color>");
            }
            Selection.activeObject = canvas;
        }

        [MenuItem("GameObject/Only Show Selection UI", false, -1)]
        public static void HideOther()
        {
            if (!Selection.activeGameObject || !Selection.activeGameObject.GetComponentInParent<UICanvas>())
                return;
            var canvas = FindObjectOfType<UICanvas>();
            foreach (Transform trans in canvas.transform) {
                if (trans.gameObject == Selection.activeGameObject)
                    trans.gameObject.SetActive(true);
                else
                    trans.gameObject.SetActive(false);
            }
        }

        [MenuItem("UI/Create Script with Selection UI")]
        public static void CreateUIScript()
        {
            CreateUIPanel();
        }

        public static void CreateUIPanel(bool start = false)
        {
            string name = "StartUI";
            string copyPath = "Assets/Scripts/UI/";

            if (!start) {
                var selected = Selection.activeObject as GameObject;
                if (!selected || selected.transform.parent != UICanvas.Instance.transform) {
                    Debug.Log("<color=yellow>Selected object is null or not a UICanvas child !</color>");
                    return;
                }
                name = selected.name.Replace(" ", "_");
                name = name.Replace("-", "_");
                copyPath = EditorUtility.SaveFilePanel("Save Script", "Assets/Scripts/UI/", name, "cs");
                if (string.IsNullOrEmpty(copyPath)) return;
            }
            else {
                copyPath = copyPath + "StartUI.cs";
                if (File.Exists(copyPath)
                    && !EditorUtility.DisplayDialog("Replace", "You already has a StartUI file , \nReplace it?", "√")) {
                    return;
                }
            }
            using (StreamWriter outfile = new StreamWriter(copyPath)) {
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using UnityEngine.UI;");
                outfile.WriteLine("using Arthas.Client.UI;");
                outfile.WriteLine("");
                outfile.WriteLine(start ? "[UIStart]" : "");
                outfile.WriteLine("[UIHeader]");
                outfile.WriteLine("[UIOrder(OrderIndex = 1)]");
                outfile.WriteLine(string.Format("public class {0} : WindowUI<{0}>", name));
                outfile.WriteLine("{");
                outfile.WriteLine("     protected override void Start()");
                outfile.WriteLine("     {");
                outfile.WriteLine("         ");
                outfile.WriteLine("     }");
                outfile.WriteLine("}");
            }
            Debug.Log("Creating Classfile: " + copyPath);
            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
        }
    }
}