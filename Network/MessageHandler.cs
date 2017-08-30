using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Arthas.Network
{
    /// <summary>
    /// 默认消息
    /// </summary>
    public class DefaultMessage : INetworkMessage
    {
        protected byte[] buffer = { };

        public bool WithLength { get; private set; }

        public object[] Parameters { get; private set; }

        public object Body { get { return buffer; } }

        public object Command { get; private set; }

        /// <summary>
        /// 默认消息构造
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="bodyBuffer">消息体</param>
        /// <param name="withLength">消息体是否包含长度</param>
        /// <param name="parameters">消息其他参数</param>
        public DefaultMessage(object command, byte[] bodyBuffer, bool withLength = true, params object[] parameters)
        {
            Command = command;
            buffer = bodyBuffer;
            WithLength = withLength;
            Parameters = parameters;
        }

        public virtual byte[] GetBuffer(bool littleEndian = false)
        {
            if (WithLength) return buffer;
            var lenBytes = littleEndian && BitConverter.IsLittleEndian
                ? BitConverter.GetBytes((short)buffer.Length)
                : BitConverter.GetBytes((short)buffer.Length).Reverse();
            var newBuffer = new byte[buffer.Length + lenBytes.Length];
            Buffer.BlockCopy(lenBytes, 0, newBuffer, 0, lenBytes.Length);
            Buffer.BlockCopy(buffer, 0, newBuffer, lenBytes.Length, buffer.Length);
            return newBuffer;
        }

        public virtual T GetValue<T>()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 默认的消息包装器
    /// </summary>
    public class DefaultMessageHandler : INetworkMessageHandler
    {
        public virtual INetworkMessage PackMessage(object command, object obj, params object[] parameters)
        {
            var bodyBuffer = obj as byte[];
            if (obj != null && bodyBuffer == null)
            {
                var msg = $"<color=cyan>{GetType()}</color> cannot support <color=cyan>{obj.GetType()}</color> type message , \n please implement your custom message!";
                throw new NotImplementedException(msg);
            }
            using (var stream = new MemoryStream())
            {
                var cmdBytes = BitConverter.GetBytes(Convert.ToInt16(command));
                stream.Write(cmdBytes, 0, cmdBytes.Length);
                if (bodyBuffer != null) stream.Write(bodyBuffer, 0, bodyBuffer.Length);
                return new DefaultMessage(command, stream.ToArray(), false, parameters);
            }
        }
        public virtual IList<INetworkMessage> ParseMessage(byte[] buffer)
        {
            var messages = new List<INetworkMessage>();
            using (var stream = new MemoryStream(buffer))
            using (var reader = new BinaryReader(stream))
            {
                while (reader.BaseStream.Position < buffer.Length)
                {
                    var len = reader.ReadInt16();
                    var cmd = reader.ReadInt16();
                    var content = reader.ReadBytes(len - sizeof(short));
                    var msg = new DefaultMessage(cmd, content);
                    messages.Add(msg);
                }
                return messages;
            }
        }
    }
}
