using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Arthas.Client.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleTrigger : MonoBehaviour
    {
        public event Action<Toggle> ToggleTriggerEvent;
        private Toggle toggler;

        void Start()
        {
            toggler = GetComponent<Toggle>();
            toggler.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(bool on)
        {
            if (ToggleTriggerEvent != null)
                ToggleTriggerEvent(toggler);
        }
    }
}
