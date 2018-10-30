using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

namespace Arthas.Common
{

    public interface INetworkAddressSelector
    {
        NetworkAddress Current { get; }

        IEnumerator ReloadAsync();
    }


    public class NetworkAddressSelector : MonoBehaviour, INetworkAddressSelector
    {
        public class Config
        {
            public string assetbundle_url;
            public List<NetworkAddress> servers = new List<NetworkAddress>();
        }

        [SerializeField]
        private string networkAddressConfigUrl = "";
        [SerializeField]
        private float timeoutThreshold = 1;

        private NetworkAddress current;
        public NetworkAddress Current
        {
            get
            {
                if (current == null)
                    current = NetworkConfiguration.Current;
                return current;
            }
        }

        // Use this for initialization
        private IEnumerator Start()
        {
            yield return ReloadAsync();
        }

        public IEnumerator ReloadAsync()
        {
            var request = UnityWebRequest.Get(networkAddressConfigUrl);
            yield return request.SendWebRequest();
            var conf = JsonUtility.FromJson<Config>(request.downloadHandler.text);
            var sorted = new SortedList<float, NetworkAddress>();
            foreach (var address in conf.servers)
            {
                using (var tcpClient = new TcpClient())
                {
                    var now = Time.time;
                    tcpClient.BeginConnect(address.ip, address.port, null, null);
                    yield return new WaitUntil(() => tcpClient.Connected || (Time.time - now > timeoutThreshold));
                    if (!tcpClient.Connected) continue;
                    sorted.Add(Time.time - now, address);
                }
            }
            current = sorted.Values.Last();
        }
    }
}
