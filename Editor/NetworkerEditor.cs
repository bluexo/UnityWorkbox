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
    int connectorIndex, handlerIndex;
    private Type[] connectors = { }, handlers = { };
    private SerializedProperty connTypeName, handlerTypeName;

    private void OnEnable()
    {
        var connTypes = typeof(IConnector).Assembly.GetTypes();
        var conns = Array.FindAll(connTypes, t => t.GetInterface(typeof(IConnector).Name, true) != null && t != typeof(IConnector));
        conns.Foreach(c => ArrayUtility.Add(ref connectors, c));

        var handlerTypes = typeof(INetworkMessageHandler).Assembly.GetTypes();
        var handlerArray = Array.FindAll(connTypes, t => t.GetInterface(typeof(INetworkMessageHandler).Name, true) != null && t != typeof(INetworkMessageHandler));
        handlerArray.Foreach(c => ArrayUtility.Add(ref handlers, c));

        connTypeName = serializedObject.FindProperty("connectorTypeName");
        handlerTypeName = serializedObject.FindProperty("messageHandlerName");
        var connType = Array.Find(connectors, c => c.FullName.Equals(connTypeName.stringValue, StringComparison.InvariantCultureIgnoreCase));
        connectorIndex = Array.IndexOf(connectors, connType);
        var handlerType = Array.Find(handlers, h => h.FullName.Equals(handlerTypeName.stringValue, StringComparison.InvariantCultureIgnoreCase));
        handlerIndex = Array.IndexOf(handlers, handlerType);
    }

    public override void OnInspectorGUI()
    {
        var rect = EditorGUILayout.GetControlRect(true, 20);
        base.OnInspectorGUI();
        EditorGUI.LabelField(rect, typeof(Networker).ToString(), EditorStyles.whiteLargeLabel);
        EditorGUI.DrawRect(rect, Color.yellow / 2);

        string[] connectorNames = { }, handlerNames = { };

        connectors.Foreach(c => ArrayUtility.Add(ref connectorNames, c.Name));
        connectorIndex = EditorGUILayout.Popup("Connector", connectorIndex, connectorNames);
        connTypeName.stringValue = connectors[connectorIndex].FullName;

        handlers.Foreach(c => ArrayUtility.Add(ref handlerNames, c.Name));
        handlerIndex = EditorGUILayout.Popup("MessageHandler", handlerIndex, handlerNames);
        handlerTypeName.stringValue = handlers[handlerIndex].FullName;

        EditorGUILayout.LabelField(new GUIContent("[Server Address] ?", "Click menu [ NETWORK ] switch or config address!"), new GUIContent(NetworkConfiguration.Current.ToString()));
        serializedObject.ApplyModifiedProperties();
    }
}
