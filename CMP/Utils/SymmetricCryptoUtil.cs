using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CMP.Utils;

/// <summary>
/// 对称加密工具类，专为CryptoStream设计
/// </summary>
public sealed class SymmetricCryptoUtil : IDisposable
{
    private readonly Aes _aes;
    private bool _disposed;

    /// <summary>
    /// 使用随机密钥初始化加密工具
    /// </summary>
    public SymmetricCryptoUtil()
    {
        _aes = Aes.Create();
        _aes.Mode = CipherMode.CBC;
        _aes.Padding = PaddingMode.PKCS7;
        _aes.GenerateKey();
        _aes.GenerateIV();
    }

    /// <summary>
    /// 使用指定的密钥和IV初始化加密工具
    /// </summary>
    public SymmetricCryptoUtil(byte[] key, byte[] iv)
    {
        _aes = Aes.Create();
        _aes.Mode = CipherMode.CBC;
        _aes.Padding = PaddingMode.PKCS7;
        _aes.Key = key;
        _aes.IV = iv;
    }

    /// <summary>
    /// 基于密码和盐值创建加密工具
    /// </summary>
    public static SymmetricCryptoUtil CreateFromPassword(string password, byte[] salt, int iterations = 10000)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        byte[] key = deriveBytes.GetBytes(32); // 256位密钥
        byte[] iv = deriveBytes.GetBytes(16);  // 128位IV
            
        return new SymmetricCryptoUtil(key, iv);
    }
        
    /// <summary>
    /// 生成随机盐值
    /// </summary>
    public static byte[] GenerateSalt(int size = 16)
    {
        var salt = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }

    /// <summary>
    /// 当前密钥
    /// </summary>
    public byte[] Key => _aes.Key;

    /// <summary>
    /// 当前向量
    /// </summary>
    public byte[] Iv => _aes.IV;

    /// <summary>
    /// 创建加密转换器，用于CryptoStream
    /// </summary>
    public ICryptoTransform CreateEncryptor()
    {
        CheckDisposed();
        return _aes.CreateEncryptor();
    }

    /// <summary>
    /// 创建解密转换器，用于CryptoStream
    /// </summary>
    public ICryptoTransform CreateDecryptor()
    {
        CheckDisposed();
        return _aes.CreateDecryptor();
    }

    /// <summary>
    /// 加密字符串
    /// </summary>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;
                
        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            
        using var ms = new MemoryStream();
        using (var encryptor = CreateEncryptor())
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(bytes, 0, bytes.Length);
            cs.FlushFinalBlock();
        }
            
        return Convert.ToBase64String(ms.ToArray());
    }
        
    /// <summary>
    /// 解密字符串
    /// </summary>
    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return encryptedText;
                
        byte[] bytes = Convert.FromBase64String(encryptedText);
            
        using var ms = new MemoryStream(bytes);
        using var decryptor = CreateDecryptor();
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
            
        return sr.ReadToEnd();
    }

    /// <summary>
    /// 将数据写入加密流并返回加密后的字节数组
    /// </summary>
    public byte[]? EncryptData(byte[]? data)
    {
        if (data == null || data.Length == 0)
            return data;
                
        using var ms = new MemoryStream();
        using (var encryptor = CreateEncryptor())
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
        }
            
        return ms.ToArray();
    }

    /// <summary>
    /// 解密数据
    /// </summary>
    public byte[]? DecryptData(byte[]? encryptedData)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            return encryptedData;
                
        using var ms = new MemoryStream(encryptedData);
        using var decryptor = CreateDecryptor();
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var output = new MemoryStream();
            
        cs.CopyTo(output);
        return output.ToArray();
    }

    private void CheckDisposed()
    {
        if (!_disposed) return;
        throw new ObjectDisposedException(nameof(SymmetricCryptoUtil));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _aes.Dispose();
        _disposed = true;
    }
}