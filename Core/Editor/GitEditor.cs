using System;
using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

public class GitEditor : EditorWindow
{
    public const string gitPathKey = "gitBashPath", gitExtensionPathKey = "gitExtensionPath";

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
        var win = GetWindow<GitEditor>();
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
        var paths = Array.FindAll(pathString.Split(';'), s => s.IndexOf("git", StringComparison.InvariantCultureIgnoreCase) > 0);
        var files = new List<FileInfo>();
        for (var i = 0; i < paths.Length; i++) {
            var path = paths[i];
            if (!Directory.Exists(path)) continue;
            var dir = new DirectoryInfo(path);
            files.AddRange(dir.GetFiles());
            files.AddRange(dir.Parent.GetFiles());
        }

        GitPath = files.Find(f => {
            var hasBash = f.FullName.IndexOf("bash", StringComparison.InvariantCultureIgnoreCase) > 0;
            var hasCmd = f.FullName.IndexOf("cmd", StringComparison.InvariantCultureIgnoreCase) > 0;
            return hasBash || hasCmd;
        }).FullName;
        GitExtensionPath = files.Find(f => f.FullName.EndsWith("GitExtensions.exe", StringComparison.InvariantCultureIgnoreCase)).FullName;
    }

    private void OnGUI()
    {
        GUILayout.Space(10f);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Git[bash|cmd]", GUILayout.Width(85));
        EditorGUILayout.TextField(GitPath);
        if (GUILayout.Button("⊙", EditorStyles.miniButtonMid, GUILayout.Width(25f)))
            GitPath = EditorUtility.OpenFilePanel("Select file", GitPath.Replace(@"/", "\\"), "exe");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("GitExtensions", GUILayout.Width(85));
        EditorGUILayout.TextField(GitExtensionPath);
        if (GUILayout.Button("⊙", EditorStyles.miniButtonMid, GUILayout.Width(25f)))
            GitExtensionPath = EditorUtility.OpenFilePanel("Select file", GitExtensionPath.Replace(@"/", "\\"), "exe");
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10f);
        EditorGUILayout.BeginHorizontal();
        var gitUrl = "https://git-scm.com/downloads";
        EditorGUILayout.LabelField(gitUrl);
        if (GUILayout.Button("Download Git", GUILayout.Width(150f))) Application.OpenURL(gitUrl);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        var gitextUrl = "https://github.com/gitextensions/gitextensions/releases";
        EditorGUILayout.LabelField(gitextUrl);
        if (GUILayout.Button("Download GitExtensions", GUILayout.Width(150f))) Application.OpenURL(gitextUrl);
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
