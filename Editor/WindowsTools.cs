#if UNITY_EDITOR_WIN

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using UnityEditor;

public static class WindowsTools
{
    public static List<Process> processes = new List<Process>();

    [MenuItem("WindowsTools/StartProcess")]
    public static void StartProcess()
    {
        var path = EditorUtility.OpenFilePanel("Open", Directory.GetCurrentDirectory(), "exe");
        if (string.IsNullOrEmpty(path))
            return;
        var psi = new ProcessStartInfo(path, "");
        psi.CreateNoWindow = true;

        psi.RedirectStandardOutput = true;
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        psi.UseShellExecute = false;
        var process = Process.Start(psi);
        UnityEngine.Debug.LogFormat("Start process:{0},MachineName {1}", path,process.MachineName);
    }
}
#endif