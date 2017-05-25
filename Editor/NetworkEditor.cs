using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using Arthas.Network;

[CustomEditor(typeof(TCPNetwork))]
public class NetworkEditor : Editor
{
    private void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        var rect = EditorGUILayout.GetControlRect(true, 20);
        base.OnInspectorGUI();
        EditorGUI.LabelField(rect, typeof(TCPNetwork).ToString(), EditorStyles.whiteLargeLabel);
        EditorGUI.DrawRect(rect, Color.yellow / 2);
    }
}
