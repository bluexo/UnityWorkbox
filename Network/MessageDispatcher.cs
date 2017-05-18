using System;
using System.Collections.Generic;
using Arthas.Protocol;
using UnityEngine;
namespace Arthas.Network
{
    public static class MessageDispatcher
    {
        private static Dictionary<object, Action<INetworkMessage>> messages = new Dictionary<object, Action<INetworkMessage>>();

        static MessageDispatcher()
        {
            TCPNetwork.PushEvent += Invoke;
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="invoker"></param>
        public static void RegisterMessage(object cmd, Action<INetworkMessage> invoker)
        {
            if (!messages.ContainsKey(cmd)) {
                messages.Add(cmd, invoker);
            }
        }

        /// <summary>
        /// 注销消息
        /// </summary>
        /// <param name="cmd"></param>
        public static void UnregisterMessage(object cmd)
        {
            if (messages.ContainsKey(cmd)) {
                messages.Remove(cmd);
            }
        }

        /// <summary>
        /// 发起
        /// </summary>
        /// <param name="msg"></param>
        public static void Invoke(INetworkMessage msg)
        {
            if (messages.ContainsKey(msg.Command))
                messages[msg.Command].Invoke(msg);
            else
                Debug.LogFormat("Cannot invoke message,CmdType:{0}", msg.Command);
        }

        public static void Once(INetworkMessage msg)
        {
            if (messages.ContainsKey(msg.Command)) {
                messages[msg.Command].Invoke(msg);
                messages.Remove(msg.Command);
            } else
                Debug.LogFormat("Cannot invoke message,CmdType:{0}", msg.Command);
        }
    }
}
