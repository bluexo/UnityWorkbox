using System.IO;
using UnityEditor;
using UnityEngine;

public static class UnityEditorTools
{
    [MenuItem("GameObject/Apply change %&H", false, -100)]
    static public void ApplyChanges()
    {
        ApplyPrefabChanges(false);
    }

    [MenuItem("GameObject/Apply change and delete from Hierarchy %&D", false, -99)]
    static public void ApplyChangesAndDelete()
    {
        ApplyPrefabChanges(true);
    }

    static public void ApplyPrefabChanges(bool del)
    {
        var objs = Selection.gameObjects;
        if (objs.Length > 0) {
            for (var i = 0; i < objs.Length; i++) {
                var prefab_root = PrefabUtility.FindPrefabRoot(objs[i]);
                var prefab_src = PrefabUtility.GetPrefabParent(prefab_root);
                if (prefab_src != null) {
                    PrefabUtility.ReplacePrefab(prefab_root, prefab_src, ReplacePrefabOptions.ConnectToPrefab);
                    if (del) Object.DestroyImmediate(prefab_root);
                    Debug.Log("Updating prefab : " + AssetDatabase.GetAssetPath(prefab_src));
                }
            }
        } else {
            Debug.Log("Nothing selected");
        }
    }

    [MenuItem("Tools/Scene/Add All Scenes to BuidSetting")]
    public static void AddAllScenesToBuildSetting()
    {
        var scenesGUIDs = AssetDatabase.FindAssets("t:Scene");
        var sceneInfos = new EditorBuildSettingsScene[scenesGUIDs.Length];
        for (var i = 0; i < scenesGUIDs.Length; i++) {
            var path = AssetDatabase.GUIDToAssetPath(scenesGUIDs[i]);
            sceneInfos[i] = new EditorBuildSettingsScene(path, true);
        }
        EditorBuildSettings.scenes = sceneInfos;
    }

    private struct SceneInfo
    { public string name, path; }

    [MenuItem("Tools/Scene/Generate Scenes Script")]
    public static void GeneratorSceneScript()
    {
        var scenes = AssetDatabase.FindAssets("t:Scene");
        var sceneInfos = new SceneInfo[scenes.Length];
        for (var i = 0; i < scenes.Length; i++) {
            var path = AssetDatabase.GUIDToAssetPath(scenes[i]);
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            sceneInfos[i] = new SceneInfo() { name = scene.name, path = path };
        }

        var copyPath = EditorUtility.SaveFilePanel("Save Script", "Assets/Scripts/", "Scenes", "cs");
        if (string.IsNullOrEmpty(copyPath)) return;
        using (StreamWriter outfile = new StreamWriter(copyPath)) {
            outfile.WriteLine("using System.Collections.Generic;");
            outfile.WriteLine("");
            outfile.WriteLine(string.Format("public static class Scenes"));
            outfile.WriteLine("{");
            outfile.WriteLine("    public static readonly Dictionary<string,string> ScenePaths = new Dictionary<string,string>()");
            outfile.WriteLine("    {");
            for (var i = 0; i < sceneInfos.Length; i++) {
                outfile.WriteLine("         {2} \"{0}\",\"{1}\" {3},", sceneInfos[i].name, sceneInfos[i].path, "{", "}");
            }
            outfile.WriteLine("    };");
            outfile.WriteLine("");
            for (var i = 0; i < sceneInfos.Length; i++) {
                outfile.WriteLine("    public const string {0} = \"{1}\";", sceneInfos[i].name.Replace(" ", ""), sceneInfos[i].name);
            }
            outfile.WriteLine("}");
        }
        Debug.Log("Creating Classfile: " + copyPath);
        AssetDatabase.Refresh(ImportAssetOptions.Default);
    }


    [MenuItem("Tools/PlayerPref/Delete All")]
    public static void DeleteAllPlayerPref()
    {
        PlayerPrefs.DeleteAll();
    }
}