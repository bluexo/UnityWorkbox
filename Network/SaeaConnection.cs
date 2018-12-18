using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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

        public override string ToString()
        {
            return $"{nameof(MessageSize)} = {MessageSize},\n" +
                    $"{nameof(DataStartOffset)} = {DataStartOffset},\n" +
                    $"{nameof(NextReceiveOffset)} = {NextReceiveOffset},\n";
        }

        #region IDisposable Members

        public void Dispose()
        {
            try { Socket.Shutdown(SocketShutdown.Send); }
            catch (Exception) { }

            try { Socket.Close(); }
            catch (Exception) { }
        }

        #endregion
    }

    public class SaeaConnection : IConnection, IDisposable
    {
        private const int BufferSize = 65536;
        private const int MessageHeaderSize = sizeof(ushort);

        private Socket clientSocket;
        private IPEndPoint hostEndPoint;
        private SocketAsyncEventArgs sendEventArgs;
        private SocketAsyncEventArgs receiveEventArgs;

        private readonly AutoResetEvent autoConnectEvent, autoSendEvent;
        private readonly BlockingCollection<byte[]> sendingQueue;
        private readonly BlockingCollection<byte[]> receivedMessageQueue;
        private readonly Thread sendMessageWorker;
        private readonly Thread processReceivedMessageWorker;

        public event Action<byte[]> MessageRespondEvent;

        private bool connected = false;
        public bool IsConnected => connected && clientSocket != null && clientSocket.Connected;

        public SaeaConnection()
        {
            autoConnectEvent = new AutoResetEvent(false);
            autoSendEvent = new AutoResetEvent(false);
            sendingQueue = new BlockingCollection<byte[]>();
            receivedMessageQueue = new BlockingCollection<byte[]>();
            sendMessageWorker = new Thread(new ThreadStart(SendQueueMessage));
            processReceivedMessageWorker = new Thread(new ThreadStart(ProcessReceivedMessage));
        }

        public void Connect(string hostname, int port, Action<object> callback = null)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(hostname);
            AddressFamily _family = AddressFamily.Unknown;

            try
            {
                foreach (IPAddress address in addresses)
                {
                    if (clientSocket == null)
                    {
                        Debug.Assert(address.AddressFamily == AddressFamily.InterNetwork || address.AddressFamily == AddressFamily.InterNetworkV6);
                        if ((address.AddressFamily == AddressFamily.InterNetwork && Socket.OSSupportsIPv4) || Socket.OSSupportsIPv6)
                        {
                            Connect(new IPEndPoint(address, port));
                        }

                        _family = address.AddressFamily;
                        break;
                    }
                    if (address.AddressFamily == _family || _family == AddressFamily.Unknown)
                    {
                        Connect(new IPEndPoint(address, port));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                callback?.Invoke(ex);
                connected = false;
            }
        }

        public void Connect(IPEndPoint hostEndPoint)
        {
            this.hostEndPoint = hostEndPoint;
            var addressFamily = Socket.OSSupportsIPv6
                ? AddressFamily.InterNetworkV6
                : AddressFamily.InterNetwork;

            clientSocket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                ExclusiveAddressUse = true,
            };

            sendEventArgs = new SocketAsyncEventArgs
            {
                UserToken = clientSocket,
                RemoteEndPoint = this.hostEndPoint
            };

            sendEventArgs.Completed += OnSend;

            receiveEventArgs = new SocketAsyncEventArgs
            {
                UserToken = new AsyncUserToken(clientSocket),
                RemoteEndPoint = this.hostEndPoint
            };
            receiveEventArgs.SetBuffer(new byte[BufferSize], 0, BufferSize);
            receiveEventArgs.Completed += OnReceive;

            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs
            {
                UserToken = clientSocket,
                RemoteEndPoint = hostEndPoint
            };
            connectArgs.Completed += OnConnect;
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

        public void Disconnect()
        {
            clientSocket.Disconnect(false);
            connected = false;
        }

        public void Send(byte[] message) => sendingQueue.Add(message);

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            autoConnectEvent.Set();
            connected = (e.SocketError == SocketError.Success);
        }

        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
                ProcessError(e);
            autoSendEvent.Set();
        }

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

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessReceive(e);
            }
            catch (Exception ex)
            {
                var token = e.UserToken as AsyncUserToken;
                Debug.LogError($"{ex.Message},{ex.StackTrace},\nBuffer:{e.Buffer.Length}\nToken:{token.ToString()}");
                Close();
            }
        }

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
            if (token.NextReceiveOffset >= e.Buffer.Length)
            {
                token.NextReceiveOffset = 0;
                if (token.DataStartOffset < e.Buffer.Length)
                {
                    var notProcessDataSize = e.Buffer.Length - token.DataStartOffset;
                    Buffer.BlockCopy(e.Buffer, token.DataStartOffset, e.Buffer, 0, notProcessDataSize);
                    token.NextReceiveOffset = notProcessDataSize;
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

            if (dataStartOffset >= e.Buffer.Length - MessageHeaderSize)
                return;

            if (token.MessageSize == null)
            {
                if (totalReceivedDataSize > MessageHeaderSize)
                {
                    var headerData = new byte[MessageHeaderSize];
                    Buffer.BlockCopy(e.Buffer, dataStartOffset, headerData, 0, MessageHeaderSize);
                    var messageSize = BitConverter.ToInt16(headerData, 0);


                    token.MessageSize = messageSize;
                    token.DataStartOffset = dataStartOffset + MessageHeaderSize;
                    ProcessReceivedData(token.DataStartOffset, totalReceivedDataSize, alreadyProcessedDataSize + MessageHeaderSize, token, e);
                }
            }
            else
            {
                var messageSize = token.MessageSize.Value;
                if (totalReceivedDataSize - alreadyProcessedDataSize >= messageSize)
                {
                    var messageData = new byte[messageSize];
                    Buffer.BlockCopy(e.Buffer, dataStartOffset, messageData, 0, messageSize);
                    ProcessMessage(messageData);

                    token.DataStartOffset = dataStartOffset + messageSize;
                    token.MessageSize = null;
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
            var token = e.UserToken as AsyncUserToken;
            var s = token.Socket;
            if (!s.Connected) return;
            try
            {
                Disconnect();
                s.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{ex.Message},{ex.StackTrace}");
            }
            finally
            {
                Debug.LogError($"Process SocketError={e.SocketError}");
                if (s.Connected)
                    s.Close();
            }
        }

        public void Dispose()
        {
            Disconnect();
            clientSocket.Shutdown(SocketShutdown.Both);
            sendMessageWorker.Abort();
            processReceivedMessageWorker.Abort();
            autoConnectEvent.Close();
            if (!clientSocket.Connected) return;
            clientSocket.Close();
        }

        public void Close() => Dispose();
    }
}
