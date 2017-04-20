using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arthas.Network
{
    public enum MessageFormat { Binary, Json, ProtoBuf }


    /// <summary>
    /// TCP网络
    /// </summary>
    public class TCPNetwork : SingletonBehaviour<TCPNetwork>
    {
        /// <summary>
        /// 推送事件
        /// </summary>
        public static event Action<IMessage> PushEvent;
        /// <summary>
        /// 网络连接事件
        /// </summary>
        public static event Action ConnectedEvent;
        /// <summary>
        /// 网络断开事件
        /// </summary>
        public static event Action DisconnectedEvent;
        /// <summary>
        /// 错误回调
        /// </summary>
        private static Action<string> ErrorCallback;

        public static IMessageWrapper MessageWrapper { get; private set; }

        public static bool IsConnected { get { return connector.IsConnected; } }

        private readonly static TCPConnect connector = new TCPConnect();
        private readonly static Queue<IMessage> msgQueue = new Queue<IMessage>();
        private readonly static Dictionary<int, Action<IMessage>> responseActions = new Dictionary<int, Action<IMessage>>();

        private WaitForSeconds heartbeatWaiter, timeoutWaiter;
        private Coroutine checkTimeout;
        private float currentTime, connectCheckDuration = .1f;
        private static object enterLock = new object();

        [SerializeField] private float connectTimeout = 10f, heartbeatDuration = 8f;
        [SerializeField] private bool isLittleEndian = true;
        private static int messageSerialNumber = 1;

        protected override void Awake()
        {
            base.Awake();
            timeoutWaiter = new WaitForSeconds(connectCheckDuration);
            heartbeatWaiter = new WaitForSeconds(heartbeatDuration);
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public void Connect(string ip, int port, IMessageWrapper wrapper = null)
        {
            if (connector.IsConnected) connector.Close();
            MessageWrapper = wrapper ?? new DefaultMessageWrapper(isLittleEndian);
            connector.Connect(ip, port);
            checkTimeout = StartCoroutine(CheckeTimeout());
        }

        /// <summary>
        /// 根据网络配置连接到服务器
        /// </summary>
        /// <param name="configuration"></param>
        public static void Connect(Action callback = null,
            Action<string> error = null,
            IMessageWrapper wrapper = null)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE
            if (Application.internetReachability == NetworkReachability.NotReachable
                && onError != null)
            {
                onError.Invoke("Network not reachable , please check you network setting!");
                return;
            }
#endif
            ErrorCallback = error;
            ConnectedEvent = callback;
            Instance.Connect(NetworkConfiguration.Current.ip, NetworkConfiguration.Current.port, wrapper);
        }

        private IEnumerator CheckeTimeout()
        {
            while (true)
            {
                yield return timeoutWaiter;
                if (connector.IsConnected)
                {
                    OnConnected();
                    StopCoroutine(checkTimeout);
                    yield break;
                }
                if ((currentTime += Time.deltaTime) > connectTimeout)
                {
                    currentTime = 0;
                    StopCoroutine(checkTimeout);
                    if (ErrorCallback != null)
                    {
                        ErrorCallback("Cannot connect to server , please check your network!");
                    }
                }
            }
        }

        private void OnConnected()
        {
            if (ConnectedEvent != null)
                ConnectedEvent();
            //StartCoroutine(Heartbeat());
            HandleEvent(true);
            Debug.LogFormat("Connect server success, Addr : {0} .", connector.Address);
        }

        private void OnDisconnected()
        {
            if (DisconnectedEvent != null)
                DisconnectedEvent();
            HandleEvent(false);
        }

        private void OnMessageRespond(byte[] buffer)
        {
            lock (enterLock)
            {
                var lenthbytes = new byte[sizeof(int)];
                Buffer.BlockCopy(buffer, 0, lenthbytes, 0, sizeof(int));
                var lenth = BitConverter.ToInt32(isLittleEndian ? lenthbytes : lenthbytes.Reverse(), 0);
                var dest = new byte[lenth - sizeof(int)];
                Buffer.BlockCopy(buffer, sizeof(int), dest, 0, lenth - sizeof(int));
                msgQueue.Enqueue(MessageWrapper.CreateMessage(dest, true));
            }
        }

        private void Update()
        {
            if (msgQueue.Count > 0)
            {
                var message = msgQueue.Dequeue();
                if (responseActions.Count > 0
                    && responseActions.ContainsKey(message.Header.Command))
                {
                    var action = responseActions[message.Header.Command];
                    if (action != null)
                    {
                        action.Invoke(message);
                        responseActions.Remove(message.Header.Command);
                        return;
                    }
                }
                else if (PushEvent != null)
                {
                    PushEvent(message);
                }
            }
        }

        public IEnumerator Heartbeat()
        {
            while (connector.IsConnected)
            {
                yield return heartbeatWaiter;
                //connector.Send(1, -1, string.Empty);
            }
        }

        public static void Send(short cmd, byte[] buf, Action<IMessage> callback)
        {
            if (messageSerialNumber >= int.MaxValue) messageSerialNumber = 1;
            MessageWrapper.RequestMessageHeader.SerialNumber = messageSerialNumber++;
            MessageWrapper.RequestMessageHeader.Descriptor = 10;
            MessageWrapper.RequestMessageHeader.Command = cmd;
            responseActions.Add(MessageWrapper.RequestMessageHeader.Command, callback);
            try
            {
                var message = MessageWrapper.CreateMessage(buf);
                var buffer = message.GetBufferWithLength();
                connector.Send(buffer);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        private void HandleEvent(bool bind)
        {
            if (bind)
            {
                connector.MessageRespondEvent += OnMessageRespond;
                connector.DisconnectEvent += OnDisconnected;
            }
            else
            {
                connector.MessageRespondEvent -= OnMessageRespond;
                connector.DisconnectEvent -= OnDisconnected;
            }
        }
    }
}