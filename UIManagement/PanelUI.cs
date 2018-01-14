using UnityEngine;
using System.Collections;

namespace Arthas.UI
{
    [DisallowMultipleComponent]
    public abstract class PanelUI<T> : BaseUI where T : BaseUI
    {
        public static T Instance
        {
            get
            {
                if (!instance) {
                    var uiName = typeof(T).Name;                                    
                    var child = UIManager.Instance.transform.Find(uiName);
                    if (child) {
                        var ui = child.GetComponent<T>();
                        if (ui) instance = ui;
                        else instance = child.gameObject.AddComponent<T>();
                    } else Debug.LogErrorFormat("Can not found ui gameobject : {0} in UICanvas!", uiName);
                }
                return instance;
            }
        }

        protected static T instance;

        public override void Show()
        {
            UIManager.AddUI(this);
            base.Show();
        }

        public virtual void Back()
        {
            if (UIManager.PrevWindow.UI) {
                UIManager.PrevWindow.UI.Show();
            }
        }

        protected override void OnDestroy()
        {
            instance = null;
        }
    }
}
