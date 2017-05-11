using System;
using System.Collections.Generic;
using Arthas.Protocol;
using UnityEngine;
namespace Arthas.Network
{
    public static class MessageDispatcher
    {
        private static Dictionary<int, Action<IMessage>> messages = new Dictionary<int, Action<IMessage>>();

        static MessageDispatcher()
        {
            TCPNetwork.PushEvent += Invoke;
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="invoker"></param>
        public static void RegisterMessage(short cmdType, Action<IMessage> invoker)
        {
            if (!messages.ContainsKey(cmdType))
            {
                messages.Add(cmdType, invoker);
            }
        }

        /// <summary>
        /// 注销消息
        /// </summary>
        /// <param name="command"></param>
        public static void UnregisterMessage(short command)
        {
            if (messages.ContainsKey(command))
            {
                messages.Remove(command);
            }
        }

        /// <summary>
        /// 发起
        /// </summary>
        /// <param name="msg"></param>
        public static void Invoke(IMessage msg)
        {
            var cmd = (short)msg.Parameters[0];
            if (messages.ContainsKey(cmd))
                messages[cmd].Invoke(msg);
            else
                Debug.LogFormat("Cannot invoke message,CmdType:{0}", cmd);
        }
    }
}
