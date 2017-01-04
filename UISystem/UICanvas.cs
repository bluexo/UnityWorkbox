using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Arthas.Client.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UICanvas : SingletonBehaviour<UICanvas>
    {
        [SerializeField]
        private BaseUI startUI;

        protected void Start()
        {
            if (!startUI) {
#if UNITY_EDITOR
                Selection.activeGameObject = gameObject;
#endif
                Debug.LogError(@"UISystem initialize fail , Cannot found start ui which has a <color=cyan>[UIStart]</color> Attribute and inherit <color=cyan>:WindowUI<T></color>");
                Debug.DebugBreak();
                return;
            }
            startUI.Show();
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                var comp = child.GetComponent<BaseUI>();
                if (!comp) {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}