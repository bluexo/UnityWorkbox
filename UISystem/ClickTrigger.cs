using System;
using UnityEngine;
using UnityEngine.UI;

namespace Arthas.UI
{
    [RequireComponent(typeof(Button))]
    public class ClickTrigger : MonoBehaviour, ISelectableUITrigger
    {
        public event UITriggerDelegate TriggerEvent;

        private Button toggler;

        void Start()
        {
            toggler = GetComponent<Button>();
            toggler.onClick.AddListener(OnValueChanged);
        }

        private void OnValueChanged()
        {
            if (TriggerEvent != null) TriggerEvent(toggler.gameObject);
        }
    }
}
