using System;
using System.Collections.Generic;

namespace Arthas.Network
{
    /// <summary>
    /// 消息接口
    /// </summary>
    public interface INetworkMessage
    {
        /// <summary>
        /// 命令
        /// </summary>
        object Command { get; }

        object Body { get; }

        /// <summary>
        /// 消息参数
        /// </summary>
        object[] Parameters { get; }

        /// <summary>
        /// 获取消息包含长度
        /// </summary>
        /// <returns></returns>
        byte[] GetBuffer(bool littleEndian = false);

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetValue<T>();
    }

    /// <summary>
    /// 消息包装接口
    /// </summary>
    public interface INetworkMessageHandler
    {
        /// <summary>
        /// 包装消息
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        INetworkMessage PackMessage(object command, object obj, params object[] parameters);

        /// <summary>
        /// 解析消息
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        INetworkMessage ParseMessage(byte[] buffer);
    }
}