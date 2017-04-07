using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class GitTools : EditorWindow
{
    public const string gitPath = "C:/Program Files/Git/git-bash.exe";

    [MenuItem("Git/Bash")]
    public static void Git()
    {
        try
        {
            Process.Start(gitPath);
        }
        catch
        {
            UnityEngine.Debug.LogError("Cannot found Git , please add \"git.exe\" directory to your environment variable path");
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
        try
        {
            Process.Start("gitextensions.exe", cmd);
        }
        catch
        {
            UnityEngine.Debug.LogError("Cannot found Git , please add \"gitextensions.exe\" directory to your environment variable path");
        }
    }
}
