using System;
using System.Collections.Generic;
using UnityEngine;
namespace Arthas.Network
{
    public static class MessageDispatcher
    {
        private static Dictionary<object, List<Action<INetworkMessage>>> messages = new Dictionary<object, List<Action<INetworkMessage>>>();
        private static Dictionary<object, List<Action<INetworkMessage>>> onceMessages = new Dictionary<object, List<Action<INetworkMessage>>>();

        static MessageDispatcher()
        {
            Networker.ResponseEvent += Invoke;
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="invoker"></param>
        public static void RegisterMessage(object cmd, Action<INetworkMessage> invoker, bool allowMultiple = true)
        {
            if (!messages.ContainsKey(cmd))
            {
                var actions = new List<Action<INetworkMessage>>();
                messages.Add(cmd, actions);
            }
            if (!allowMultiple && messages[cmd].Count > 0) return;
            messages[cmd].Add(invoker);
        }

        /// <summary>
        /// 注册消息一次性处理消息
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="invoker"></param>
        public static void Once(object cmd, Action<INetworkMessage> invoker)
        {
            if (!onceMessages.ContainsKey(cmd))
            {
                var actions = new List<Action<INetworkMessage>>();
                onceMessages.Add(cmd, actions);
            }
            onceMessages[cmd].Add(invoker);
        }

        /// <summary>
        /// 注销消息
        /// </summary>
        /// <param name="cmd"></param>
        public static void UnregisterMessage(object cmd, Action<INetworkMessage> invoker)
        {
            if (messages.ContainsKey(cmd))
            {
                var msgs = messages[cmd];
                msgs.Remove(invoker);
            }
        }

        /// <summary>
        /// 发起
        /// </summary>
        /// <param name="msg"></param>
        private static void Invoke(INetworkMessage msg)
        {
            if (onceMessages.ContainsKey(msg.Command))
            {
                var msgs = onceMessages[msg.Command];
                for (var i = 0; i < msgs.Count; i++)
                {
                    msgs[i].Invoke(msg);
                }
                msgs.Clear();
                return;
            }

            if (messages.ContainsKey(msg.Command))
            {
                var msgs = messages[msg.Command];
                for (var i = 0; i < msgs.Count; i++) msgs[i].Invoke(msg);
            }
#if UNITY_EDITOR
            else Debug.LogFormat("Cannot invoke message,CmdType:{0}", msg.Command);
#endif
        }

        public static void Clear()
        {
            foreach (var actions in messages.Values) actions.Clear();
            messages.Clear();
            foreach (var actions in onceMessages.Values) actions.Clear();
            onceMessages.Clear();
        }
    }
}
