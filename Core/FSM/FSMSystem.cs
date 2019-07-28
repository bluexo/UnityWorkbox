using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityWorkbox.FSM
{
    /// <summary>
    /// 状态机类
    /// </summary>
    public class FSMSystem<T>
    {
        private Dictionary<StateType, FSMState<T>> states { get; set; }

        public StateType CurrentStateType { get; protected set; }

        public FSMState<T> CurrentState { get; protected set; }

        public FSMSystem()
        {
            states = new Dictionary<StateType, FSMState<T>>();
        }

        /// <summary>
        /// 添加状态
        /// </summary>
        public void AddState(StateType type, FSMState<T> state)
        {
            if (state == null) {
                Debug.LogError("FSM ERROR: Null reference is not allowed");
            }
            if (states.Count == 0) {
                CurrentState = state;
                CurrentStateType = state.StateType;
            }
            if (states.ContainsKey(type)) {
                Debug.LogErrorFormat("FSM ERROR: Impossible to add state {0} because state has already been added",
                    type.ToString());
                return;
            }
            states.Add(type, state);
        }

        /// <summary>
        ///删除状态
        /// </summary>
        public void DeleteState(StateType type)
        {
            // Check for NullState before deleting
            if (type == StateType.NullState) {
                Debug.LogError("FSM ERROR: NullStateID is not allowed for a real state");
                return;
            }

            // Search the List and delete the state if it's inside it
            if (states.ContainsKey(type)) {
                states.Remove(type);
                return;
            }
            Debug.LogErrorFormat("FSM ERROR: Impossible to delete state {0} . It was not on the list of states",
                type.ToString());
        }

        /// <summary>
        /// 执行状态变换
        /// </summary>
        public void PerformTransition(StateTriggerType trigger)
        {
            // Check for NullTransition before changing the current state
            if (trigger == StateTriggerType.NullTransition) {
                Debug.LogError("FSM ERROR: NullTransition is not allowed for a real transition");
                return;
            }
            // Check if the currentState has the transition passed as argument
            var type = CurrentState.GetStateType(trigger);
            if (type == StateType.NullState) {
                Debug.LogErrorFormat("FSM ERROR: State {0} does not have a target state {1} for transition ",
                    CurrentStateType.ToString(),
                    trigger.ToString());
                return;
            }

#if UNITY_EDITOR
            Debug.LogFormat("<color=cyan> State translate {0} >>> {1} .</color>", CurrentStateType.ToString(), type.ToString());
#endif

            CurrentStateType = type;
            if (states.ContainsKey(CurrentStateType)) {
                var state = states[CurrentStateType];
                CurrentState.BeforeLeaving();
                CurrentState = state;
                CurrentState.BeforeEntering();
            }
        }

        public void Update<T1>(T t, T1 t1) where T1 : Component
        {
            CurrentState.Reason(t, t1);
            CurrentState.Act(t, t1);
        }
    }
}