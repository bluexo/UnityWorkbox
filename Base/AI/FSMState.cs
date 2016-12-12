using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 状态触发类型
/// </summary>
public enum StateTriggerType
{
    NullTransition = 0,     // Use this transition to represent a non-existing transition in your system
    Idle = 1,
    Fight = 2,
}

/// <summary>
/// 状态类型
/// </summary>
public enum StateType
{
    NullState = 0,
    Idle = 1,            //待机
    Fight = 2,          //攻击
}

/// <summary>
/// 状态基类
/// </summary>
public abstract class FSMState<T>
{
    protected Dictionary<StateTriggerType, StateType> map = new Dictionary<StateTriggerType, StateType>();

    public StateType StateType { get; protected set; }

    public void AddTransition(StateTriggerType trans, StateType id)
    {
        // Check if anyone of the args is invalid
        if (trans == StateTriggerType.NullTransition)
        {
            Debug.LogError("FSMState ERROR: NullTransition is not allowed for a real transition");
            return;
        }

        if (id == StateType.NullState)
        {
            Debug.LogError("FSMState ERROR: NullStateID is not allowed for a real ID");
            return;
        }

        //   check if the current transition was already inside the map
        if (map.ContainsKey(trans))
        {
            Debug.LogErrorFormat("FSMState ERROR: State {0} already has transition {1} Impossible to assign to another state",
                StateType.ToString(),
                trans.ToString());
            return;
        }
        map.Add(trans, id);
    }

    public void DeleteTransition(StateTriggerType trans)
    {
        // Check for NullTransition
        if (trans == StateTriggerType.NullTransition)
        {
            Debug.LogError("FSMState ERROR: NullTransition is not allowed");
            return;
        }

        // Check if the pair is inside the map before deleting
        if (map.ContainsKey(trans))
        {
            map.Remove(trans);
            return;
        }
        Debug.LogErrorFormat("FSMState ERROR: Transition {0} passed to {1} was not on the state's transition list",
            trans.ToString(),
            StateType.ToString());
    }

    public StateType GetStateType(StateTriggerType trans)
    {
        if (map.ContainsKey(trans))
        {
            return map[trans];
        }
        return StateType.NullState;
    }

    public virtual void BeforeEntering() { }

    public virtual void BeforeLeaving() { }

    public abstract void Reason<T1>(T owner, T1 param1)  where T1 : Component;
    public abstract void Act<T1>(T owner, T1 param1) where T1 : Component;
}