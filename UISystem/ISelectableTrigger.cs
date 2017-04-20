using System;
using UnityEngine;

/// <summary>
/// UI交互触发接口
/// </summary>
public interface ISelectableUITrigger
{
    event Action<GameObject> TriggerEvent;
}
