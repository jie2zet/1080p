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

    /// <summary>
    /// 与串口助手一致：默认把整帧十六进制当 CarrierId（如 0088776655443322110000）。
    /// </summary>
    public static string ParseCarrierId(string rawLine, string payloadHex, int maxBytes, string format = "Hex")
    {
        var mode = string.IsNullOrWhiteSpace(format) ? "Hex" : format.Trim();
        var payload = CompactHex(payloadHex);

        if (mode.Equals("Ascii", StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrEmpty(payload) ? string.Empty : BytesToAscii(TakeBytes(HexToBytes(payload), maxBytes));
        }

        if (mode.Equals("UInt32", StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrEmpty(payload) ? string.Empty : BytesToUInt32(TakeBytes(HexToBytes(payload), maxBytes > 0 ? maxBytes : 4));
        }

        if (mode.Equals("Auto", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(payload))
        {
            var ascii = BytesToAscii(TakeBytes(HexToBytes(payload), maxBytes));
            if (IsPrintableAscii(ascii))
            {
                return ascii;
            }
        }

        var hex = CompactHex(rawLine);
        if (string.IsNullOrEmpty(hex))
        {
            hex = payload;
        }

        if (maxBytes > 0)
        {
            var prefix = hex.StartsWith(SuccessCode, StringComparison.OrdinalIgnoreCase) ? 2 : 0;
            var maxChars = prefix + maxBytes * 2;
            if (hex.Length > maxChars)
            {
                hex = hex[..maxChars];
            }
        }

        return hex.ToUpperInvariant();
    }

    public static string ParseCarrierId(string payloadHex, int maxBytes, string format = "Hex")
        => ParseCarrierId(payloadHex, payloadHex, maxBytes, format);

    private static string CompactHex(string? hex)
    {
        return (hex ?? string.Empty)
            .Replace(" ", "", StringComparison.Ordinal)
            .Trim('\0', '\r', '\n', '\t', ' ')
            .ToUpperInvariant();
    }

    private static byte[] TakeBytes(byte[] bytes, int maxBytes)
    {
        if (maxBytes > 0 && bytes.Length > maxBytes)
        {
            return bytes[..maxBytes];
        }

        return bytes;
    }

    private static string BytesToAscii(byte[] bytes)
    {
        return Encoding.ASCII.GetString(bytes).Trim('\0', ' ', '\t', '\r', '\n');
    }

    private static bool IsPrintableAscii(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        foreach (var ch in text)
        {
            if (ch < 0x20 || ch > 0x7E)
            {
                return false;
            }
        }

        return true;
    }

    private static string BytesToUInt32(byte[] bytes)
    {
        if (bytes.Length < 4)
        {
            return Convert.ToHexString(bytes);
        }

        var value = (uint)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);
        return value.ToString(CultureInfo.InvariantCulture);
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
