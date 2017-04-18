using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 消息头
/// </summary>
public interface IMessageHead
{
    bool IsLittleEndian { get; }

    /// <summary>
    /// 序号
    /// </summary>
    int SerialNumber { get; set; }

    /// <summary>
    /// 命令Id
    /// </summary>
    short CommandId { get; set; }

    /// <summary>
    /// 获取消息头的字节
    /// </summary>
    /// <returns></returns>
    byte[] GetBuffer();

    /// <summary>
    /// 排除消息体
    /// </summary>
    /// <returns></returns>
    void ExceptHead(ref byte[] buffer);
}

/// <summary>
/// 消息
/// </summary>
public interface IMessage
{
    /// <summary>
    /// 消息头
    /// </summary>
    IMessageHead Head { get; }

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