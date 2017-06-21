//////////////////////////////////////////////////////////////
// 跨平台TCP连接 支持 Standalone/iOS/Android/Windows(UWP)                                        
//////////////////////////////////////////////////////////////                                                           

using System;
using UnityEngine;

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
    public class TCPConnector : IConnector
    {
        public bool IsConnected { get; private set; }
        public string Address { get; private set; }
        public event Action<byte[]> MessageRespondEvent;
        const int READ_BUFFER_SIZE = 4096;
        const int MSG_LEN_SIZE = 4;
        private byte[] readBuffer = new byte[READ_BUFFER_SIZE];

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
        public TCPConnector()
        {
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
            try {
                client = new TcpClient(ip, port);
                var stream = client.GetStream();
                stream.BeginRead(readBuffer, 0, READ_BUFFER_SIZE, Read, null);
                IsConnected = true;
                Address = string.Format("{0}:{1}", ip, port);
            }
            catch (Exception ex) {
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
        public async void Send(byte[] buffer)
        {
            using (writer = new DataWriter(client.OutputStream))
            {
                try
                {
                    writer.WriteBytes(buffer);
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
        public void Send(byte[] buffer)
        {
            if (client == null || !client.Connected) return;
            var writer = new BinaryWriter(client.GetStream());
            writer.Write(buffer);
            writer.Flush();
        }
#endif

#if WINDOWS_UWP
        private async void ReadAsync()
        {
            try
            {
                reader = new DataReader(client.InputStream);
                await reader.LoadAsync(MSG_LEN_SIZE);
                var lenBuffer = new byte[MSG_LEN_SIZE];
                reader.ReadBytes(lenBuffer);
                var actualStringLength = await reader.LoadAsync(BitConverter.ToUInt32(lenBuffer, 0));
                var dataArray = new byte[actualStringLength];
                reader.ReadBytes(dataArray);
                if (MessageRespondEvent != null) MessageRespondEvent(dataArray);
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
            try {
                if (client == null) return;
                var stream = client.GetStream();
                var lengthToRead = stream.EndRead(ar);
                if (lengthToRead < 1 || lengthToRead > READ_BUFFER_SIZE) {
                    Debug.LogError("Stream read error , network will be closed!");
                    return;
                }
                var arr = new byte[lengthToRead];
                Buffer.BlockCopy(readBuffer, 0, arr, 0, lengthToRead);
                if (MessageRespondEvent != null) MessageRespondEvent(arr);
                stream.BeginRead(readBuffer, 0, READ_BUFFER_SIZE, Read, null);
            }
            catch (Exception ex) {
                Debug.LogErrorFormat("Server Disconnected, Detail:{0},\n{1}", ex.Message, ex.StackTrace);
                Close();
            }
        }
#endif

        public void Close()
        {
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
