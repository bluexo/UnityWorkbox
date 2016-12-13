using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using Arthas.Protocol;
using System;

public static class PlayerLocalData
{
    public const string CryptoKey = "de6cf71e-62b2-41ff-b234-981799aa752d";

    public static PlayerRoleInfo PlayerInfo
    {
        get
        {
            var json = PlayerPrefs.GetString("PlayerInfo");
            var info = JsonConvert.DeserializeObject<PlayerRoleInfo>(json);
            return info;
        }
        set
        {
            var json = JsonConvert.SerializeObject(value);
            PlayerPrefs.SetString("PlayerInfo", json);
        }
    }

    /// <summary>
    /// 玩家资料是否提交成功
    /// </summary>
    public static bool IsProfileSubmited
    {
        get
        {
            return PlayerPrefs.HasKey("IsProfileSubmited") && (PlayerPrefs.GetInt("IsProfileSubmited") > 0);
        }
        set
        {
            PlayerPrefs.SetInt("IsProfileSubmited", IsProfileSubmited ? 1 : -1);
        }
    }

    public static int UserId
    {
        get { return PlayerPrefs.GetInt("UserId"); }
        set { PlayerPrefs.SetInt("UserId", value); }
    }

    /// <summary>
    /// 玩家标记
    /// </summary>
    public static string Token
    {
        get
        {
            return PlayerPrefs.HasKey("Token") ? PlayerPrefs.GetString("Token") : string.Empty;

        }
        set
        {
            PlayerPrefs.SetString("Token", value);
        }
    }
}
