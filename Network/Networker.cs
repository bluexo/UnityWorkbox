using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Arthas.Common;
using System.Threading;
using System.Net;

namespace Arthas.Network
{
    public enum NetworkStatus
    {
        Connecting = 1,            //正在连接
        Connected = 2,             //网络已经连接
        Disconnected = 3,          //网络断开
        NetworkNotReachbility = 4, //无网络
        ConnectTimeout = 5,        //超时
    }

    /// <summary>
    /// 网络
    /// </summary>
    public class Networker : SingletonBehaviour<Networker>
    {

        /// <summary>
        /// 推送和响应事件
        /// </summary>
        public static event Action<INetworkMessage> ResponseEvent, PushEvent;

        /// <summary>
        /// 网络连接事件
        /// </summary>
        public static event Action ConnectedEvent;

        /// <summary>
        /// 网络断开事件
        /// </summary>
        public static event Action DisconnectedEvent;

        /// <summary>
        /// 网络状态事件
        /// </summary>
        public static event Action<NetworkStatus> NetworkStatusEvent;

        /// <summary>
        /// 错误回调
        /// </summary>
        public static event Action<string> ConnectErrorEvent;

        public static INetworkMessageHandler MessageHandler { get { return messageHandler; } }
        public static IConnector CurrentConnector { get { return connector; } }
        public static NetworkStatus NetworkStatus { get; private set; }
        public static Func<object> HeartbeatCommandGetter { get; set; }

        public static ByteBuf buf = new ByteBuf(1024);

        public static bool IsLittleEndian
        {
            get { return Instance.isLittleEndian; }
            set { Instance.isLittleEndian = value; }
        }
        public static bool IsConnected { get { return connector != null && connector.IsConnected; } }
        public static long DelayTime { get; private set; }

        private readonly static Queue<INetworkMessage> msgQueue = new Queue<INetworkMessage>();
        private readonly static Dictionary<object, Queue<Action<INetworkMessage>>> responseActions = new Dictionary<object, Queue<Action<INetworkMessage>>>();

        private Coroutine timeoutCor, connectCor, heartbeatCor;
        private WaitForSeconds heartbeatWaitFor, timeoutWaiter, connectPollWaiter = new WaitForSeconds(.5f);
        private float prevConnectTime, connectCheckDuration = .1f;
        private static object enterLock = new object();
        private bool isPaused = false;
        private int retryCount = 0;

        [SerializeField]
        private float connectTimeout = 10f, heartbeatInterval = 12f;
        [SerializeField]
        private bool isLittleEndian = true, useSSL = false;
        [SerializeField]
        private int maxRetryCount = 3;

        [SerializeField, HideInInspector]
        private string connectorTypeName, messageHandlerName;
        private static IConnector connector;
        private static INetworkMessageHandler messageHandler;
        public NetworkAddress NetworkAddress { get; private set; }
        private static float prevSendTime = 0;
        private static bool isConnecting = false;

