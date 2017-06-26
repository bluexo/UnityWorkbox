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
        public static readonly Color okColor = Color.green;
        public static readonly Color infoColor = Color.black;
        public static readonly Color alertColor = Color.yellow;
        public static readonly Color errorColor = Color.red;
        public Queue<KeyValuePair<string, Color>> queue = new Queue<KeyValuePair<string, Color>>();
        private WaitForSeconds interval = new WaitForSeconds(.6f);
        private WaitForSeconds waitForHide = new WaitForSeconds(3f);
        private Coroutine coroutine;

        [SerializeField]
        private Text text;

        protected override void Awake()
        {
            text = transform.GetComponentFromChild<Text>("Text");
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
            Debug.Log(content);
            if (!IsActive())
                Show();
            var img = GetComponent<Image>();
            var col = color - new Color(0, 0, 0, 0.3f);
            var pair = new KeyValuePair<string, Color>(content, col);
            queue.Enqueue(pair);
            if (coroutine == null)
                coroutine = StartCoroutine(Pop());
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
            yield return interval;
            if (queue.Count <= 0) {
                StopCoroutine(coroutine);
                coroutine = null;
            } else {
                StartCoroutine(Pop());
            }
        }

        public void Slide(bool show = true)
        {
            var height = RectTransform.sizeDelta.y;
            var end = show ? -height / 2 + 3f : height / 2;
            RectTransform.position = new Vector3(RectTransform.position.x, end, RectTransform.position.z);
        }
    }
}