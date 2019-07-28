using System;
using System.Collections;

using UnityEngine;
using UnityEditor;

using UnityWorkbox.Network;

[CustomEditor(typeof(Networker))]
public class NetworkerEditor : Editor
{
    int connectorIndex = 0, handlerIndex = 0;
    private Type[] connectors = { }, handlers = { };
    private SerializedProperty connTypeName, handlerTypeName;

    private void OnEnable()
    {
        var allTypes = typeof(Networker).Assembly.GetTypes();
        var conns = Array.FindAll(allTypes, t => t.GetInterface(typeof(IConnection).Name) != null && t != typeof(IConnection));
        conns.Foreach(c => ArrayUtility.Add(ref connectors, c));

        var handlerArray = Array.FindAll(allTypes, t => t.GetInterface(typeof(INetworkMessageHandler).Name) != null && t != typeof(INetworkMessageHandler));
        handlerArray.Foreach(c => ArrayUtility.Add(ref handlers, c));

        connTypeName = serializedObject.FindProperty("connectorTypeName");
        handlerTypeName = serializedObject.FindProperty("messageHandlerName");
        var connType = Array.Find(connectors, c => c.FullName.Equals(connTypeName.stringValue, StringComparison.InvariantCultureIgnoreCase));
        connectorIndex = connType == null ? 0 : Array.IndexOf(connectors, connType);
        var handlerType = Array.Find(handlers, h => h.FullName.Equals(handlerTypeName.stringValue, StringComparison.InvariantCultureIgnoreCase));
        handlerIndex = handlerType == null ? 0 : Array.IndexOf(handlers, handlerType);
    }

    public override void OnInspectorGUI()
    {
        var rect = EditorGUILayout.GetControlRect(true, 30);
        base.OnInspectorGUI();
        var running = Application.isPlaying && Networker.IsConnected;
        var style = new GUIStyle(EditorStyles.whiteLargeLabel) { richText = true, fontSize = 20 };
        var label = string.Format("{0} [<color={2}>{1}</color>]",
            typeof(Networker),
            running ? "On" : "Off",
            running ? "green" : "red");
        EditorGUI.LabelField(rect, label, style);
        EditorGUI.DrawRect(rect, (running ? Color.cyan : Color.yellow) / 3f);

        string[] connectorNames = { }, handlerNames = { };

        connectors.Foreach(c => ArrayUtility.Add(ref connectorNames, c.Name));
        connectorIndex = EditorGUILayout.Popup("Connector", connectorIndex, connectorNames);
        if (connectorIndex < connectors.Length)
            connTypeName.stringValue = connectors[connectorIndex].FullName;

        handlers.Foreach(c => ArrayUtility.Add(ref handlerNames, c.Name));
        handlerIndex = EditorGUILayout.Popup("MessageHandler", handlerIndex, handlerNames);
        if (handlerIndex < handlers.Length) handlerTypeName.stringValue = handlers[handlerIndex].FullName;

        EditorGUILayout.LabelField(new GUIContent("[Server Address] ?", "Click menu [ NETWORK ] switch or config address!"),
            new GUIContent(NetworkConfiguration.Current.ToString()));

        serializedObject.ApplyModifiedProperties();
    }
}
