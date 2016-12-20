using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if WINDOWS_UWP
using System.Reflection;
#endif

namespace Arthas.Client.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    public abstract class BaseUI : UIBehaviour
    {
        public event Action<BaseUI> UIShowEvent;

        public event Action<BaseUI> UIHideEvent;

        #region beforeShow or afterHide event when ui active or deactive!

        [Header("Trigger when show[显示时触发的事件]")]
        public UnityEvent BeforeShow;

        public UnityEvent AfterShow;

        [Header("Trigger when hide[隐藏时触发的事件]")]
        public UnityEvent BeforeHide;

        public UnityEvent AfterHide;

        #endregion beforeShow or afterHide event when ui active or deactive!

        public RectTransform RectTransform { get { return transform as RectTransform; } }

        public virtual void Hide()
        {
            if (BeforeHide != null)
                BeforeHide.Invoke();
            gameObject.SetActive(false);
            if (UIHideEvent != null)
                UIHideEvent(this);
            if (AfterHide != null)
                AfterHide.Invoke();
        }

        public virtual void Show()
        {
            if (BeforeShow != null)
                BeforeShow.Invoke();
            if (UIShowEvent != null)
                UIShowEvent(this);
            gameObject.SetActive(true);
            if (AfterShow != null)
                AfterShow.Invoke();
        }
    }

    public abstract class WindowUI<T> : BaseUI where T : BaseUI
    {
        public static T Instance
        {
            get {
                if (!instance) {
                    var uiName = typeof(T).Name;
                    var go = UICanvas.Instance.transform.FindChild(uiName).gameObject;
                    if (go) {
                        var ui = go.GetComponent<T>();
                        if (ui)
                            instance = ui;
                        else
                            instance = go.AddComponent<T>();
                    } else Debug.LogFormat(RichText.Red("Can not found ui gameobject : {0} in MainCanvas!", uiName));
                }
                return instance;
            }
        }

        private static T instance;

        public override void Show()
        {
            if (!UIManager.ContainsUI(name)) {
                UIManager.AddUI(name, this);
            }
            base.Show();
        }

        protected virtual void Back()
        {
            if (UIManager.PrevWindow) {
                UIManager.PrevWindow.Show();
            }
        }
    }
}