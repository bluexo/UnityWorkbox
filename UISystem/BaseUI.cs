using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if WINDOWS_UWP
using System.Reflection;
#endif

namespace Arthas.Client.UI
{
    public class UIEventArgs : EventArgs { }

    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    public abstract class BaseUI : UIBehaviour
    {
        public event EventHandler<UIEventArgs> UIShowEvent;
        public event EventHandler<UIEventArgs> UIHideEvent;

        #region beforeShow or afterHide event when ui active or deactive!
        [Header("Do something before UI show")]
        public UnityEvent BeforeShow;
        [Header("Do something after UI hide")]
        public UnityEvent AfterHide;
        #endregion

        public UIEventArgs args = new UIEventArgs();
        public RectTransform RectTransform { get { return transform as RectTransform; } }

        public virtual void Hide()
        {
            if (UIHideEvent != null) {
                UIHideEvent(this, args);
            }
            gameObject.SetActive(false);
            AfterHide.Invoke();
        }

        public virtual void Show()
        {
            BeforeShow.Invoke();
            if (UIShowEvent != null) {
                UIShowEvent(this, args);
            }
            gameObject.SetActive(true);
        }

        public virtual bool IsExclusive { get { return true; } }

        public virtual bool IsAlwaysShow { get { return true; } }
    }

    public abstract class WindowUI<T> : BaseUI where T : BaseUI
    {
        public static T Instance {
            get {
                if (!instance) {
                    var uiName = typeof(T).Name;
                    var go = UIManager.MainCanvasUI.transform.FindChild(uiName).gameObject;
                    if (go) {
                        var ui = go.GetComponent<T>();
                        if (ui)
                            instance = ui;
                        else
                            instance = go.AddComponent<T>();
                    }
                    else Debug.LogFormat(RichText.Red("Can not found ui gameobject : {0} in MainCanvas!", uiName));
                }
                return instance;
            }
        }

        private static T instance;

        public override void Show()
        {
            if (!UIManager.ContainsUI(name))
                UIManager.AddUI(name, this);
            base.Show();
        }

        public override bool IsExclusive {
            get {
#if WINDOWS_UWP
                var head = typeof(T).GetTypeInfo().GetCustomAttribute<UIHeaderAttribute>();
                if (head != null)
                {
                    return head.Exclusive;
                }
#else
                var heads = typeof(T).GetCustomAttributes(typeof(UIHeaderAttribute), false);
                if (heads.Length > 0) {
                    var head = (UIHeaderAttribute)heads[0];
                    return head.Exclusive;
                }
#endif
                return false;
            }
        }

        public override bool IsAlwaysShow {
            get {
#if WINDOWS_UWP
                var head = typeof(T).GetTypeInfo().GetCustomAttribute<UIHeaderAttribute>();
                if (head != null)
                {
                    return head.AlwaysShow;
                }
#else
                var heads = typeof(T).GetCustomAttributes(typeof(UIHeaderAttribute), false);
                if (heads.Length > 0) {
                    var head = (UIHeaderAttribute)heads[0];
                    return head.AlwaysShow;
                }
#endif
                return false;
            }
        }
    }
}