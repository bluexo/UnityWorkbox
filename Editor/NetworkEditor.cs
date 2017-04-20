using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using Arthas.Network;

[CustomEditor(typeof(TCPNetwork))]
public class NetworkEditor : Editor
{
    bool cmdFold = false;

    private void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        var rect = EditorGUILayout.GetControlRect(true, 20);
        base.OnInspectorGUI();
        EditorGUI.LabelField(rect, typeof(TCPNetwork).ToString(), EditorStyles.whiteLargeLabel);
        EditorGUI.DrawRect(rect, Color.yellow / 2);

        cmdFold = EditorGUILayout.Foldout(cmdFold, typeof(CommandType).ToString());
        if (cmdFold)
        {
            var names = Enum.GetNames(typeof(CommandType));
            GUI.contentColor = Color.green;
            foreach (var name in names)
            {
                var content = string.Format("{0} = [{1}]", name, (int)Enum.Parse(typeof(CommandType), name));
                EditorGUILayout.LabelField(content);
            }
            GUI.contentColor = Color.white;
        }
    }
}
