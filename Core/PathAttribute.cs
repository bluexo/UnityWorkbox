using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(PathAttribute))]
public class PathAttributeDrawer : PropertyDrawer
{
    public readonly float ratio = .9f;

    public override void OnGUI(Rect p, SerializedProperty property, GUIContent label)
    {
        if (property.stringValue != null) {
            var pathAttr = attribute as PathAttribute;
            property.stringValue = EditorGUI.TextField(new Rect(p.x, p.y, p.width * ratio, p.height),
                property.displayName,
                property.stringValue);
            GUI.color = Color.green;
            if (GUI.Button(new Rect(p.x + p.width * ratio, p.y, p.width * (1 - ratio), p.height), "+")) {
                var path = pathAttr.PathType == PathType.Folder
                    ? EditorUtility.OpenFolderPanel("Select folder", property.stringValue, "")
                    : EditorUtility.OpenFilePanel("Select file", property.stringValue, pathAttr.FileExtension);
                if (!string.IsNullOrEmpty(path)) property.stringValue = path;
            }
            GUI.color = Color.white;
        } else base.OnGUI(p, property, label);
    }
}
#endif

public enum PathType { File, Folder }

public class PathAttribute : PropertyAttribute
{
    public PathAttribute(PathType type = PathType.File, string ext = "")
    {
        PathType = type;
        FileExtension = ext;
    }
    public string FileExtension { get; set; }
    public PathType PathType { get; set; }
}
