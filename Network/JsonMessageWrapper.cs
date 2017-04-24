using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Arthas.Network
{
    public class JsonMessage : DefaultMessage
    {
        private string jsonString;

        public JsonMessage(IMessageHeader header, string json = null, Encoding encoding = null)
        {
            Header = header;
            jsonString = json;
            if (json != null)
            {
                bodyBuffer = (encoding ?? Encoding.UTF8).GetBytes(json);
            }
        }

        public override T GetValue<T>()
        {
            return JsonUtility.FromJson<T>(jsonString);
        }
    }

    public class JsonMessageWrapper : IMessageWrapper
    {
        public IMessageHeader RequestMessageHeader { get; private set; }

        public IMessageHeader ResponseMessageHeader { get; private set; }

        public JsonMessageWrapper()
        {
            RequestMessageHeader = new RequestHeader();
            ResponseMessageHeader = new ResponseHeader();
        }

        public IMessage FromString(string str)
        {
            return new JsonMessage(RequestMessageHeader, str);
        }

        public IMessage FromObject(object obj)
        {
            var json = JsonUtility.ToJson(obj);
            return new JsonMessage(RequestMessageHeader, json);
        }

        public IMessage FromBuffer(byte[] buffer, bool containHeader = false)
        {
            ResponseMessageHeader.Overwrite(buffer);
            if (containHeader) ResponseMessageHeader.ExceptHeader(ref buffer);
            return new JsonMessage(ResponseMessageHeader, Encoding.UTF8.GetString(buffer));
        }
    }
}
