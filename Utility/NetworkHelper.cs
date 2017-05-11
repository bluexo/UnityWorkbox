using UnityEngine;
using System.Net;
using System.Text;
using System.Collections;

public static class NetworkHelper
{
    /// <summary>
    /// 将IP地址字符串转换为整数
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static long IpToInt(string ip)
    {
        char[] separator = new char[] { '.' };
        string[] items = ip.Split(separator);
        return long.Parse(items[0]) << 24
                | long.Parse(items[1]) << 16
                | long.Parse(items[2]) << 8
                | long.Parse(items[3]);
    }

    /// <summary>
    /// 将整数IP地址转换为字符串
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static string IntToIp(long ipInt)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append((ipInt >> 24) & 0xFF).Append(".");
        sb.Append((ipInt >> 16) & 0xFF).Append(".");
        sb.Append((ipInt >> 8) & 0xFF).Append(".");
        sb.Append(ipInt & 0xFF);
        return sb.ToString();
    }
}
