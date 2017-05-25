using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;

public class CommandEditor : EditorWindow
{
    public enum CommandType { String, Enum }
    private CommandType cmdType;
    public const string kCommandKey = "Commands", kDefaultCmd = "[Empty]";

    [MenuItem("Network/Command/Open", priority = 10)]
    public static void OpenCommandEditor()
    {
        var window = GetWindow<CommandEditor>();
        window.minSize = new Vector2(240, 480);
        if (!EditorPrefs.HasKey(kCommandKey)) EditorPrefs.SetString(kCommandKey, string.Empty);
    }

    [MenuItem("Network/Command/Clear", priority = 10)]
    public static void ClearCommands()
    {
        EditorPrefs.DeleteKey(kCommandKey);
    }

    public static void GenerateCommands(CommandType cmdType)
    {
        var commands = EditorPrefs.GetString(kCommandKey).Split('|');
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Scripts/Command.cs");
        if (Directory.Exists(path)) Directory.CreateDirectory(path);
        using (var writer = new StreamWriter(path))
        {
            writer.WriteLine("namespace Arthas.Network");
            writer.WriteLine("{");
            writer.WriteLine("  public {0} Command", cmdType == CommandType.String ? "class" : "enum");
            writer.WriteLine("  {");
            for (var i = 0; i < commands.Length; i++)
            {
                if (string.IsNullOrEmpty(commands[i])) continue;
                var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
                var arr = commands[i].ToCharArray();
                arr[0] = textInfo.ToUpper(arr[0]);
                switch (cmdType)
                {
                    case CommandType.String:
                        writer.WriteLine("      public const string {0} = \"{1}\";", new string(arr), commands[i]);
                        break;
                    case CommandType.Enum:
                        writer.WriteLine("      {0} = {1},", commands[i], i);
                        break;
                }
            }
            writer.WriteLine("  }");
            writer.WriteLine("}");
        }
        AssetDatabase.Refresh();
    }

    private void OnGUI()
    {
        var generate = GUILayout.Button("GENERATE", GUILayout.Height(45f));
        cmdType = (CommandType)EditorGUILayout.EnumPopup("CommandType", cmdType);
        if (generate) GenerateCommands(cmdType);
        var cmdString = EditorPrefs.GetString(kCommandKey);
        var commands = new List<string>(cmdString.Split('|'));
        EditorGUILayout.BeginVertical();
        for (var i = 0; i < commands.Count; i++)
        {
            if (string.IsNullOrEmpty(commands[i])) continue;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("[{0}]", i), GUILayout.Height(16f), GUILayout.Width(20f));
            commands[i] = EditorGUILayout.TextField(commands[i], GUILayout.Height(16f));
            if (GUILayout.Button("-", GUILayout.Width(20f))) commands.Remove(commands[i]);
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("+") && !commands.Contains(kDefaultCmd))
            commands.Add(kDefaultCmd);
        EditorGUILayout.EndVertical();
        cmdString = string.Empty;
        commands.ForEach(r => { if (!string.IsNullOrEmpty(r)) cmdString += "|" + r; });
        EditorPrefs.SetString(kCommandKey, cmdString);
    }
}
