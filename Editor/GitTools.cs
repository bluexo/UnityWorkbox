using System;
using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;

public class GitTools : EditorWindow
{
    public const string gitPathKey = "gitPath", gitExtensionPathKey = "gitExtensionPath";

    public static string GitPath
    {
        get { return EditorPrefs.GetString(gitPathKey, string.Empty); }
        set { EditorPrefs.SetString(gitPathKey, value); }
    }

    public static string GitExtensionPath
    {
        get { return EditorPrefs.GetString(gitExtensionPathKey, string.Empty); }
        set { EditorPrefs.SetString(gitExtensionPathKey, value); }
    }

    [MenuItem("Git/Options", priority = 1)]
    public static void Open()
    {
        var win = GetWindow<GitTools>();
        win.minSize = new Vector2(320, 100);
        win.titleContent = new GUIContent("GitConfig");
        InitializeConfig();
    }

    static void InitializeConfig()
    {
        if (!string.IsNullOrEmpty(GitPath)
            && File.Exists(GitPath)
            && !string.IsNullOrEmpty(GitExtensionPath)
            && File.Exists(GitExtensionPath)) return;
        var pathString = Environment.GetEnvironmentVariable("PATH");
        var paths = pathString.Split(';');
        var files = new List<string>();
        for (var i = 0; i < paths.Length; i++) {
            var path = paths[i];
            if (!Directory.Exists(path)) continue;
            files.AddRange(Directory.GetFiles(path, "*.exe"));
        }
        GitPath = files.Find(f => f.EndsWith("git-bash.exe", StringComparison.InvariantCultureIgnoreCase));
        GitExtensionPath = files.Find(f => f.EndsWith("GitExtensions.exe", StringComparison.InvariantCultureIgnoreCase));
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Git[bash|cmd]" , GUILayout.Width(85));
        EditorGUILayout.TextField(GitPath);
        if (GUILayout.Button("+")) GitPath = EditorUtility.OpenFilePanel("Select file", GitPath.Replace(@"/", "\\"), "exe");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("GitExtensions", GUILayout.Width(85));
        EditorGUILayout.TextField(GitExtensionPath);
        if (GUILayout.Button("+")) GitExtensionPath = EditorUtility.OpenFilePanel("Select file", GitExtensionPath.Replace(@"/", "\\"), "exe");
        EditorGUILayout.EndHorizontal();
    }

    #region Menus
    [MenuItem("Git/Bash", priority = 1)]
    public static void Git()
    {
        try {
            Process.Start(GitPath);
        }
        catch {
            Debug.LogError("Cannot found Git-Bash.exe , please config your git path!");
        }
    }

    [MenuItem("Git/GitExtensions/Browse")]
    public static void Add()
    {
        GitCommand(string.Format("browse {0}", Application.dataPath));
    }

    [MenuItem("Git/GitExtensions/Commit")]
    public static void Commit()
    {
        GitCommand("commit");
    }

    [MenuItem("Git/GitExtensions/Pull")]
    public static void Pull()
    {
        GitCommand("pull");
    }

    [MenuItem("Git/GitExtensions/Push")]
    public static void Push()
    {
        GitCommand("push");
    }

    public static void GitCommand(string cmd)
    {
        try {
            Process.Start(GitExtensionPath, cmd);
        }
        catch {
            Open();
            Debug.LogError("Cannot found GitExtension.exe , please config your gitextension path!");
        }
    }
    #endregion
}
