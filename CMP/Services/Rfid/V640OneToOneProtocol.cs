using System.Globalization;
using System.Text;

namespace CMP.Services.Rfid;

/// <summary>
/// Omron V640 1:1 协议（无 SECS、无 SOH/节点号/FCS），帧以 CR 结束。
/// </summary>
public static class V640OneToOneProtocol
{
    public const string SuccessCode = "00";
    public const string FormatErrorCode = "14";
    public const string CommunicationErrorCode = "70";
    public const string NoTagCode = "72";
    public const string WriteProtectCode = "7B";
    public const string IdSystemError1Code = "7E";
    public const string IdSystemError2Code = "7F";

    public static string BuildReadCommand(string pageMask)
    {
        if (string.IsNullOrWhiteSpace(pageMask) || pageMask.Length != 8)
        {
            throw new ArgumentException("页掩码必须是 8 位十六进制字符。", nameof(pageMask));
        }

        return "0100" + pageMask.ToUpperInvariant();
    }

    public static string BuildTestCommand(string hexData = "ABCD")
    {
        if (string.IsNullOrWhiteSpace(hexData) || hexData.Length % 2 != 0)
        {
            throw new ArgumentException("测试数据必须是偶数长度的十六进制字符。", nameof(hexData));
        }

        return "10" + hexData.ToUpperInvariant();
    }

    public static V640Response Parse(string line)
    {
        var trimmed = (line ?? string.Empty).Trim('\0', '\r', '\n', ' ');
        if (trimmed.Length < 2)
        {
            return V640Response.Fail(string.Empty, trimmed);
        }

        var code = trimmed[..2].ToUpperInvariant();
        var payload = trimmed.Length > 2 ? trimmed[2..] : string.Empty;
        return new V640Response(code, payload, code == SuccessCode);
    }

    public static string ParseCarrierId(string payloadHex, int maxBytes)
    {
        var bytes = HexToBytes(payloadHex);
        if (maxBytes > 0 && bytes.Length > maxBytes)
        {
            bytes = bytes[..maxBytes];
        }

        var text = Encoding.ASCII.GetString(bytes);
        return text.Trim('\0', ' ', '\t', '\r', '\n');
    }

    public static byte[] HexToBytes(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return [];
        }

        var compact = hex.Replace(" ", "", StringComparison.Ordinal).Trim();
        if (compact.Length % 2 != 0)
        {
            throw new FormatException("RFID 响应数据长度不是 2 的倍数。");
        }

        var bytes = new byte[compact.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = byte.Parse(compact.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return bytes;
    }

    /// <summary>
    /// 按 V640 页位图生成 8 位十六进制掩码。Page1=末字节 bit2，Page2=bit3，…，Page6=bit7。
    /// </summary>
    public static string BuildPageMask(params int[] pages)
    {
        uint bits = 0;
        foreach (var page in pages)
        {
            if (page is < 1 or > 17)
            {
                throw new ArgumentOutOfRangeException(nameof(pages), page, "页号必须在 1–17。");
            }

            if (page <= 6)
            {
                bits |= 1u << (page + 1);
            }
            else if (page <= 14)
            {
                bits |= 1u << (page - 7 + 8);
            }
            else
            {
                bits |= 1u << (page - 15 + 16);
            }
        }

        return bits.ToString("X8", CultureInfo.InvariantCulture);
    }
}

public readonly record struct V640Response(string Code, string PayloadHex, bool IsSuccess)
{
    public static V640Response Fail(string code, string payload) => new(code, payload, false);
}
