using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor.Animations;
using System.Collections.Generic;

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
        if (objs.Length > 0)
        {
            for (var i = 0; i < objs.Length; i++)
            {
                var prefab_root = PrefabUtility.FindPrefabRoot(objs[i]);
                var prefab_src = PrefabUtility.GetPrefabParent(prefab_root);
                if (prefab_src != null)
                {
                    PrefabUtility.ReplacePrefab(prefab_root, prefab_src, ReplacePrefabOptions.ConnectToPrefab);
                    if (del) UnityEngine.Object.DestroyImmediate(prefab_root);
                    Debug.Log("Updating prefab : " + AssetDatabase.GetAssetPath(prefab_src), prefab_src);
                }
            }
        }
        else
        {
            Debug.Log("Nothing selected");
        }
    }

    [MenuItem("Assets/Copy To", priority = -1)]
    public static void CopyTo()
    {
        var path = EditorUtility.OpenFolderPanel("CopyTo", Directory.GetCurrentDirectory(), string.Empty);
        if (string.IsNullOrEmpty(path)) return;
        foreach (var go in Selection.objects)
        {
            var src = Application.dataPath.Replace("Assets", "") + AssetDatabase.GetAssetPath(go);
            var dst = path + src.Substring(src.LastIndexOf("/"));
            File.Copy(src, dst);
            Debug.LogFormat("Copy file from:<color=cyan>{0}</color>\nto:<color=cyan>{1}</color>", src, dst);
        }
    }

    [MenuItem("Tools/Scene/Add All Scenes to BuidSetting")]
    public static void AddAllScenesToBuildSetting()
    {
        var scenesGUIDs = AssetDatabase.FindAssets("t:Scene");
        var sceneInfos = new EditorBuildSettingsScene[scenesGUIDs.Length];
        for (var i = 0; i < scenesGUIDs.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(scenesGUIDs[i]);
            sceneInfos[i] = new EditorBuildSettingsScene(path, true);
        }
        EditorBuildSettings.scenes = sceneInfos;
    }

    private struct SceneInfo { public string name, path; }

    [MenuItem("Tools/Scene/Generate Scenes Script")]
    public static void GeneratorSceneScript()
    {
        var scenes = AssetDatabase.FindAssets("t:Scene");
        var sceneInfos = new SceneInfo[scenes.Length];
        for (var i = 0; i < scenes.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(scenes[i]);
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            sceneInfos[i] = new SceneInfo() { name = scene.name, path = path };
        }

        var copyPath = EditorUtility.SaveFilePanel("Save Script", "Assets/Scripts/", "Scenes", "cs");
        if (string.IsNullOrEmpty(copyPath)) return;
        using (StreamWriter outfile = new StreamWriter(copyPath))
        {
            outfile.WriteLine("using System.Collections.Generic;");
            outfile.WriteLine("");
            outfile.WriteLine(string.Format("public static class Scenes"));
            outfile.WriteLine("{");
            outfile.WriteLine("    public static readonly Dictionary<string,string> ScenePaths = new Dictionary<string,string>()");
            outfile.WriteLine("    {");
            for (var i = 0; i < sceneInfos.Length; i++)
            {
                var name = sceneInfos[i].name;
                if (string.IsNullOrEmpty(name)) continue;
                outfile.WriteLine("         {2} \"{0}\",\"{1}\" {3},", name.ToCodeSymbol(), sceneInfos[i].path, "{", "}");
            }
            outfile.WriteLine("    };");
            outfile.WriteLine("");
            for (var i = 0; i < sceneInfos.Length; i++)
            {
                var name = sceneInfos[i].name;
                if (string.IsNullOrEmpty(name)) continue;
                outfile.WriteLine("    public const string {0} = \"{1}\";", name.ToCodeSymbol(), sceneInfos[i].name);
            }
            outfile.WriteLine("}");
        }
        Debug.Log("Creating Classfile: " + copyPath);
        AssetDatabase.Refresh(ImportAssetOptions.Default);
    }

    [MenuItem("Tools/Animation/Generate Select Animator StringToHash Script")]
    public static void GenerateSelectionAnimatorStates()
    {
        GenerateStringToHash(Selection.objects);
    }

    [MenuItem("Tools/Animation/Generate All Animator StringToHash Script")]
    public static void GenerateAllAnimatorStates()
    {
        var guids = AssetDatabase.FindAssets("t:AnimatorController");
        var assets = new UnityEngine.Object[guids.Length];
        for (var i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            assets[i] = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        }
        GenerateStringToHash(assets);
    }

    private static void GenerateStringToHash(IEnumerable<UnityEngine.Object> assets)
    {
        var paramaterNames = new HashSet<string>();

        foreach (var r in assets)
        {
            var animator = r as UnityEditor.Animations.AnimatorController;
            if (!animator) return;
            var parameters = animator.parameters;
            Array.ForEach(parameters, p =>
            {
                paramaterNames.Add(p.name);
                Debug.LogFormat("Name:<color=yellow>{0}</color>,NameHash:{1}", p.name, p.nameHash);
            });
        }

        if (paramaterNames.Count <= 0) return;

        var copyPath = EditorUtility.SaveFilePanel("Save Script", "Assets/Scripts/", "AnimatorS2H", "cs");
        if (string.IsNullOrEmpty(copyPath)) return;
        var fileInfo = new FileInfo(copyPath);
        using (StreamWriter outfile = new StreamWriter(copyPath))
        {
            outfile.WriteLine("using UnityEngine;");
            outfile.WriteLine("");
            outfile.WriteLine("public static class {0}", fileInfo.Name.Replace(fileInfo.Extension, ""));
            outfile.WriteLine("{");
            foreach (var name in paramaterNames)
            {
                if (string.IsNullOrEmpty(name)) continue;
                outfile.WriteLine("     public static readonly int {0} = Animator.StringToHash(\"{1}\");", name.ToCodeSymbol(), name);
            }
            outfile.WriteLine("}");
        }
        Debug.Log("Creating Classfile: " + copyPath);
        AssetDatabase.Refresh();
    }

    private static void Print(AnimatorStateMachine stm)
    {
        Array.ForEach(stm.states, s => Debug.LogFormat("StateName:<color=cyan>{0}</color>,NameHash:{1}", s.state.name, s.state.nameHash));
        if (stm.stateMachines.Length > 0)
            Array.ForEach(stm.stateMachines, sms => Print(sms.stateMachine));
    }

    [MenuItem("Tools/Tag/Generate Tag Script")]
    public static void GenerateTagScript()
    {
        var tags = InternalEditorUtility.tags;
        var copyPath = EditorUtility.SaveFilePanel("Save Script", "Assets/Scripts/", "Tags", "cs");
        if (string.IsNullOrEmpty(copyPath)) return;
        var fileInfo = new FileInfo(copyPath);
        using (StreamWriter outfile = new StreamWriter(copyPath))
        {
            outfile.WriteLine("");
            outfile.WriteLine("public static class {0}", fileInfo.Name.Replace(fileInfo.Extension, ""));
            outfile.WriteLine("{");
            for (var i = 0; i < tags.Length; i++)
                outfile.WriteLine("     public const string {0} = \"{1}\";", tags[i].ToCodeSymbol(), tags[i]);
            outfile.WriteLine("}");
        }
        Debug.Log("Creating Classfile: " + copyPath);
        AssetDatabase.Refresh();
    }


    [MenuItem("Tools/PlayerPref/Delete All")]
    public static void DeleteAllPlayerPref()
    {
        PlayerPrefs.DeleteAll();
    }
}