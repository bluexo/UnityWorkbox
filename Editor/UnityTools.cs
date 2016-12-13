using UnityEditor;
using UnityEngine;

public static class UnityTools
{
    [MenuItem("GameObject/Apply change %&H", false, -100)]
    static public void ApplyChanges()
    {
        ApplyPrefabChanges(false);
    }

    [MenuItem("GameObject/Apply change and delete from Hierarchy %&D", false, -99)]
    static public void ApplyChangesAndDelete()
    {
        ApplyPrefabChanges(true);
    }

    static public void ApplyPrefabChanges(bool del)
    {
        var objs = Selection.gameObjects;
        //var obj = Selection.activeGameObject;
        if (objs.Length > 0) {
            for (var i = 0; i < objs.Length; i++) {
                var prefab_root = PrefabUtility.FindPrefabRoot(objs[i]);
                var prefab_src = PrefabUtility.GetPrefabParent(prefab_root);
                if (prefab_src != null) {
                    PrefabUtility.ReplacePrefab(prefab_root, prefab_src, ReplacePrefabOptions.ConnectToPrefab);
                    Debug.Log("Updating prefab : " + AssetDatabase.GetAssetPath(prefab_src));
                }
            }
            for (var i = 0; i < objs.Length; i++)
                if (del) Object.DestroyImmediate(objs[i]);
        }
        else {
            Debug.Log("Nothing selected");
        }
    }
}