using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Text;

public class CommandEditor : EditorWindow
{
    public enum CommandType { String, Enum }
    private CommandType cmdType;
    public const string kCommandKey = "Commands", kDefaultCmd = "0:Login", cmdPath = "Assets/Scripts/Command.cs";

    [MenuItem("Network/Command/Open", priority = 10)]
    public static void OpenCommandEditor()
    {
        var window = GetWindow<CommandEditor>();
        window.minSize = new Vector2(240, 480);
        if (!EditorPrefs.HasKey(kCommandKey)) EditorPrefs.SetString(kCommandKey, kDefaultCmd);
    }

    [MenuItem("Network/Command/Clear", priority = 10)]
    public static void ClearCommands()
    {
        EditorPrefs.DeleteKey(kCommandKey);
    }

    public static void GenerateCommands(CommandType cmdType)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), cmdPath);
        if (Directory.Exists(path)) Directory.CreateDirectory(path);
        using (var writer = new StreamWriter(path)) writer.Write(GetCmdText(cmdType));
        AssetDatabase.Refresh();
    }

    private static string GetCmdText(CommandType cmdType)
    {
        var commands = EditorPrefs.GetString(kCommandKey).Split('|');
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("namespace Arthas.Network");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine(string.Format("      public {0} Command", cmdType == CommandType.String ? "class" : "enum"));
        stringBuilder.AppendLine("      {");
        for (var i = 0; i < commands.Length; i++) {
            if (string.IsNullOrEmpty(commands[i])) continue;
            var cmds = commands[i].Split(':');
            if (cmds.Length < 2) continue;
            var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
            var arr = cmds[1].ToCharArray();
            arr[0] = textInfo.ToUpper(arr[0]);
            switch (cmdType) {
                case CommandType.String:
                    stringBuilder.AppendLine(string.Format("            public const string {0} = \"{1}\";", new string(arr), cmds[1]));
                    break;
                case CommandType.Enum:
                    stringBuilder.AppendLine(string.Format("            {0} = {1},", cmds[1], cmds[0]));
                    break;
            }
        }
        stringBuilder.AppendLine("      }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private void OnGUI()
    {
        var generate = GUILayout.Button("GENERATE", GUILayout.Height(45f));
        GUILayout.Space(5);
        cmdType = (CommandType)EditorGUILayout.EnumPopup("CommandType", cmdType);
        GUILayout.Space(5);
        if (generate) GenerateCommands(cmdType);
        var cmdString = EditorPrefs.GetString(kCommandKey);
        var commands = new List<string>(cmdString.Split('|'));
        EditorGUILayout.BeginVertical();
        for (var i = 0; i < commands.Count; i++) {
            if (string.IsNullOrEmpty(commands[i])) continue;
            var cmds = commands[i].Split(':');
            if (cmds.Length < 2) continue;
            EditorGUILayout.BeginHorizontal();
            if (cmdType == CommandType.Enum) cmds[0] = EditorGUILayout.TextField(string.Format("{0}", cmds[0]), GUILayout.Height(16f), GUILayout.Width(60f));
            else EditorGUILayout.LabelField(string.Format("[{0}]", i), GUILayout.Height(16f), GUILayout.Width(20f));
            cmds[1] = EditorGUILayout.TextField(cmds[1], GUILayout.Height(16f));
            commands[i] = string.Format("{0}:{1}", cmds[0], cmds[1]);
            if (GUILayout.Button("-", GUILayout.Width(20f))) commands.Remove(commands[i]);
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("+")) commands.Add(kDefaultCmd);
        GUILayout.Space(10);
        EditorGUILayout.PrefixLabel("Code Proview");
        GUILayout.TextArea(GetCmdText(cmdType));
        EditorGUILayout.EndVertical();
        cmdString = string.Empty;
        commands.ForEach(r => { if (!string.IsNullOrEmpty(r)) cmdString += "|" + r; });
        EditorPrefs.SetString(kCommandKey, cmdString);
    }
}
