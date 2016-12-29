using System;
using System.Collections.Generic;

#if WINDOWS_UWP
using System.Reflection;
#endif

namespace Arthas.Client.UI
{
    public struct WindowInfo : IComparable<WindowInfo>
    {
        private const int headerOrderBegin = 1000;

        public byte Order { get; set; }
        public bool IsHeader { get; set; }
        public bool IsExclusive { get; set; }
        public BaseUI UI { get; set; }

        public void SetOrder(int headerCount, int amountCount, int index)
        {
            var i = IsHeader ? (amountCount - index - 1) : (amountCount - headerCount - index - 1);
            UI.transform.SetSiblingIndex(i);
            UI.gameObject.SetActive(true);
        }

        public bool ContainBrother(BaseUI ui)
        {
            return UI && UI.BrotherWindows.Contains(ui);
        }

        public int CompareTo(WindowInfo other)
        {
            return other.Order.CompareTo(Order);
        }
    }

    public static class UIManager
    {
        private static readonly Dictionary<BaseUI, WindowInfo> windows = new Dictionary<BaseUI, WindowInfo>();
        private static readonly List<WindowInfo> showedWindows = new List<WindowInfo>();
        private static readonly List<WindowInfo> showedHeaderWindows = new List<WindowInfo>();

        static UIManager()
        {
            var uis = UICanvas.Instance.GetComponentsInChildren<BaseUI>(true);
            for (var i = 0; i < uis.Length; i++) {
                AddUI(uis[i]);
                if (uis[i].isActiveAndEnabled) {
                    var window = CreateWindowInfo(uis[i]);
                    var showed = window.IsHeader ? showedHeaderWindows : showedWindows;
                    showed.Add(window);
                }
            }
        }

        /// <summary>
        /// 上一个显示的窗口
        /// </summary>
        public static WindowInfo CurrentWindow { get; private set; }

        public static WindowInfo PrevWindow { get; private set; }

        /// <summary>
        /// 添加UI
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ui"></param>
        public static void AddUI(BaseUI ui)
        {
            if (!windows.ContainsKey(ui)) {
                var window = CreateWindowInfo(ui);
                windows.Add(ui, window);
                ui.UIShowEvent += OnShow;
                ui.UIHideEvent += OnHide;
            }
        }

        private static WindowInfo CreateWindowInfo(BaseUI ui)
        {
            var uiType = ui.GetType();
#if WINDOWS_UWP
                var isHeader = uiType.GetTypeInfo().IsDefined(typeof(UIHeaderAttribute));
                var exclusive = uiType.GetTypeInfo().IsDefined(typeof(UIExclusiveAttribute), false);
                var order = uiType.GetTypeInfo().GetCustomAttributes(typeof(UIOrderAttribute), false);
#else
            var header = uiType.IsDefined(typeof(UIHeaderAttribute), false);
            var exclusive = uiType.IsDefined(typeof(UIExclusiveAttribute), false);
            var order = uiType.GetCustomAttributes(typeof(UIOrderAttribute), false);
#endif
            var window = new WindowInfo()
            {
                IsHeader = header,
                IsExclusive = exclusive,
                Order = order.Length > 0 ? ((UIOrderAttribute)order[0]).SortOrder : (byte)0,
                UI = ui
            };
            return window;
        }

        /// <summary>
        /// 当UI显示
        /// </summary>
        /// <param name="name"></param>
        private static void OnShow(BaseUI ui)
        {
            if (windows.ContainsKey(ui)) {
                var window = windows[ui];
                var windowList = window.IsHeader ? showedHeaderWindows : showedWindows;
                if (window.IsExclusive) {
                    var array = windowList.ToArray();
                    foreach (var item in array) {
                        if (window.ContainBrother(item.UI) || item.UI.Equals(ui)) continue;
                        item.UI.Hide();
                    }
                }
                PrevWindow = CurrentWindow;
                CurrentWindow = window;
                var sortWindows = new List<WindowInfo>(new WindowInfo[] { CurrentWindow });
                var brothers = CurrentWindow.UI.BrotherWindows;
                for (var i = 0; i < brothers.Count; i++) {
                    if (windows.ContainsKey(brothers[i])) {
                        var brotherWindow = windows[brothers[i]];
                        sortWindows.Add(brotherWindow);
                    }
                }
                sortWindows.Sort();
                for (var i = 0; i < sortWindows.Count; i++) {
                    var sortWindow = sortWindows[i];
                    if (!windowList.Contains(sortWindow))
                        windowList.Add(sortWindow);
                    sortWindow.SetOrder(showedHeaderWindows.Count, UICanvas.Instance.transform.childCount, i);
                }
            }
        }

        /// <summary>
        /// 当UI隐藏
        /// </summary>
        /// <param name="name"></param>
        private static void OnHide(BaseUI ui)
        {
            if (windows.ContainsKey(ui)) {
                var window = windows[ui];
                if (window.IsHeader)
                    showedWindows.Remove(window);
                else
                    showedHeaderWindows.Remove(window);
            }
        }
    }
}