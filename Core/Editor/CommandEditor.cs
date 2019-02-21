using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class CommandEditor : EditorWindow
{
    [Serializable]
    private class Context
    {
        public string nameSpaceName = "Arthas.Network";
        public string className = "Commands";
        public CommandType commandType = CommandType.Int16;
        public List<string> members = new List<string>();
    }

    public enum CommandType
    {
        Int16,
        String,
        Enum,
        Int32
    }

    public enum Lang { CS, Lua }

    public const string kCommandKey = "Commands",
        kDefaultCmdConfigDirectory = "ProjectSettings/NetworkCommandsConfig.json",
        cmdPath = "Assets/Scripts/Commands.cs";
    private static string settingPath;
    private Vector2 scrollViewPosition;
    private Context context;
    private Lang lang = Lang.CS;


    private void OnEnable()
    {
        settingPath = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, kDefaultCmdConfigDirectory);
        if (!File.Exists(settingPath))
            WriteConfig();
        ReadConfig();
    }

    [MenuItem("Network/Command")]
    public static void OpenCommandEditor()
    {
        var window = GetWindow<CommandEditor>();
        window.minSize = new Vector2(240, 480);
        window.titleContent = new GUIContent("Command");
    }

    public void GenerateCommands(CommandType cmdType)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), cmdPath);
        path = EditorUtility.SaveFilePanel("Save File", path, context.className, lang.ToString());
        if (string.IsNullOrWhiteSpace(path)) return;
        if (Directory.Exists(path)) Directory.CreateDirectory(path);
        using (var writer = new StreamWriter(path)) writer.Write(GetCmdText(cmdType));
        AssetDatabase.Refresh();
    }

    public static void GenerateCSharp()
    {
        var dec = new CodeTypeDeclaration();
    }

    private string GetCmdText(CommandType cmdType)
    {
        var commands = context.members;
        var stringBuilder = new StringBuilder();
        if (lang == Lang.CS)
        {
            stringBuilder.AppendLine($"namespace {context.nameSpaceName}");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(string.Format("\tpublic {0} {1}", cmdType == CommandType.Enum ? "enum" : "class", context.className));
        }
        else if (lang == Lang.Lua)
        {
            stringBuilder.AppendLine($"{context.className} =");
            stringBuilder.AppendLine("{");
        }
        if (lang == Lang.CS)
            stringBuilder.AppendLine("\t{");
        for (var i = 0; i < commands.Count; i++)
        {
            if (string.IsNullOrEmpty(commands[i])) continue;
            var cmds = commands[i].Split(':');
            if (cmds.Length < 2) continue;
            if (lang == Lang.CS)
            {
                switch (cmdType)
                {
                    case CommandType.String:
                        stringBuilder.AppendLine(string.Format("\t\tpublic const string {0} = \"{1}\";", cmds[0], cmds[0]));
                        break;
                    case CommandType.Int32:
                        stringBuilder.AppendLine(string.Format("\t\tpublic const int {0} = {1};", cmds[0], cmds[1]));
                        break;
                    case CommandType.Int16:
                        stringBuilder.AppendLine(string.Format("\t\tpublic const short {0} = {1};", cmds[0], cmds[1]));
                        break;
                    case CommandType.Enum:
                        stringBuilder.AppendLine(string.Format("\t\t{0} = {1},", cmds[0], cmds[1]));
                        break;
                }
            }
            else if (lang == Lang.Lua)
            {
                switch (cmdType)
                {
                    case CommandType.Int16:
                    case CommandType.Int32:
                    case CommandType.Enum:
                        stringBuilder.AppendLine(string.Format("\t{0} = {1},", cmds[0], cmds[1]));
                        break;
                    case CommandType.String:
                        stringBuilder.AppendLine(string.Format("\t{0} = \"{1}\",", cmds[0], cmds[0]));
                        break;
                }
            }
        }
        if (lang == Lang.CS)
        {
            stringBuilder.AppendLine("\t}");
        }

        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private void ReadConfig()
    {
        var json = File.ReadAllText(settingPath);
        context = string.IsNullOrWhiteSpace(json)
            ? new Context()
            : JsonUtility.FromJson<Context>(json);
    }

    private void WriteConfig()
    {
        var json = JsonUtility.ToJson(context ?? new Context());
        File.WriteAllText(settingPath, json);
    }

    private void OnGUI()
    {
        var color = lang == Lang.CS
            ? new Color(.11f, .93f, .8f)
            : new Color(.24f, .38f, .92f);
        GUI.backgroundColor = color;
        var generate = GUILayout.Button("GENERATE", GUILayout.Height(45f));
        GUI.backgroundColor = Color.white;
        GUILayout.Space(5);
        lang = (Lang)EditorGUILayout.EnumPopup("Language", lang);
        context.nameSpaceName = EditorGUILayout.TextField("NameSpaceName", context.nameSpaceName);
        context.className = EditorGUILayout.TextField("ClassName", context.className);
        if (string.IsNullOrEmpty(context.className)) context.className = "Commands";
        context.commandType = (CommandType)EditorGUILayout.EnumPopup("CommandType", context.commandType);
        GUILayout.Space(5);
        if (generate) GenerateCommands(context.commandType);
        var commands = context.members;
        scrollViewPosition = EditorGUILayout.BeginScrollView(scrollViewPosition);
        for (var i = 0; i < commands.Count; i++)
        {
            if (string.IsNullOrEmpty(commands[i])) continue;
            var cmds = commands[i].Split(':');
            if (cmds.Length < 2) continue;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", GUILayout.Width(45));
            cmds[0] = EditorGUILayout.TextField(cmds[0], GUILayout.Height(16f));
            EditorGUILayout.LabelField("Value:", GUILayout.Width(48));
            if (context.commandType == CommandType.String)
                EditorGUILayout.LabelField(string.Format("[{0}]", i), GUILayout.Height(16f));
            else cmds[1] = EditorGUILayout.TextField(string.Format("{0}", cmds[1]), GUILayout.Height(16f));
            commands[i] = string.Format("{0}:{1}", cmds[0], cmds[1]);
            if (GUILayout.Button("-", GUILayout.Width(20f))) commands.Remove(commands[i]);
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("+"))
            context.members.Add($":");
        GUILayout.Space(10);
        EditorGUILayout.PrefixLabel("Code Preview");
        GUILayout.TextArea(GetCmdText(context.commandType));
        WriteConfig();
        EditorGUILayout.EndScrollView();
    }
}
