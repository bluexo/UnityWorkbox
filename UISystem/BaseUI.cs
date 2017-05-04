using System;
using System.Collections.Generic;
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
		[SerializeField] private int sortOrder;
		
		public virtual int SortOrder { get { return sortOrder; } }

		public event Action<BaseUI> UIShowEvent;

        public event Action<BaseUI> UIHideEvent;

        #region show or hide event when ui active or deactive!
        [SerializeField, Header("Trigger when show")]
        protected UnityEvent BeforeShow, AfterShow;

        [SerializeField, Header("Trigger when hide")]
        public UnityEvent BeforeHide, AfterHide;

        [SerializeField, Header("Share screen with brothers")]
        protected List<BaseUI> brotherWindows = new List<BaseUI>();

        public IList<BaseUI> BrotherWindows { get { return brotherWindows; } }

        #endregion show or hide event when ui active or deactive!

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
            get
            {
                if (!instance)
                {
                    var uiName = typeof(T).Name;
                    var child = UIManager.Instance.transform.FindChild(uiName);
                    if (child)
                    {
                        var ui = child.GetComponent<T>();
                        if (ui)
                            instance = ui;
                        else
                            instance = child.gameObject.AddComponent<T>();
                    }
                    else Debug.LogErrorFormat("Can not found ui gameobject : {0} in UICanvas!", uiName);
                }
                return instance;
            }
        }
   
        private static T instance;

        public virtual void Show()
        {
            UIManager.AddUI(this);
            base.Show();
        }

        public virtual void Back()
        {
            if (UIManager.PrevWindow.UI)
            {
                UIManager.PrevWindow.UI.Show();
            }
        }
    }
}