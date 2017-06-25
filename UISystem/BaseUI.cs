using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if WINDOWS_UWP
using System.Reflection;
#endif

namespace Arthas.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    public abstract class BaseUI : UIBehaviour
    {
        [SerializeField]
        [Range(1, 100)]
        protected int sortOrder;
        public virtual int SortOrder { get { return sortOrder; } }

        [SerializeField]
        protected bool header;
        public virtual bool IsHeader { get { return header; } }

        [SerializeField]
        protected bool exlusive;
        public virtual bool IsExlusive { get { return exlusive; } }


        public event Action<BaseUI> UIShowEvent;

        public event Action<BaseUI> UIHideEvent;

        #region show or hide event when ui active or deactive!
        [SerializeField, HideInInspector]
        protected UnityEvent BeforeShow, AfterShow;

        [SerializeField, HideInInspector]
        public UnityEvent BeforeHide, AfterHide;

        [SerializeField, HideInInspector]
        protected List<BaseUI> group = new List<BaseUI>();
        public IList<BaseUI> UIGroup { get { return group; } }

        #endregion show or hide event when ui active or deactive!

        public RectTransform RectTransform { get { return transform as RectTransform; } }

        public virtual void Hide()
        {
            if (BeforeHide != null) BeforeHide.Invoke();
            gameObject.SetActive(false);
            if (UIHideEvent != null) UIHideEvent(this);
            if (AfterHide != null) AfterHide.Invoke();
        }

        public virtual void Show()
        {
            if (BeforeShow != null) BeforeShow.Invoke();
            if (UIShowEvent != null) UIShowEvent(this);
            gameObject.SetActive(true);
            if (AfterShow != null) AfterShow.Invoke();
        }

        public virtual IEnumerator ShowAsync()
        {
            Show();
            yield return new WaitForEndOfFrame();
        }

        public virtual IEnumerator HideAsync()
        {
            Hide();
            yield return new WaitForEndOfFrame();
        }
    }

    [DisallowMultipleComponent]
    public abstract class WindowUI<T> : BaseUI where T : BaseUI
    {
        public static T Instance
        {
            get
            {
                if (!instance) {
                    var uiName = typeof(T).Name;
                    var child = UIManager.Instance.transform.FindChild(uiName);
                    if (child) {
                        var ui = child.GetComponent<T>();
                        if (ui) instance = ui;
                        else instance = child.gameObject.AddComponent<T>();
                    } else Debug.LogErrorFormat("Can not found ui gameobject : {0} in UICanvas!", uiName);
                }
                return instance;
            }
        }

        private static T instance;

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
    }
}