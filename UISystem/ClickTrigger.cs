using System;
using UnityEngine;
using UnityEngine.UI;

namespace Arthas.Client.UI
{

    [RequireComponent(typeof(Button))]
    public class ClickTrigger : MonoBehaviour
    {
        public event Action<Button> ClickedEvent;

        private Button toggler;

        void Start()
        {
            toggler = GetComponent<Button>();
            toggler.onClick.AddListener(OnValueChanged);
        }

        private void OnValueChanged()
        {
            if (ClickedEvent != null)
                ClickedEvent(toggler);
        }
    }
}
