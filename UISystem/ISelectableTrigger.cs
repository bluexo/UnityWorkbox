using System;
using UnityEngine;

namespace Arthas.UI
{
    public delegate void UITriggerDelegate(GameObject go);

    /// <summary>
    /// UI交互触发接口
    /// </summary>
    public interface ISelectableUITrigger
    {
        event UITriggerDelegate TriggerEvent;
    }
}
