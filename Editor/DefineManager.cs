/**
 *	Editor Wizard for easily managing global defines in Unity
 *	@khenkel
 */

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

public class DefineManager : EditorWindow
{
    const string DEF_MANAGER_PATH = "Assets/Scripts/Common/Editor/DefineManager.cs";

    enum Compiler
    {
        CSharp,
        Editor,
        UnityScript,
        Boo
    }
    Compiler compiler = Compiler.Editor;

    // http://forum.unity3d.com/threads/93901-global-define/page2
    // Do not modify these paths
    const int COMPILER_COUNT = 4;
    const string CSHARP_PATH = "Assets/mcs.rsp";

    List<string> csDefines = new List<string>();

    [MenuItem("Window/Define Manager")]
    public static void OpenDefManager()
    {
        EditorWindow.GetWindow<DefineManager>(true, "Global Define Manager", true);
        return;
        //TODO PlayerSetting Define
        //var defines = EditorUserBuildSettings.activeScriptCompilationDefines;
        //if (!Array.Exists(defines, d => d.Contains("LUA"))) ArrayUtility.AddRange(ref defines, new string[] { "LUA", "WTF?" });
        //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone | BuildTargetGroup.Android | BuildTargetGroup.iOS, defines.ToArrayString());
    }

    void OnEnable()
    {
        csDefines = ParseRspFile(CSHARP_PATH);
    }

    Vector2 scroll = Vector2.zero;
    void OnGUI()
    {
        Color oldColor = GUI.backgroundColor;
        GUILayout.Label(compiler.ToString() + " User Defines");

        scroll = GUILayout.BeginScrollView(scroll);
        for (int i = 0; i < csDefines.Count; i++) {
            GUILayout.BeginHorizontal();
            csDefines[i] = EditorGUILayout.TextField(csDefines[i]);
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("x", GUIStyle.none, GUILayout.MaxWidth(18))) csDefines.RemoveAt(i);
            GUI.backgroundColor = oldColor;
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(4);

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Add")) csDefines.Add("NEW_DEFINE");
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Apply")) {
            WriteDefines(CSHARP_PATH, csDefines);
            AssetDatabase.ImportAsset(DEF_MANAGER_PATH, ImportAssetOptions.ForceUpdate);
            csDefines = ParseRspFile(CSHARP_PATH);
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Apply All", GUILayout.MaxWidth(64)))
            for (int i = 0; i < COMPILER_COUNT; i++) {
                WriteDefines(CSHARP_PATH, csDefines);
                AssetDatabase.ImportAsset(DEF_MANAGER_PATH, ImportAssetOptions.ForceUpdate);
                csDefines = ParseRspFile(CSHARP_PATH);
            }
        GUILayout.EndHorizontal();
        GUI.backgroundColor = oldColor;
    }

    private List<string> ParseRspFile(string path)
    {
        if (!File.Exists(path)) return new List<string>();

        var lines = File.ReadAllLines(path);
        var defs = new List<string>();

        foreach (string cheese in lines) {
            if (cheese.StartsWith("-define:")) {
                defs.AddRange(cheese.Replace("-define:", "").Split(';'));
            }
        }
        return defs;
    }

    private void WriteDefines(string path, List<string> defs)
    {
        if (defs.Count < 1 && File.Exists(path)) {
            File.Delete(path);
            File.Delete(path + ".meta");
            AssetDatabase.Refresh();
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("-define:");

        for (int i = 0; i < defs.Count; i++) {
            sb.Append(defs[i]);
            if (i < defs.Count - 1) sb.Append(";");
        }

        using (StreamWriter writer = new StreamWriter(path, false)) {
            writer.Write(sb.ToString());
        }
    }
}