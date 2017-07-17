﻿using System;
using System.Collections.Generic;
using UnityEngine;
namespace Arthas.Network
{
    public static class MessageDispatcher
    {
        private static Dictionary<object, Action<INetworkMessage>> messages = new Dictionary<object, Action<INetworkMessage>>();
        private static Dictionary<object, Action<INetworkMessage>> onceMessages = new Dictionary<object, Action<INetworkMessage>>();

        static MessageDispatcher()
        {
            Networker.PushEvent += Invoke;
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
        /// 注册消息
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="invoker"></param>
        public static void Once(object cmd, Action<INetworkMessage> invoker)
        {
            if (!onceMessages.ContainsKey(cmd)) {
                onceMessages.Add(cmd, invoker);
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
        private static void Invoke(INetworkMessage msg)
        {
            if (onceMessages.ContainsKey(msg.Command)) {
                onceMessages[msg.Command].Invoke(msg);
                return;
            }
            if (messages.ContainsKey(msg.Command)) messages[msg.Command].Invoke(msg);
            else Debug.LogFormat("Cannot invoke message,CmdType:{0}", msg.Command);
        }
    }
}
