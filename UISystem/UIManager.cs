using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas.Client.UI
{
    public class UIManager : Singleton<UIManager>
    {
        private Dictionary<string, BaseUI> windows = new Dictionary<string, BaseUI>();
        private List<BaseUI> showedWindows = new List<BaseUI>();

        /// <summary>
        /// 主画布
        /// </summary>
        public static MainCanvasUI MainCanvasUI {
            get {
                if (!mainCanvas) {
                    mainCanvas = UnityEngine.Object.FindObjectOfType<MainCanvasUI>();
                }
                return mainCanvas;
            }
        }

        private static MainCanvasUI mainCanvas;

        /// <summary>
        /// 上一个显示的窗口
        /// </summary>
        public static BaseUI CurrentWindow { get; private set; }
        public static BaseUI PrevWindows { get; private set; }

        /// <summary>
        /// 添加UI
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ui"></param>
        public static void AddUI(string name, BaseUI ui)
        {
            if (!Instance.windows.ContainsKey(name)) {
                Instance.windows.Add(name, ui);
                ui.UIShowEvent += Instance.OnShow;
                ui.UIHideEvent += Instance.OnHide;
            }
        }

        internal static bool ContainsUI(string name)
        {
            return Instance.windows.ContainsKey(name);
        }

        /// <summary>
        /// 当UI显示
        /// </summary>
        /// <param name="name"></param>
        private void OnShow(object sender, UIEventArgs args)
        {
            var ui = sender as BaseUI;
            if (windows.ContainsKey(ui.name)) {
                var window = windows[ui.name];
                if (window.IsExclusive) {
                    var arr = showedWindows.ToArray();
                    foreach (var item in arr) {
                        if (!item.IsAlwaysShow)
                            item.Hide();
                        else
                            item.transform.SetAsLastSibling();
                    }
                    showedWindows.Clear();
                }
                showedWindows.Add(window);
                CurrentWindow = window;
            }
        }

        /// <summary>
        /// 当UI隐藏
        /// </summary>
        /// <param name="name"></param>
        private void OnHide(object sender, UIEventArgs args)
        {
            var ui = sender as BaseUI;
            if (windows.ContainsKey(ui.name)) {
                var window = windows[ui.name];
                showedWindows.Remove(window);
            }
        }
    }
}