using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas.Network
{
    /// <summary>
    /// 远程调用
    /// </summary>
    public class RPCAttribute : Attribute
    {
        public object Command { get; set; }
    }

    /// <summary>
    /// 网络消息处理器
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface INetworkHandler<out TCommand>
    {
        /// <summary>
        /// 获取消息命令
        /// </summary>
        /// <returns></returns>
        TCommand Command { get; }

        /// <summary>
        /// 当消息被接收
        /// </summary>
        /// <param name="message"></param>
        void OnMessageReceive(INetworkMessage message);

        /// <summary>
        /// 当对象被释放
        /// </summary>
        event EventHandler DisposedEvent;
    }
}
