/*******************************************************
 * Class  : NetworkConfiguration 
 * Date   : 2017/3/30
 * Author : Alvin
 * ******************************************************/

using UnityEngine;
using System;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif


/// <summary>
/// 网络配置
/// </summary>
[CreateAssetMenu(fileName = "NetworkConfiguration", menuName = "Configs/NetworkConfiguration")]
public class NetworkConfiguration : ScriptableObject
{
    public const string kConfigPath = "Configs/";

    /// <summary>
    /// 网络地址
    /// </summary>
    [Serializable]
    public class NetworkAddress
    {
        public string tag;
        public string ip;
        public short port;
        [Space(10), Header("Http: [http[s]]://{ip}:{port}/[Path]")]
        public string url = "http:/{0}:{1}/";
        public Uri Uri { get { return new Uri(string.Format(url, ip, port)); } }

        #region Compare
        public static bool operator ==(NetworkAddress lhs, NetworkAddress rhs) { return lhs.ip == rhs.ip && lhs.port == rhs.port; }
        public static bool operator !=(NetworkAddress lhs, NetworkAddress rhs) { return lhs.ip != rhs.ip || lhs.port != rhs.port; }
        public override bool Equals(object obj) { return base.Equals(obj); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public override string ToString() { return string.Format("{0}:{1}", ip, port); }
        #endregion
    }

    public static NetworkAddress Current
    {
        get
        {
            NetworkConfiguration conf;
            if (loader != null) conf = loader();
            else conf = Resources.Load<NetworkConfiguration>(kConfigPath + "NetworkConfiguration");
            if (!conf) throw new NullReferenceException("Cannot found <color=yellow>[NetworkConfiguration]</color>!");
            if (string.IsNullOrEmpty(conf.current.ip)) conf.current = conf.intranet;
            return conf.current;
        }
    }

    public static void AddLoader(Func<NetworkConfiguration> configLoader) { loader = configLoader; }
    private static Func<NetworkConfiguration> loader;

    [Space(30)]
    [HideInInspector]
    public NetworkAddress current;
    [Header("LOCAL")]
    public NetworkAddress local;
    [Header("LAN")]
    public NetworkAddress intranet;
    [Header("WAN")]
    public NetworkAddress internet;
    [Header("Option1")]
    public NetworkAddress option1;
    [Header("Option2")]
    public NetworkAddress option2;

#if UNITY_EDITOR
    public const string kPath = "Assets/Resources/" + kConfigPath + "NetworkConfiguration.asset";
    public const string kMenu = "Network/Address",
        kLocal = "/LOCAL",
        kIntranet = "/LAN",
        kInternet = "/WAN",
        kOp1 = "/Option1",
        kOp2 = "/Option2";

    [MenuItem(kMenu + kLocal, priority = 1)]
    public static void SetLocal()
    {
        var conf = GetConfiguration();
        conf.current = conf.local;
        EditorUtility.SetDirty(conf);
        Debug.LogFormat("<color=cyan>Current Address:[{0}:{1}]</color>", conf.current.ip, conf.current.port);
    }

    [MenuItem(kMenu + kLocal, true, priority = 1)]
    public static bool ToggleSetLocalValidate()
    {
        var conf = GetConfiguration();
        Menu.SetChecked(kMenu + kLocal, conf.current == conf.local);
        return true;
    }

    [MenuItem(kMenu + kIntranet, priority = 1)]
    public static void SetIntranet()
    {
        var conf = GetConfiguration();
        conf.current = conf.intranet;
        EditorUtility.SetDirty(conf);
        Debug.LogFormat("<color=cyan>Current Address:[{0}:{1}]</color>", conf.current.ip, conf.current.port);
    }

    [MenuItem(kMenu + kIntranet, true, priority = 1)]
    public static bool ToggleSetIntranetValidate()
    {
        var conf = GetConfiguration();
        Menu.SetChecked(kMenu + kIntranet, conf.current == conf.intranet);
        return true;
    }

    [MenuItem(kMenu + kInternet, priority = 1)]
    public static void SetInternet()
    {
        var conf = GetConfiguration();
        conf.current = conf.internet;
        EditorUtility.SetDirty(conf);
        Debug.LogFormat("<color=cyan>Current Address:[{0}:{1}]</color>", conf.current.ip, conf.current.port);
    }

    [MenuItem(kMenu + kInternet, true, priority = 1)]
    public static bool ToggleSetInternetValidate()
    {
        var conf = GetConfiguration();
        Menu.SetChecked(kMenu + kInternet, conf.current == conf.internet);
        return true;
    }


    [MenuItem(kMenu + kOp1, priority = 1)]
    public static void SetOp1()
    {
        var conf = GetConfiguration();
        conf.current = conf.option1;
        EditorUtility.SetDirty(conf);
        Debug.LogFormat("<color=cyan>Current Address:[{0}:{1}]</color>", conf.current.ip, conf.current.port);
    }

    [MenuItem(kMenu + kOp1, true, priority = 1)]
    public static bool ToggleSetOp1Validate()
    {
        var conf = GetConfiguration();
        Menu.SetChecked(kMenu + kOp1, conf.current == conf.option1);
        return true;
    }

    [MenuItem(kMenu + kOp2, priority = 1)]
    public static void SetOp2()
    {
        var conf = GetConfiguration();
        conf.current = conf.option2;
        EditorUtility.SetDirty(conf);
        Debug.LogFormat("<color=cyan>Current Address:[{0}:{1}]</color>", conf.current.ip, conf.current.port);
    }

    [MenuItem(kMenu + kOp2, true, priority = 1)]
    public static bool ToggleSetOp2Validate()
    {
        var conf = GetConfiguration();
        Menu.SetChecked(kMenu + kOp2, conf.current == conf.option2);
        return true;
    }

    [MenuItem("Network/Option", priority = 0)]
    public static void Configure()
    {
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<NetworkConfiguration>(kPath);
    }

    private static NetworkConfiguration GetConfiguration()
    {
        var conf = AssetDatabase.LoadAssetAtPath<NetworkConfiguration>(kPath);
        if (conf == null)
        {
            conf = new NetworkConfiguration()
            {
                local = new NetworkAddress() { ip = "127.0.0.1", port = 10000 },
                intranet = new NetworkAddress() { ip = "192.168.1.10", port = 10000 },
                internet = new NetworkAddress() { ip = "0.0.0.0", port = 10000 }
            };
            var path = Path.Combine(Application.dataPath, "Resources/" + kConfigPath);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            AssetDatabase.CreateAsset(conf, kPath);
            EditorUtility.SetDirty(conf);
            Selection.activeObject = conf;
            EditorUtility.DisplayDialog("Configuration", "Please configure your network  >>>>", "√");
        }
        return conf;
    }
#endif
}
