/*******************************************************
 * Class  : NetworkConfiguration 
 * Date   : 2017/3/30
 * Author : Alvin
 * ******************************************************/

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// 网络配置
/// </summary>
[CreateAssetMenu(fileName = "NetworkConfiguration", menuName = "CreateConfig/NetworkConfiguration")]
public class NetworkConfiguration : ScriptableObject
{
    /// <summary>
    /// 网络地址
    /// </summary>
    [System.Serializable]
    public struct NetworkAddress
    {
        public string ip;
        public short port;

        public static bool operator ==(NetworkAddress lhs, NetworkAddress rhs) { return lhs.ip == rhs.ip && lhs.port == rhs.port; }
        public static bool operator !=(NetworkAddress lhs, NetworkAddress rhs) { return lhs.ip != rhs.ip || lhs.port != rhs.port; }
    }

    public static NetworkAddress Current
    {
        get
        {
            var conf = Resources.Load<NetworkConfiguration>("NetworkConfiguration");
            if (string.IsNullOrEmpty(conf.current.ip)) conf.current = conf.intranet;
            return conf.current;
        }
    }

    [SerializeField, HideInInspector]
    private NetworkAddress current;
    [SerializeField, Header("本机服务器")]
    private NetworkAddress local;
    [SerializeField, Header("内网服务器")]
    private NetworkAddress intranet;
    [SerializeField, Header("外网服务器")]
    private NetworkAddress internet;

#if UNITY_EDITOR
    public const string kPath = "Assets/Resources/NetworkConfiguration.asset";
    public const string kMenu = "Network/切换网络", kLocal = "/localhost", kIntranet = "/内网", kInternet = "/外网";

    [MenuItem(kMenu + kLocal)]
    public static void SetLocal()
    {
        var conf = AssetDatabase.LoadAssetAtPath<NetworkConfiguration>(kPath);
        conf.current = conf.local;
        EditorUtility.SetDirty(conf);
        Debug.LogFormat("<color=cyan>当前服务器地址:[{0}:{1}]</color>", conf.current.ip, conf.current.port);
    }

    [MenuItem(kMenu + kLocal, true)]
    public static bool ToggleSetLocalValidate()
    {
        var conf = AssetDatabase.LoadAssetAtPath<NetworkConfiguration>(kPath);
        Menu.SetChecked(kMenu + kLocal, conf.current == conf.local);
        return true;
    }

    [MenuItem(kMenu + kIntranet)]
    public static void SetIntranet()
    {
        var conf = AssetDatabase.LoadAssetAtPath<NetworkConfiguration>(kPath);
        conf.current = conf.intranet;
        EditorUtility.SetDirty(conf);
        Debug.LogFormat("<color=cyan>当前服务器地址:[{0}:{1}]</color>", conf.current.ip, conf.current.port);
    }

    [MenuItem(kMenu + kIntranet, true)]
    public static bool ToggleSetIntranetValidate()
    {
        var conf = AssetDatabase.LoadAssetAtPath<NetworkConfiguration>(kPath);
        Menu.SetChecked(kMenu + kIntranet, conf.current == conf.intranet);
        return true;
    }

    [MenuItem(kMenu + kInternet)]
    public static void SetInternet()
    {
        var conf = AssetDatabase.LoadAssetAtPath<NetworkConfiguration>(kPath);
        conf.current = conf.internet;
        EditorUtility.SetDirty(conf);
        Debug.LogFormat("<color=cyan>当前服务器地址:[{0}:{1}]</color>", conf.current.ip, conf.current.port);
    }

    [MenuItem(kMenu + kInternet, true)]
    public static bool ToggleSetInternetValidate()
    {
        var conf = AssetDatabase.LoadAssetAtPath<NetworkConfiguration>(kPath);
        Menu.SetChecked(kMenu + kInternet, conf.current == conf.internet);
        return true;
    }

    [MenuItem("Network/配置网络")]
    public static void Configure()
    {
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<NetworkConfiguration>(kPath);
    }

    public static NetworkConfiguration GetConfiguration()
    {
        var conf = AssetDatabase.LoadAssetAtPath<NetworkConfiguration>(kPath);
        if (conf == null)
        {
            conf = new NetworkConfiguration() { local = new NetworkAddress() { ip = "127.0.0.1" , port = 19001 } };
            AssetDatabase.CreateAsset(conf, kPath);
            EditorUtility.SetDirty(conf);
            Selection.activeObject = conf;
            EditorUtility.DisplayDialog("Configuration", "Please configure your network", "√");
        }
        return conf;
    }
#endif
}
