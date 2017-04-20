using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Arthas.Client.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleTrigger : MonoBehaviour , ISelectableUITrigger
    {
        public event Action<GameObject> TriggerEvent;
       
        private Toggle toggler;

        void Start()
        {
            toggler = GetComponent<Toggle>();
            toggler.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(bool on)
        {
            if (TriggerEvent != null)
                TriggerEvent(toggler.gameObject);
        }
    }
}
