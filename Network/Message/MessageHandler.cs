using System;
using System.Collections.Generic;
using Arthas.Protocol;
using UnityEngine;
namespace Arthas.Network
{
    public class MessageHandler
    {
        private static Dictionary<short, Action<IMessage>> messages = new Dictionary<short, Action<IMessage>>();

        static MessageHandler()
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
            var cmdType = msg.Head.CommandId;
            if (messages.ContainsKey(cmdType))
                messages[cmdType].Invoke(msg);
            else
                Debug.LogFormat("Cannot invoke message,MsgID:{0},CmdType:{1}", msg.Head, cmdType);
        }
    }
}
