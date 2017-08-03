using Arthas.Common;
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

        [SerializeField, Rename("Floating [?]"), Tooltip("[Floating] ui always on top of others")]
        protected bool floating;
        public virtual bool Floating { get { return floating; } }

        [SerializeField, Rename("Exclusive [?]"), Tooltip("Other ui will be auto hide , when [Exclusive] ui show")]
        protected bool exclusive;
        public virtual bool IsExclusive { get { return exclusive; } }

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

        public virtual IEnumerator ShowAsync(float delay = 0)
        {
            this.Invoke(Show, delay);
            yield return new WaitUntil(() => isActiveAndEnabled);
        }

        public virtual IEnumerator HideAsync(float delay = 0)
        {
            this.Invoke(Hide, delay);
            yield return new WaitUntil(() => !isActiveAndEnabled);
        }
    }
}