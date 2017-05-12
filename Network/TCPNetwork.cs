using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arthas.Network
{
    /// <summary>
    /// TCP网络
    /// </summary>
    public class TCPNetwork : SingletonBehaviour<TCPNetwork>
    {
        /// <summary>
        /// 推送事件
        /// </summary>
        public static event Action<INetworkMessage> PushEvent;

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

        public static INetworkMessageHandler MessageHandler { get; private set; }

        public static bool IsLittleEndian
        {
            get { return Instance.isLittleEndian; }
            set { Instance.isLittleEndian = value; }
        }

        public static bool IsConnected { get { return connector.IsConnected; } }

        private readonly static TCPConnect connector = new TCPConnect();
        private readonly static Queue<INetworkMessage> msgQueue = new Queue<INetworkMessage>();
        private readonly static Dictionary<object, Action<INetworkMessage>> responseActions = new Dictionary<object, Action<INetworkMessage>>();

        private WaitForSeconds heartbeatWaiter, timeoutWaiter;
        private Coroutine checkTimeout;
        private float currentTime, connectCheckDuration = .1f;
        private static object enterLock = new object();

        [SerializeField]
        private float connectTimeout = 10f, heartbeatInterval = 8f;
        [SerializeField]
        private bool isLittleEndian = true;

        protected override void Awake()
        {
            base.Awake();
            timeoutWaiter = new WaitForSeconds(connectCheckDuration);
            heartbeatWaiter = new WaitForSeconds(heartbeatInterval);
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        private void Connect(string ip, int port, INetworkMessageHandler handler = null)
        {
            if (connector.IsConnected) connector.Close();
            MessageHandler = handler ?? new DefaultMessageHandler();
            connector.Connect(ip, port);
            checkTimeout = StartCoroutine(CheckeTimeout());
#if UNITY_EDITOR
            Debug.LogFormat("Connect to server , <color=cyan>Addr:[{0}:{1}] ,wrapper:{2}.</color>", ip, port, MessageHandler.GetType());
#endif
        }

        public static void Connect(Action callback = null,
           Action<string> error = null,
           INetworkMessageHandler handler = null)
        {
            Connect(NetworkConfiguration.Current.ip,
                NetworkConfiguration.Current.port,
                callback,
                error,
                handler);
        }

        /// <summary>
        /// 根据网络配置连接到服务器
        /// </summary>
        /// <param name="configuration"></param>
        public static void Connect(string ip, 
            int port,
            Action callback = null,
            Action<string> error = null,
            INetworkMessageHandler handler = null)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE
            if (Application.internetReachability == NetworkReachability.NotReachable
                && error != null)
            {
                error.Invoke("Network not reachable , please check you network setting!");
                return;
            }
#endif
            ErrorCallback = error;
            ConnectedEvent = callback;
            Instance.Connect(ip, port, handler);
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
            HandleEvent(true);
            Debug.LogFormat("Connect server success, Addr : {0} .", connector.Address);
        }

        private void OnDisconnected()
        {
            if (DisconnectedEvent != null) DisconnectedEvent();
            HandleEvent(false);
        }

        private void OnMessageRespond(byte[] buffer)
        {
            lock (enterLock)
            {
                var msg = MessageHandler.ParseMessage(buffer);
                msgQueue.Enqueue(msg);
#if UNITY_EDITOR
                Debug.LogFormat("<color=blue>[TCPNetwork]</color> [Receive] << CMD:{0},TIME:{1}", msg.Command, DateTime.Now);
#endif
            }
        }

        private void Update()
        {
            if (msgQueue.Count > 0)
            {
                var message = msgQueue.Dequeue();
                if (responseActions.Count > 0
                    && responseActions.ContainsKey(message.Command))
                {
                    var action = responseActions[message.Command];
                    if (action != null)
                    {
                        action.Invoke(message);
                        responseActions.Remove(message.Command);
                        return;
                    }
                }
                else if (PushEvent != null)
                {
                    PushEvent(message);
                }
            }
        }

        public static void Send(object cmd, object content, Action<INetworkMessage> callback, params object[] parameters)
        {
            try
            {
                responseActions.Replace(cmd, callback);
                var message = MessageHandler.PackMessage(cmd, content, callback, parameters);
                var buffer = message.GetBuffer(true, IsLittleEndian);
                connector.Send(buffer);
#if UNITY_EDITOR
                Debug.LogFormat("<color=cyan>[TCPNetwork]</color> [Send] >> CMD:{0},TIME:{1}", cmd, DateTime.Now);
#endif
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

        public static void Close()
        {
            connector.Close();
        }
    }
}