using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Arthas.Network
{
    /// <summary>
    /// 消息头接口
    /// </summary>
    public interface IMessageHeader
    {
        /// <summary>
        /// 是否为小字节序
        /// </summary>
        bool IsLittleEndian { get; }

        /// <summary>
        /// 序号
        /// </summary>
        int SerialNumber { get; set; }

        /// <summary>
        /// 命令Id
        /// </summary>
        short Command { get; set; }

        /// <summary>
        /// 描述符
        /// </summary>
        short Descriptor { get; set; }

        /// <summary>
        /// 获取消息头的字节
        /// </summary>
        /// <returns></returns>
        byte[] GetBuffer();

        /// <summary>
        /// 获取Head
        /// </summary>
        /// <returns></returns>
        IMessageHeader CreateFromBuffer(byte[] buffer);

        /// <summary>
        /// 排除消息头
        /// </summary>
        /// <returns></returns>
        void ExceptHeader(ref byte[] buffer);
    }

    /// <summary>
    /// 消息接口
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// 消息头
        /// </summary>
        IMessageHeader Header { get; }

        /// <summary>
        /// 获取消息体
        /// </summary>
        /// <returns></returns>
        byte[] GetBuffer(bool containsHead = false);

        /// <summary>
        /// 获取消息包含长度
        /// </summary>
        /// <returns></returns>
        byte[] GetBufferWithLength();

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
    public interface IMessageWrapper
    {
        IMessageHeader RequestMessageHeader { get; }

        IMessageHeader ResponseMessageHeader { get; }

        /// <summary>
        /// 创建消息
        /// </summary>
        /// <param name="content"></param>
        /// <param name="containHeader"></param>
        /// <returns></returns>
        IMessage CreateMessage<T>(T content, bool containHeader = false) where T : class;
    }
}