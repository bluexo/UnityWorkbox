using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using Arthas.Network;
using System.Reflection;

[CustomEditor(typeof(Networker))]
public class NetworkerEditor : Editor
{
    int connectorIndex = 0, handlerIndex = 0;
    private Type[] connectors = { }, handlers = { };
    private SerializedProperty connTypeName, handlerTypeName;

    private void OnEnable()
    {
        var allTypes = typeof(Networker).Assembly.GetTypes();

        var conns = Array.FindAll(allTypes, t => t.GetInterface(typeof(IConnector).Name, true) != null && t != typeof(IConnector));
        conns.Foreach(c => ArrayUtility.Add(ref connectors, c));

        var handlerArray = Array.FindAll(allTypes, t => t.GetInterface(typeof(INetworkMessageHandler).Name, true) != null && t != typeof(INetworkMessageHandler));
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
        var rect = EditorGUILayout.GetControlRect(true, 20);
        base.OnInspectorGUI();
        EditorGUI.LabelField(rect, typeof(Networker).ToString(), EditorStyles.whiteLargeLabel);
        EditorGUI.DrawRect(rect, (Application.isPlaying && Networker.IsConnected ? Color.green : Color.yellow) / 2f);

        string[] connectorNames = { }, handlerNames = { };

        connectors.Foreach(c => ArrayUtility.Add(ref connectorNames, c.Name));
        connectorIndex = EditorGUILayout.Popup("Connector", connectorIndex, connectorNames);
        if (connectorIndex < connectors.Length)
            connTypeName.stringValue = connectors[connectorIndex].FullName;

        handlers.Foreach(c => ArrayUtility.Add(ref handlerNames, c.Name));
        handlerIndex = EditorGUILayout.Popup("MessageHandler", handlerIndex, handlerNames);
        if (handlerIndex < handlers.Length)
            handlerTypeName.stringValue = handlers[handlerIndex].FullName;

        EditorGUILayout.LabelField(new GUIContent("[Server Address] ?", "Click menu [ NETWORK ] switch or config address!"), new GUIContent(NetworkConfiguration.Current.ToString()));

        serializedObject.ApplyModifiedProperties();
    }
}
