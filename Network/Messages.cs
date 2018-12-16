using System;
using System.Collections.Generic;

namespace Arthas.Network
{
    /// <summary>
    /// 消息接口 封装消息的格式和获取方式
    /// </summary>
    public interface INetworkMessage
    {
        short ResponseCode { get; }

        /// <summary>
        /// 命令
        /// </summary>
        object Command { get; }

        /// <summary>
        /// 消息体
        /// </summary>
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
    /// 消息包装接口 封装消息的处理方式
    /// </summary>
    public interface INetworkMessageHandler
    {
        /// <summary>
        /// 全局响应码处理程序
        /// </summary>
        IDictionary<short, Action<INetworkMessage>> GlobalResponseCodeHandlers { get; }

        /// <summary>
        /// 包装消息
        /// </summary>
        /// <param name="command">消息命令</param>
        /// <param name="obj">消息体</param>
        /// <param name="parameters">消息参数</param>
        /// <returns></returns>
        INetworkMessage PackMessage(object command, object obj, params object[] parameters);

        /// <summary>
        /// 解析消息
        /// </summary>
        /// <param name="buffer">需要解析的数据</param>
        /// <returns></returns>
        IList<INetworkMessage> ParseMessage(byte[] buffer);

        /// <summary>
        /// 解析消息
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        IList<INetworkMessage> ParseMessage(ByteBuffer buffer);
    }
}