        protected override void Awake()
        {
            base.Awake();
            timeoutWaiter = new WaitForSeconds(connectCheckDuration);
            heartbeatWaitFor = new WaitForSeconds(heartbeatInterval);
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        protected void ConnectInternal(string ip, short port, IConnector conn, INetworkMessageHandler handler = null)
        {
            if (connector != null && connector.IsConnected)  connector.Close();
            if (!string.IsNullOrEmpty(connectorTypeName))
                connector = (IConnector)Activator.CreateInstance(Type.GetType(connectorTypeName, true, true));
            if (!string.IsNullOrEmpty(messageHandlerName))
                messageHandler = (INetworkMessageHandler)Activator.CreateInstance(Type.GetType(messageHandlerName, true, true));
            if (messageHandler == null)
                messageHandler = handler ?? new DefaultMessageHandler();
            if (connector == null)
                connector = conn ?? new TCPConnector();
            NetworkAddress = new NetworkAddress { ip = ip, port = port };
            Connect();
#if UNITY_EDITOR
            Debug.LogFormat("Connect to server , <color=cyan>Addr:[{0}:{1}] ,connector:{2} ,wrapper:{3}.</color>", ip, port, connector.GetType(), messageHandler.GetType());
#endif
        }

        /// <summary>
        /// 根据网络配置连接到服务器
        /// </summary>
        /// <param name="configuration"></param>
        public static void Connect(Action callback = null,
           Action<string> error = null,
           IConnector conn = null,
           INetworkMessageHandler handler = null)
        {
            Connect(NetworkConfiguration.Current.ip,
                NetworkConfiguration.Current.port,
                callback,
                error,
                conn,
                handler);
        }

        /// <summary>
        /// 根据IP连接到服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="callback"></param>
        /// <param name="error"></param>
        /// <param name="handler"></param>
        public static void Connect(string ip,
            short port,
            Action callback = null,
            Action<string> error = null,
            IConnector connector = null,
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
            ConnectErrorEvent = error;
            ConnectedEvent = callback;
            Instance.ConnectInternal(ip, port, connector, handler);
        }

        protected void Connect()
        {
            if (isConnecting) return;
            isConnecting = true;
            prevConnectTime = Time.time;
            buf = new ByteBuf(1024);
            if (connector != null) connector.Connect(NetworkAddress.ip, NetworkAddress.port);
            InvokeStatusEvent(NetworkStatus.Connecting);
            timeoutCor = StartCoroutine(TimeoutDetectAsync());
        }

        protected IEnumerator TimeoutDetectAsync()
        {
            while (true)
            {
                yield return timeoutWaiter;
                if (connector.IsConnected)
                {
                    OnConnected();
                    StopCoroutine(timeoutCor);
                    yield break;
                }
                if (Time.time - prevConnectTime > connectTimeout)
                {
                    prevConnectTime = 0;
                    OnDisconnected();
                    StopCoroutine(timeoutCor);
                    if (ConnectErrorEvent != null) ConnectErrorEvent("Cannot connect to server , please check your network!");
                }
            }
        }

        private void InvokeStatusEvent(NetworkStatus status)
        {
            NetworkStatus = status;
            if (NetworkStatusEvent != null) NetworkStatusEvent(NetworkStatus);
        }

        protected IEnumerator HeartbeatDetectAsync()
        {
            while (true)
            {
                yield return heartbeatWaitFor;
                if (connector.IsConnected)
                {
                    if (HeartbeatCommandGetter != null) Send(HeartbeatCommandGetter());
                    else Send(0);
                }
                else yield break;
            }
        }

        protected IEnumerator ConnectionDetectAsync()
        {
            while (true)
            {
                yield return connectPollWaiter;
                if (Application.internetReachability == NetworkReachability.NotReachable
                    || !connector.IsConnected)
                {
                    OnDisconnected();
                    StopCoroutine(connectCor);
                    var status = Application.internetReachability == NetworkReachability.NotReachable
                        ? NetworkStatus.NetworkNotReachbility
                        : NetworkStatus.Disconnected;
                    InvokeStatusEvent(status);
                }
            }
        }

        protected void OnConnected()
        {
            retryCount = 0;
            isConnecting = false;
            if (connector != null) connector.MessageRespondEvent += OnMessageRespond;
            if (ConnectedEvent != null) ConnectedEvent();
            InvokeStatusEvent(NetworkStatus.Connected);
            connectCor = StartCoroutine(ConnectionDetectAsync());
            heartbeatCor = StartCoroutine(HeartbeatDetectAsync());
        }

        protected void OnDisconnected(bool retry = true)
        {
            connector.MessageRespondEvent -= OnMessageRespond;
            isConnecting = false;
            if (connector.IsConnected) return;
            if (retryCount == maxRetryCount)
            {
                InvokeStatusEvent(NetworkStatus.Disconnected);
                if (DisconnectedEvent != null) DisconnectedEvent();
                retryCount = 0;
            }
            else
            {
                retryCount++;
                StartCoroutine(RetryConnect());
            }
        }

        protected IEnumerator RetryConnect()
        {
            yield return new WaitForSeconds(1f);
            Connect();
        }

        protected void OnMessageRespond()
        {
            if (isPaused) return;
            lock (enterLock)
            {
                var msgs = messageHandler.ParseMessage(buf);
                for (var i = 0; i < msgs.Count; i++)
                {
                    msgQueue.Enqueue(msgs[i]);
                    if (msgQueue.Count > byte.MaxValue) msgQueue.Dequeue();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogFormat("<color=blue>[TCPNetwork]</color> [Receive] << CMD:{0},TIME:{1}", msgs[i].Command, DateTime.Now);
#endif
                }
            }
        }

        private void Update()
        {
            while (msgQueue.Count > 0)
            {
                var message = msgQueue.Dequeue();
                if (responseActions.Count > 0 && responseActions.ContainsKey(message.Command))
                {
                    var action = responseActions[message.Command].Dequeue();
                    if (action != null) action.Invoke(message);
                }
                else if (PushEvent != null)
                {
                    PushEvent(message);
                }
                if (ResponseEvent != null)
                {
                    ResponseEvent(message);
                }
            }
        }

        public static void Send(object cmd, object buf = null, Action<INetworkMessage> callback = null, params object[] parameters)
        {
            if (!connector.IsConnected) instance.Connect();
            try
            {
                var message = messageHandler.PackMessage(cmd, buf, parameters);
                var buffer = message.GetBuffer(IsLittleEndian);
                if (!responseActions.ContainsKey(message.Command))
                    responseActions.Add(message.Command, new Queue<Action<INetworkMessage>>());
                responseActions[message.Command].Enqueue(callback);
                connector.Send(buffer);
                prevSendTime = Time.time;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogFormat("<color=cyan>[TCPNetwork]</color> [Send] >> CMD:{0},TIME:{1}", cmd, DateTime.Now);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static IEnumerator SendAsync(object cmd, object buf, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public static void Close()
        {
            if (connector != null) connector.Close();
        }

        void OnApplicationFocus(bool hasFocus)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE
            isPaused = !hasFocus;
#endif
        }

        void OnApplicationPause(bool pause)
        {
            isPaused = pause;
            //if (!pause && connector != null && !connector.IsConnected)
            //    Connect();
        }

        private void OnApplicationQuit()
        {
            Close();
        }
    }
}