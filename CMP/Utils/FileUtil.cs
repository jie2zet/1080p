using System.IO;
using System.Security.Cryptography;

namespace CMP.Utils;

public class FileUtil
{
    private static readonly SymmetricCryptoUtil Crypto;
    /// <summary>
    /// 32位密钥
    /// </summary>
    private static readonly byte[] Key = "12345678901234567890123456789012"u8.ToArray();
    /// <summary>
    /// 16位IV
    /// </summary>
    private static readonly byte[] Iv = "1234567890123456"u8.ToArray();

    static FileUtil()
    {
        Crypto = new SymmetricCryptoUtil(Key, Iv);
    }

    /// <summary>
    /// 读取加密文件内容
    /// </summary>
    /// <param name="path"></param>
    /// <param name="needCrypto"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static string ReadFile(string path, bool needCrypto = false)
    {
        // // ===== 从密码创建加密工具 =====
        // string password = "MySecretPassword";
        // byte[] salt = SymmetricCrypto.GenerateSalt();
        // using var passwordCrypto = SymmetricCrypto.CreateFromPassword(password, salt);
        //
        // // 使用密码加密内容
        // using (var inputStream = new FileStream("config.json", FileMode.Open))
        // using (var outputStream = File.Create("config.password.encrypted"))
        // using (var encryptor = passwordCrypto.CreateEncryptor())
        // using (var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
        // {
        //     inputStream.CopyTo(cryptoStream);
        // }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);

        if (needCrypto)
        {
            using var cryptoStream = new CryptoStream(fileStream, Crypto.CreateDecryptor(), CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }
        else
        {
            using var streamReader = new StreamReader(fileStream);
            return streamReader.ReadToEnd();
        }
    }

    /// <summary>
    /// 写入加密文件内容
    /// </summary>
    /// <param name="path"></param>
    /// <param name="content"></param>
    /// <param name="needCrypto"></param>
    public static void WriteFile(string path, string content, bool needCrypto = false)
    {
        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        if (needCrypto)
        {
            using var cryptoStream = new CryptoStream(fileStream, Crypto.CreateEncryptor(), CryptoStreamMode.Write);
            using var streamWriter = new StreamWriter(cryptoStream);
            streamWriter.Write(content);
        }
        else
        {
            using var streamWriter = new StreamWriter(fileStream);
            streamWriter.Write(content);
        }
    }

    public static void CreateDirectory(string path)
    {
        // Check if the directory exists
        if (!Directory.Exists(path))
        {
            // Create the directory
            Directory.CreateDirectory(path);
        }
    }
}