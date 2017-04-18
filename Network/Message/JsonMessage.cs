using System;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// 消息包装器
/// </summary>
public class JsonMessage : IMessage
{
    public IMessageHead Head { get; private set; }
    
    private string json;
  
    private object obj;

	public JsonMessage(string msg)
    {
        obj = JsonConvert.DeserializeObject(msg);
    }

    public JsonMessage(object obj)
    {
        json = JsonConvert.SerializeObject(obj);
    }

    /// <summary>
    /// 获取消息的字节数组
    /// </summary>
    /// <returns></returns>
	public byte[] GetBuffer(bool containsHead = false)
    {
        var headBuffer = Head.GetBuffer();
        var jsonBuffer = Encoding.UTF8.GetBytes(json);
        var buffer = new byte[headBuffer.Length + jsonBuffer.Length];
        headBuffer.CopyTo(buffer, 0);
        jsonBuffer.CopyTo(buffer, headBuffer.Length);
        return buffer;
    }

    public byte[] GetBufferWithLength()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取消息的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
	public T GetValue<T>()
    {
        return (T)obj;
    }
}

