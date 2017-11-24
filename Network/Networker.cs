using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arthas.Network
{
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
        /// 错误回调
        /// </summary>
        private static event Action<string> ConnectErrorEvent;

        public static INetworkMessageHandler MessageHandler { get { return messageHandler; } }

        public static bool IsLittleEndian
        {
            get { return Instance.isLittleEndian; }
            set { Instance.isLittleEndian = value; }
        }

        public static bool IsConnected { get { return connector != null && connector.IsConnected; } }

        private readonly static Queue<INetworkMessage> msgQueue = new Queue<INetworkMessage>();
        private readonly static Dictionary<object, Action<INetworkMessage>> responseActions = new Dictionary<object, Action<INetworkMessage>>();

        private Coroutine timeoutCor, connectCor, heartbeatCor;
        private WaitForSeconds heartbeatWaitFor, timeoutWaiter, connectPollWaiter = new WaitForSeconds(.5f);
        private float currentTime, connectCheckDuration = .1f;
        private static object enterLock = new object();

        [SerializeField]
        private float connectTimeout = 10f, heartbeatInterval = 12f;
        [SerializeField]
        private bool isLittleEndian = true, useSSL = false;

        [SerializeField, HideInInspector]
        private string connectorTypeName, messageHandlerName;
        private static IConnector connector;
        private static INetworkMessageHandler messageHandler;

        /// <summary>
        /// 连接到服务器
        /// </summary>
        protected void ConnectInternal(string ip, int port, IConnector conn, INetworkMessageHandler handler = null)
        {
            if (connector != null && connector.IsConnected)
                connector.Close();
            timeoutWaiter = new WaitForSeconds(connectCheckDuration);
            heartbeatWaitFor = new WaitForSeconds(heartbeatInterval);
            if (!string.IsNullOrEmpty(connectorTypeName))
                connector = (IConnector)Activator.CreateInstance(Type.GetType(connectorTypeName, true, true));
            if (!string.IsNullOrEmpty(messageHandlerName))
                messageHandler = (INetworkMessageHandler)Activator.CreateInstance(Type.GetType(messageHandlerName, true, true));
            if (messageHandler == null)
                messageHandler = handler ?? new DefaultMessageHandler();
            if (connector == null)
                connector = conn ?? new TCPConnector();
            connector.Connect(ip, port);
            timeoutCor = StartCoroutine(TimeoutDetectAsync());
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
            int port,
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

        protected IEnumerator TimeoutDetectAsync()
        {
            while (true) {
                yield return timeoutWaiter;
                if (connector.IsConnected) {
                    OnConnected();
                    StopCoroutine(timeoutCor);
                    yield break;
                }
                if ((currentTime += connectCheckDuration) > connectTimeout) {
                    currentTime = 0;
                    StopCoroutine(timeoutCor);
                    if (ConnectErrorEvent != null) {
                        ConnectErrorEvent("Cannot connect to server , please check your network!");
                    }
                }
            }
        }

        protected IEnumerator HeartbeatDetectAsync()
        {
            while (true) {
                if (connector.IsConnected)
                    //Send(0);
                    yield return heartbeatWaitFor;
            }
        }

        protected IEnumerator ConnectionDetectAsync()
        {
            while (true) {
                yield return connectPollWaiter;
                if (!connector.IsConnected) {
                    OnDisconnected();
                    StopCoroutine(connectCor);
                }
            }
        }

        protected void OnConnected()
        {
            connector.MessageRespondEvent += OnMessageRespond;
            if (ConnectedEvent != null)
                ConnectedEvent();
            connectCor = StartCoroutine(ConnectionDetectAsync());
            heartbeatCor = StartCoroutine(HeartbeatDetectAsync());
        }

        protected void OnDisconnected()
        {
            if (DisconnectedEvent != null)
                DisconnectedEvent();
            connector.MessageRespondEvent -= OnMessageRespond;
            StopCoroutine(heartbeatCor);
        }

        protected void OnMessageRespond(byte[] buffer)
        {
            lock (enterLock) {
                var msgs = messageHandler.ParseMessage(buffer);
                for (var i = 0; i < msgs.Count; i++) {
                    msgQueue.Enqueue(msgs[i]);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogFormat("<color=blue>[TCPNetwork]</color> [Receive] << CMD:{0},TIME:{1}", msgs[i].Command, DateTime.Now);
#endif
                }
            }
        }

        private void Update()
        {
            if (msgQueue.Count > 0) {
                var message = msgQueue.Dequeue();
                if (responseActions.Count > 0
                    && responseActions.ContainsKey(message.Command)) {
                    var action = responseActions[message.Command];
                    if (action != null) {
                        action.Invoke(message);
                        responseActions.Remove(message.Command);
                    }
                } else if (PushEvent != null) {
                    PushEvent(message);
                }
                if (ResponseEvent != null) {
                    ResponseEvent(message);
                }
            }
        }

        public static void Send(object cmd, object buf = null, Action<INetworkMessage> callback = null, params object[] parameters)
        {
            try {
                var message = messageHandler.PackMessage(cmd, buf, parameters);
                var buffer = message.GetBuffer(IsLittleEndian);
                responseActions.Replace(message.Command, callback);
                connector.Send(buffer);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogFormat("<color=cyan>[TCPNetwork]</color> [Send] >> CMD:{0},TIME:{1}", cmd, DateTime.Now);
#endif
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        public static IEnumerator SendAsync(object cmd, object buf, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public static void Close()
        {
            if (connector != null)
                connector.Close();
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause && connector != null && !connector.IsConnected)
                Connect();
        }

        private void OnApplicationQuit()
        {
            Close();
        }
    }
}