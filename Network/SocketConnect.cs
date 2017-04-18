using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class SocketConnect : MonoBehaviour
{
    public static SocketConnect Instance;
    
    private Socket socket;
    private Thread thread;

    private bool isReceive = true;

    private void Awake()
    {
        Instance = this;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var address = IPAddress.Parse(NetworkConfiguration.Current.ip);
        var endpoint = new IPEndPoint(address, NetworkConfiguration.Current.port);
        var result = socket.BeginConnect(endpoint, new AsyncCallback(ConnectCallback), socket);
        if (result.AsyncWaitHandle.WaitOne(5000, true))
        {
            thread = new Thread(new ThreadStart(Receive));
            thread.Start();
        }
        else
        {
            Close();
            Debug.Log("connect Time Out");
        }
    }

    private void ConnectCallback(IAsyncResult asyncConnect)
    {
        Debug.Log("Connect success");
    }

    private void Receive()
    {
        while (isReceive)
        {
            if (!socket.Connected)
            {
                Debug.Log("Failed to clientSocket server.");
                socket.Close();
                break;
            }
            try
            {
                byte[] bytes = new byte[4096];
                int i = socket.Receive(bytes);
                if (i <= 0)
                {
                    socket.Close();
                    break;
                }
                Debug.Log("Server say :" + Encoding.Default.GetString(bytes));
            }
            catch (Exception e)
            {
                Debug.Log("Disconnected from server , info :" + e);
                socket.Close();
                break;
            }
        }
    }

    //关闭Socket
    public void Close()
    {
        if (socket != null && socket.Connected)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
        socket = null;
    }

    /// <summary>
    /// 向服务端发送一条字符串
    /// </summary>
    /// <param name="str"></param>
    public void Send(string str)
    {
        var headBuffer = new DefaultMessageHead(false) { SerialNumber = 0, Dest = 2, CommandId = 10001 }.GetBuffer();
        var contentBuffer = Encoding.UTF8.GetBytes(str);
        var length = headBuffer.Length + contentBuffer.Length;
        var lengthBytes = BitConverter.GetBytes(length).ReverseToBytes();
        var buffer = new byte[headBuffer.Length + contentBuffer.Length + sizeof(int)];
        Buffer.BlockCopy(lengthBytes, 0, buffer, 0, sizeof(int));
        Buffer.BlockCopy(headBuffer, 0, buffer, sizeof(int), headBuffer.Length);
        Buffer.BlockCopy(contentBuffer, 0, buffer, headBuffer.Length + sizeof(int), contentBuffer.Length);

        if (!socket.Connected)
        {
            socket.Close();
            return;
        }
        try
        {
            var asyncSend = socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            if (!asyncSend.AsyncWaitHandle.WaitOne(5000, true))
            {
                socket.Close();
                Debug.Log("Failed to SendMessage server.");
            }
        }
        catch
        {
            socket.Close();
            Debug.Log("send message error");
        }
    }

    public void Send(IMessage message)
    {
        var buffer = message.GetBufferWithLength();
        if (!socket.Connected)
        {
            socket.Close();
            return;
        }
        try
        {
            var asyncSend = socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            if (asyncSend.AsyncWaitHandle.WaitOne(5000, true))
            {
                Debug.LogError("Send data to server fail or timeout!");
                socket.Close();
            }
        }
        catch (Exception ex)
        {
            socket.Close();
            Debug.LogErrorFormat("Send message error , detail:", ex.Message);
        }
    }

    private void SendCallback(IAsyncResult asyncConnect)
    {
        
    }

    void OnApplicationQuit()
    {
        isReceive = false;
        thread.Abort();
        Close();
    }
}