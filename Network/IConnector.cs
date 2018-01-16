using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arthas.Network
{
    /// <summary>
    /// 连接器接口
    /// </summary>
    public interface IConnector
    {
        bool IsConnected { get; }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void Connect(string ip, int port);

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="buffer"></param>
        void Send(byte[] buffer);

        /// <summary>
        /// 消息接收事件
        /// </summary>
        event Action<byte[]> MessageRespondEvent;

        /// <summary>
        /// 断开事件
        /// </summary>
        event Action DisconnectEvent;

        /// <summary>
        /// 关闭
        /// </summary>
        void Close();
    }
}
