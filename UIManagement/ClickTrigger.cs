using System;
using UnityEngine;
using UnityEngine.UI;

namespace Arthas.UI
{
    [RequireComponent(typeof(Button))]
    public class ClickTrigger : MonoBehaviour, ISelectableUITrigger
    {
        public event UITriggerDelegate TriggerEvent;

        private Button button;

        void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnValueChanged);
        }

        private void OnValueChanged()
        {
            if (TriggerEvent != null) TriggerEvent(button.gameObject);
        }
    }
}
