using System.Diagnostics;
using UnityEditor;

public class GitTools : EditorWindow
{
    public const string gitPath = "C:/Program Files/Git/git-bash.exe";

    [MenuItem("Git/Bash")]
    public static void Git()
    {
        Process.Start(gitPath);
    }

    [MenuItem("Git/Add *")]
    public static void Add()
    {
        Process.Start("git.exe", "git Add *");
    }

    [MenuItem("Git/Pull")]
    public static void Pull()
    {
        Process.Start(gitPath, "git pull");
    }

    [MenuItem("Git/Push")]
    public static void Push()
    {
        Process.Start(gitPath, "git push");
    }
}