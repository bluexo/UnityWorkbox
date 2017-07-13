using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Arthas.UI
{
    [UIHeader]
    [UIOrder(SortOrder = 100)]
    public class TipsUI : PanelUI<TipsUI>
    {
        public static readonly Color okColor = Color.green, infoColor = Color.black, alertColor = Color.yellow, errorColor = Color.red;
        public Queue<KeyValuePair<string, Color>> queue = new Queue<KeyValuePair<string, Color>>();
        private WaitForSeconds waitForInterval, waitForHide;
        private Coroutine coroutine;

        [SerializeField]
        private Text text;
        [SerializeField]
        private float intervalDur = 0.6f, hideDur = 6f;

        protected override void Awake()
        {
            base.Awake();
            waitForHide = new WaitForSeconds(hideDur);
            waitForInterval = new WaitForSeconds(intervalDur);
        }

        public static void Info(string content)
        {
            Instance.Pop(infoColor, content);
        }

        public static void Alert(string content)
        {
            Instance.Pop(alertColor, content);
        }

        public static void Ok(string content)
        {
            Instance.Pop(okColor, content);
        }

        public static void Error(string content)
        {
            Instance.Pop(errorColor, content, 5f);
        }

        private void Pop(Color color, string content, float delay = 3f)
        {
            if (!IsActive()) Show();
            var img = GetComponent<Image>();
            var pair = new KeyValuePair<string, Color>(content, color - new Color(0, 0, 0, 0.3f));
            queue.Enqueue(pair);
            if (coroutine == null) coroutine = StartCoroutine(Pop());
        }

        private IEnumerator Pop()
        {
            transform.SetAsLastSibling();
            var pair = queue.Dequeue();
            var img = GetComponent<Image>();
            img.color = pair.Value;
            text.text = pair.Key;
            Slide();
            yield return waitForHide;
            Slide(false);
            yield return intervalDur;
            if (queue.Count <= 0) {
                StopCoroutine(coroutine);
                coroutine = null;
            } else {
                StartCoroutine(Pop());
            }
        }

        public virtual void Slide(bool show = true)
        {
            var height = RectTransform.sizeDelta.y;
            var end = show ? -height / 2 + 3f : height / 2;
            RectTransform.position = new Vector3(RectTransform.position.x, end, RectTransform.position.z);
        }
    }
}