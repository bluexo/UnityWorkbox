using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Arthas.Network
{
    public class AsyncUserToken : IDisposable
    {
        public Socket Socket { get; private set; }
        public int? MessageSize { get; set; }
        public int DataStartOffset { get; set; }
        public int NextReceiveOffset { get; set; }

        public AsyncUserToken(Socket socket)
        {
            Socket = socket;
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                Socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            { }

            try
            {
                Socket.Close();
            }
            catch (Exception)
            { }
        }

        #endregion
    }

    public class SaeaConnection : IConnection, IDisposable
    {
        private int bufferSize = 60000;
        private const int MessageHeaderSize = sizeof(ushort);

        private Socket clientSocket;
        private IPEndPoint hostEndPoint;
        private SocketAsyncEventArgs sendEventArgs;
        private SocketAsyncEventArgs receiveEventArgs;

        private readonly AutoResetEvent autoConnectEvent;
        private readonly AutoResetEvent autoSendEvent;
        private readonly BlockingCollection<byte[]> sendingQueue;
        private readonly BlockingCollection<byte[]> receivedMessageQueue;
        private readonly Thread sendMessageWorker;
        private readonly Thread processReceivedMessageWorker;

        private int _receivedMessageCount;
        private Action<object> connectedCallback;
        public event Action<byte[]> MessageRespondEvent;

        public bool IsConnected { get; private set; }

        public SaeaConnection()
        {
            autoConnectEvent = new AutoResetEvent(false);
            autoSendEvent = new AutoResetEvent(false);
            sendingQueue = new BlockingCollection<byte[]>();
            receivedMessageQueue = new BlockingCollection<byte[]>();
            sendMessageWorker = new Thread(new ThreadStart(SendQueueMessage));
            processReceivedMessageWorker = new Thread(new ThreadStart(ProcessReceivedMessage));
        }

        public void Connect(string ip, int port, Action<object> callback = null)
        {
            IPAddress address = null;
            if (IPAddress.TryParse(ip, out address))
            {
                connectedCallback = callback;
                var endPoint = new IPEndPoint(address, port);
                Connect(endPoint);
            }
            else
            {
                Debug.LogError($"Cannot parse ip {ip} !");
            }
        }

        public void Connect(IPEndPoint hostEndPoint)
        {
            this.hostEndPoint = hostEndPoint;

            clientSocket = new Socket(this.hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sendEventArgs = new SocketAsyncEventArgs
            {
                UserToken = clientSocket,
                RemoteEndPoint = this.hostEndPoint
            };

            sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSend);

            receiveEventArgs = new SocketAsyncEventArgs
            {
                UserToken = new AsyncUserToken(clientSocket),
                RemoteEndPoint = this.hostEndPoint
            };
            receiveEventArgs.SetBuffer(new byte[bufferSize], 0, bufferSize);
            receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);

            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs
            {
                UserToken = clientSocket,
                RemoteEndPoint = hostEndPoint
            };
            connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);

            clientSocket.ConnectAsync(connectArgs);
            autoConnectEvent.WaitOne();

            SocketError errorCode = connectArgs.SocketError;
            if (errorCode != SocketError.Success)
                throw new SocketException((int)errorCode);

            sendMessageWorker.Start();
            processReceivedMessageWorker.Start();

            if (!clientSocket.ReceiveAsync(receiveEventArgs))
                ProcessReceive(receiveEventArgs);
        }

        public void Disconnect() => clientSocket.Disconnect(false);

        public void Send(byte[] message) => sendingQueue.Add(message);

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            autoConnectEvent.Set();
            IsConnected = (e.SocketError == SocketError.Success);
            connectedCallback?.Invoke(e);
        }

        private void OnSend(object sender, SocketAsyncEventArgs e) => autoSendEvent.Set();

        private void SendQueueMessage()
        {
            while (true)
            {
                var message = sendingQueue.Take();
                if (message != null)
                {
                    sendEventArgs.SetBuffer(message, 0, message.Length);
                    clientSocket.SendAsync(sendEventArgs);
                    autoSendEvent.WaitOne();
                }
            }
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e) => ProcessReceive(e);

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success)
            {
                ProcessError(e);
                return;
            }

            var token = e.UserToken as AsyncUserToken;
            ProcessReceivedData(token.DataStartOffset, token.NextReceiveOffset - token.DataStartOffset + e.BytesTransferred, 0, token, e);
            token.NextReceiveOffset += e.BytesTransferred;
            if (token.NextReceiveOffset == e.Buffer.Length)
            {
                token.NextReceiveOffset = 0;
                if (token.DataStartOffset < e.Buffer.Length)
                {
                    var notYesProcessDataSize = e.Buffer.Length - token.DataStartOffset;
                    Buffer.BlockCopy(e.Buffer, token.DataStartOffset, e.Buffer, 0, notYesProcessDataSize);
                    token.NextReceiveOffset = notYesProcessDataSize;
                }
                token.DataStartOffset = 0;
            }

            e.SetBuffer(token.NextReceiveOffset, e.Buffer.Length - token.NextReceiveOffset);
            if (!token.Socket.ReceiveAsync(e))
                ProcessReceive(e);
        }

        private void ProcessReceivedData(int dataStartOffset,
            int totalReceivedDataSize,
            int alreadyProcessedDataSize,
            AsyncUserToken token,
            SocketAsyncEventArgs e)
        {
            if (alreadyProcessedDataSize >= totalReceivedDataSize)
                return;

            if (token.MessageSize == null)
            {
                //如果之前接收到到数据加上当前接收到的数据大于消息头的大小，则可以解析消息头
                if (totalReceivedDataSize > MessageHeaderSize)
                {
                    //解析消息长度
                    var headerData = new byte[MessageHeaderSize];
                    Buffer.BlockCopy(e.Buffer, dataStartOffset, headerData, 0, MessageHeaderSize);
                    var messageSize = BitConverter.ToInt16(headerData, 0);

                    token.MessageSize = messageSize;
                    token.DataStartOffset = dataStartOffset + MessageHeaderSize;

                    //递归处理
                    ProcessReceivedData(token.DataStartOffset, totalReceivedDataSize, alreadyProcessedDataSize + MessageHeaderSize, token, e);
                }
            }
            else
            {
                var messageSize = token.MessageSize.Value;
                //判断当前累计接收到的字节数减去已经处理的字节数是否大于消息的长度，如果大于，则说明可以解析消息了
                if (totalReceivedDataSize - alreadyProcessedDataSize >= messageSize)
                {
                    var messageData = new byte[messageSize];
                    Buffer.BlockCopy(e.Buffer, dataStartOffset, messageData, 0, messageSize);
                    ProcessMessage(messageData);

                    //消息处理完后，需要清理token，以便接收下一个消息
                    token.DataStartOffset = dataStartOffset + messageSize;
                    token.MessageSize = null;

                    //递归处理
                    ProcessReceivedData(token.DataStartOffset, totalReceivedDataSize, alreadyProcessedDataSize + messageSize, token, e);
                }
            }
        }

        private void ProcessMessage(byte[] messageData) => receivedMessageQueue.Add(messageData);

        private void ProcessReceivedMessage()
        {
            while (true)
            {
                var message = receivedMessageQueue.Take();
                if (message != null)
                    MessageRespondEvent?.Invoke(message);
            }
        }

        private void ProcessError(SocketAsyncEventArgs e)
        {
            Debug.LogError($"Socket error {e.SocketError}");
            var token = e.UserToken as AsyncUserToken;
            var s = token.Socket;
            if (!s.Connected) return;
            // close the socket associated with the client
            try
            {
                s.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                // throws if client process has already closed
                Debug.LogError($"{ex.Message},{ex.StackTrace}");
            }
            finally
            {
                if (s.Connected)
                    s.Close();
            }
        }

        public void Dispose()
        {
            autoConnectEvent.Close();
            if (!clientSocket.Connected) return;
            clientSocket.Close();
        }

        public void Close() => Dispose();
    }
}
