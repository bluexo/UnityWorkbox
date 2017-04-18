//////////////////////////////////////////////////////////////
// 跨平台TCP连接 支持 Standalone/iOS/Android/Windows(UWP)                                        
//////////////////////////////////////////////////////////////                                                           

using System;
using System.Collections;
using UnityEngine;
using System.Text;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Storage.Streams;
using Windows.Networking.Sockets;
#else
using System.IO;
using System.Net.Sockets;
#endif

namespace Arthas.Network
{
    public class TCPConnect
    {
        public bool IsConnected { get; private set; }
        public string Address { get; private set; }
        public event Action<IMessage> MessageRespondEvent;
        public event Action DisconnectEvent;
        const int READ_BUFFER_SIZE = 8192;
        const int MSG_LEN_SIZE = 4;
        private byte[] readBuffer = new byte[READ_BUFFER_SIZE];
        private readonly IMessageHead messageHead;


        /// <summary>
        /// TCP客户端
        /// </summary>
#if WINDOWS_UWP
        private StreamSocket client = new StreamSocket();
        private DataReader reader;
        private DataWriter writer;
#else
        private TcpClient client;
#endif
        public TCPConnect(IMessageHead head = null)
        {
            if (head == null) messageHead = new DefaultMessageHead() { SerialNumber = 0 , Dest = 2 };
            else messageHead = head;
            IsConnected = false;
        }

#if WINDOWS_UWP
        /// <summary>
        /// 使用<see cref="StreamSocket"/> 建立连接并且异步接收数据
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public async void Connect(string ip, int port)
        {
            try
            {
                var serverHost = new HostName(ip);
                await client.ConnectAsync(serverHost, port.ToString());
                IsConnected = true;
                Address = ip + port;
                ReadAsync();
            }
            catch (Exception ex)
            {
                client.Dispose();
                client = null;
                IsConnected = false;
                Debug.LogErrorFormat("Can not connect to Server. Detail: {0}.\n{1}", ex.Message, ex.StackTrace);
            }
        }
#else
        /// <summary>
        /// 使用 <see cref="TcpClient"/> 建立连接并异步接收数据
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            Debug.LogFormat("Connect to server ip:{0} port:{1}", ip, port);
            try
            {
                client = new TcpClient(ip, port);
                client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, Read, null);
                IsConnected = true;
                Address = ip + ":" + port;
            }
            catch (Exception ex)
            {
                client = null;
                IsConnected = false;
                Debug.LogErrorFormat("Can not connect to Server. Detail: {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
#endif

#if WINDOWS_UWP
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public async void SendData(int userId, int msgId, string data)
        {
            using (writer = new DataWriter(client.OutputStream))
            {
                try
                {
                    writer.WriteBytes(BitConverter.GetBytes(userId));
                    writer.WriteBytes(BitConverter.GetBytes(msgId));
                    writer.WriteBytes(Encoding.UTF8.GetBytes(data));
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                    writer.DetachStream();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

#else
        /// <summary>
        /// 发送数据到服务器
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="data"></param>
        public void SendData(int userId, int msgId, string data)
        {
            if (client == null)
                return;
            var writer = new BinaryWriter(client.GetStream());
            var userIdBuffer = BitConverter.GetBytes(userId);
            var msgIdBuffer = BitConverter.GetBytes(msgId);
            var dataBuffer = Encoding.UTF8.GetBytes(data);
            var fullBuffer = new byte[userIdBuffer.Length + msgIdBuffer.Length + dataBuffer.Length];
            Buffer.BlockCopy(userIdBuffer, 0, fullBuffer, 0, userIdBuffer.Length);
            Buffer.BlockCopy(msgIdBuffer, 0, fullBuffer, userIdBuffer.Length, msgIdBuffer.Length);
            Buffer.BlockCopy(dataBuffer, 0, fullBuffer, userIdBuffer.Length + msgIdBuffer.Length, dataBuffer.Length);
            writer.Write(fullBuffer);
            writer.Flush();
        }

        public void Send(short command, object body)
        {
            if (client == null) return;
            messageHead.CommandId = command;
            var msg = MessageFactory.CreateMessage(messageHead, body);
            var buffer = msg.GetBuffer();
            var writer = new BinaryWriter(client.GetStream());
            writer.Write(buffer);
            writer.Flush();
        }
#endif

#if WINDOWS_UWP
        public async void ReadAsync()
        {
            try
            {
                reader = new DataReader(client.InputStream);
                //读取字符串长度
                await reader.LoadAsync(MSG_LEN_SIZE);
                var lenBuffer = new byte[MSG_LEN_SIZE];
                reader.ReadBytes(lenBuffer);
                //读取字符串内容
                var actualStringLength = await reader.LoadAsync(BitConverter.ToUInt32(lenBuffer, 0));
                var dataArray = new byte[actualStringLength];
                reader.ReadBytes(dataArray);
                ProcessCommands(dataArray);
                reader.DetachStream();
                ReadAsync();
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("Received data: \"{0}\"", "Read stream failed with error: " + exception.Message));
                reader.DetachStream();
                Close();
            }
        }
#else

        private void Read(IAsyncResult ar)
        {
            try
            {
                var stream = client.GetStream();
                var lengthToRead = stream.EndRead(ar);
                if (lengthToRead < 1 || lengthToRead > READ_BUFFER_SIZE)
                {
                    Debug.Log("Stream read error!");
                    Close();
                    return;
                }
                var dataArray = new byte[lengthToRead];
                Buffer.BlockCopy(readBuffer, MSG_LEN_SIZE, dataArray, 0, lengthToRead);
                ProcessCommands(dataArray);
                stream.BeginRead(readBuffer, 0, READ_BUFFER_SIZE, Read, null);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Server Disconnected, Detail:{0},\n{1}", ex.Message, ex.StackTrace);
                Close();
            }
        }
#endif
        private void ProcessCommands(byte[] dataArray)
        {
            var message = MessageFactory.CreateMessage(messageHead, dataArray);
            if (MessageRespondEvent != null)
                MessageRespondEvent(message);
#if UNITY_EDITOR
            Debug.LogFormat("MsgID:{0}", message.Head);
#endif
        }

        public void Close()
        {
            if (DisconnectEvent != null) DisconnectEvent();
            IsConnected = false;
            if (client == null) return;
#if WINDOWS_UWP
            client.Dispose();
#else
            client.Close();
#endif
        }
    }
}
