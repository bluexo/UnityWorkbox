using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// 加密和解密
/// </summary>
public static class CryptoHelper
{
    private static TripleDES provider = TripleDES.Create();
    private static ICryptoTransform encryptor, decryptor;
    private const int BUFFER_SIZE = 1024;

    static CryptoHelper()
    {
        provider.Key = new byte[24] { 130, 67, 152, 103, 71, 172, 78, 209, 255, 134, 158, 88, 204, 34, 51, 19, 255, 225, 16, 172, 242, 5, 250, 58 };
        provider.IV = new byte[8] { 19, 87, 06, 20, 19, 87, 06, 05 };
        encryptor = provider.CreateEncryptor();
        decryptor = provider.CreateDecryptor();
    }

    public static string Encrypt(string source, byte[] key = null, byte[] iv = null)
    {
        if (key != null) provider.Key = key;
        if (iv != null) provider.IV = iv;
        var orginBuffer = Encoding.UTF8.GetBytes(source);
        var stream = new MemoryStream();
        var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(orginBuffer, 0, orginBuffer.Length);
        cryptoStream.FlushFinalBlock();
        return Convert.ToBase64String(stream.ToArray());
    }

    public static string Decrypt(string source, byte[] key = null, byte[] iv = null)
    {
        if (key != null) provider.Key = key;
        if (iv != null) provider.IV = iv;
        var encryptedStream = new MemoryStream(Convert.FromBase64String(source));
        var cryptoStream = new CryptoStream(encryptedStream, decryptor, CryptoStreamMode.Read);
        var readBytes = 1;
        var buffer = new byte[BUFFER_SIZE];
        var orginStream = new MemoryStream();
        while (readBytes > 0)
        {
            readBytes = cryptoStream.Read(buffer, 0, BUFFER_SIZE);
            orginStream.Write(buffer, 0, readBytes);
        }
        orginStream.Flush();
        return Encoding.UTF8.GetString(orginStream.ToArray());
    }
}