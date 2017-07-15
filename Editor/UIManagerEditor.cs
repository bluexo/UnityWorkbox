using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Arthas.UI
{
    [CustomEditor(typeof(UIManager), true)]
    public class UIManagerEditor : Editor
    {
        private void OnEnable()
        {
            var panels = serializedObject.FindProperty("preloadPanels");
            if (panels != null && panels.isArray && panels.arraySize <= 0) panels.arraySize++;

            var canvasObject = (target as UIManager).gameObject;
            var ui = serializedObject.FindProperty("startUI");
            if (!ui.objectReferenceValue) {
                var type = GetChildStartUI();
                if (type != null) {
                    Transform child = canvasObject.transform.Find(type.Name);
                    if (!child) {
                        var obj = new GameObject(type.Name);
                        child = obj.transform;
                        child.transform.SetParent(canvasObject.transform);
                    }
                    var comp = child.GetComponent(type);
                    if (!comp)
                        comp = child.gameObject.AddComponent(type);
                    ui.objectReferenceValue = comp;
                } else {
                    if (EditorUtility.DisplayDialog("Create Script", "You must be have a StartUI class, \ncreate now?", "√")) {
                        BaseUIEditor.CreateUIPanel(true);
                    } else
                        Debug.LogError(@"UISystem initialize fail , Cannot found start ui which has a <color=cyan>[UIStart]</color> Attribute and inherit <color=cyan>:WindowUI<T></color>");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        public static Type GetChildStartUI()
        {
            var uiComps = Assembly.GetAssembly(typeof(BaseUI)).GetTypes();
            return Array.Find(uiComps, ui => ui.IsSubclassOf(typeof(BaseUI)) && ui.IsDefined(typeof(UIStartAttribute), true));
        }
    }
}