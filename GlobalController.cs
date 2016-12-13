using System;
using UnityEngine;
using System.Collections.Generic;
using Arthas.Network;
using Arthas.Protocol;
using Arthas.Client.UI;
using Arthas.Client;

public class GlobalController : MonoBehaviour
{
    public static GlobalController Instance { get; private set; }
    public static int UserId { get; private set; }
    public static PlayerRoleInfo PlayerInfo { get; set; }

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OnApplicationPause(bool pause)
    {
        if (pause)
        {

        }
        else
        {

        }
    }

    /// <summary>
    /// 注册账号，如果注册失败，则获取账号资料
    /// </summary>
    public static void Register(Action onComplete)
    {
        var register = new UserRegisterInfo()
        {
            DeviceName = SystemInfo.deviceName,
#if UNITY_STANDALONE || UNITY_EDITOR 
            DeviceId = Guid.NewGuid().ToString(),
#else
            DeviceId = SystemInfo.deviceUniqueIdentifier,
#endif
            DeviceModel = SystemInfo.deviceModel,
            DeviceType = SystemInfo.deviceType.ToString(),
            OS = SystemInfo.operatingSystem,
            ProcessorCount = SystemInfo.processorCount,
            ProcessorFrequency = SystemInfo.processorFrequency,
            ProcessorType = SystemInfo.processorType,
            SystemMemerySize = SystemInfo.systemMemorySize,
            NickName = "Alvin",
            Language = "CN",
        };

        TCPNetwork.Send(CommandType.Register, register, r =>
        {
            var passport = r.GetValue<Passport>();
            PlayerLocalData.UserId = UserId = (int)passport.UserId;
            PlayerLocalData.Token = passport.Token.ToString();
            GetProfile(onComplete);
        });
    }

    public static void QueueOperation(bool enter, MatchQueueType type = MatchQueueType.Two, Action OnComplete = null)
    {
        var param = new { QueueType = type, Enter = enter, };
        TCPNetwork.Send(CommandType.QueueOperation, param, r =>
        {
            if (OnComplete != null)
                OnComplete();
        });
    }

    public static void GetProfile(Action onComplete)
    {
        TCPNetwork.Send(CommandType.PlayerProfile, r =>
        {
            PlayerInfo = r.GetValue<PlayerRoleInfo>();
            if (onComplete != null)
                onComplete();
        });
    }
}
