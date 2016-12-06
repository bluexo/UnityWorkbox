namespace Arthas.Client.UI
{
    using UnityEngine.UI;
    using System;
    using UnityEngine;
    using System.Reflection;

#if UNITY_EDITOR

    using UnityEditor;

#endif

# if UNITY_EDITOR

    [CustomEditor(typeof(MainCanvasUI))]
    public class MainCanvasUIEditor : Editor
    {
        private void OnEnable()
        {
            var canvasObject = MainCanvasUI.Instance.gameObject;
            var ui = serializedObject.FindProperty("startUI");
            if (!ui.objectReferenceValue) {
                var type = GetChildStartUI();
                if (type != null) {
                    Transform child = canvasObject.transform.FindChild(type.Name);
                    if (!child) {
                        var obj = new GameObject(type.Name);
                        child = obj.transform;
                        child.transform.SetParent(MainCanvasUI.Instance.transform);
                    }
                    var comp = child.GetComponent(type);
                    if (!comp)
                        comp = child.gameObject.AddComponent(type);
                    ui.objectReferenceValue = comp;
                }
                else {
                    Debug.LogError(@"UISystem initialize fail , Cannot found start ui which has a <color=cyan>[UIStart]</color> Attribute and inherit <color=cyan>:WindowUI<T></color>");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        public Type GetChildStartUI()
        {
            var uiComps = Assembly.GetAssembly(typeof(BaseUI)).GetTypes();
            for (var i = 0; i < uiComps.Length; i++) {
                var comp = uiComps[i];
                if (!comp.IsSubclassOf(typeof(BaseUI)))
                    continue;
                if (comp.IsDefined(typeof(UIStartAttribute), false)) {
                    return comp;
                }
            }
            return null;
        }
    }

#endif

    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class MainCanvasUI : BaseUI
    {
        public static MainCanvasUI Instance {
            get {
                if (!instance) {
                    instance = FindObjectOfType<MainCanvasUI>();
                }
                return instance ;
            }
        }

        private static MainCanvasUI instance;

        private static Camera canvasCamera;

        public static Camera CanvasCamera {
            get {
                if (!canvasCamera) {
                    var canvas = FindObjectOfType<Canvas>();
                    if (canvas)
                        canvasCamera = canvas.worldCamera;
                    else
                        Debug.LogFormat(RichText.Red("Cannot find Canvas!"));
                }
                return canvasCamera;
            }
        }

        [SerializeField]
        private BaseUI startUI;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        protected override void Start()
        {
            if (!startUI) {
                Debug.LogError(@"UISystem initialize fail , Cannot found start ui which has a <color=cyan>[UIStart]</color> Attribute and inherit <color=cyan>:WindowUI<T></color>");
#if UNITY_EDITOR
                Selection.activeGameObject = gameObject;
#endif
                Debug.DebugBreak();
                return;
            }
            foreach (Transform child in transform) {
                var comp = child.GetComponent<BaseUI>();
                if (comp && comp.Equals(startUI))
                    comp.Show();
                else
                    child.gameObject.SetActive(false);
            }
        }
    }
}