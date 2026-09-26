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

    public static string ParseCarrierId(string rawLine, string payloadHex, int maxBytes, string format = "Ascii")
        => ParseCarrierId(rawLine, payloadHex, new CarrierIdParseOptions
        {
            Format = format,
            ByteLength = maxBytes
        });

    public static string ParseCarrierId(string payloadHex, int maxBytes, string format = "Ascii")
        => ParseCarrierId(payloadHex, payloadHex, maxBytes, format);

    public static string ParseCarrierId(string rawLine, string payloadHex, CarrierIdParseOptions? options)
    {
        options ??= new CarrierIdParseOptions();
        var format = string.IsNullOrWhiteSpace(options.Format) ? "Ascii" : options.Format.Trim();
        var source = string.IsNullOrWhiteSpace(options.Source)
            ? CarrierIdParseOptions.DefaultSource(format)
            : options.Source.Trim();

        if (format.Equals("Raw", StringComparison.OrdinalIgnoreCase)
            || source.Equals("Raw", StringComparison.OrdinalIgnoreCase))
        {
            return ApplyLetterCase((rawLine ?? string.Empty).Trim('\0', '\r', '\n', ' ', '\t'), options.LetterCase);
        }

        var payload = CompactHex(payloadHex);
        var frame = CompactHex(rawLine);
        if (string.IsNullOrEmpty(frame))
        {
            frame = payload;
        }

        var hex = source.Equals("Payload", StringComparison.OrdinalIgnoreCase)
            ? (string.IsNullOrEmpty(payload) && frame.Length > 2 ? frame[2..] : payload)
            : frame;

        hex = SliceHex(hex, options.SkipBytes, options.ByteLength);
        if (options.ReverseBytes)
        {
            hex = ReverseHexBytes(hex);
        }

        var trimNull = options.TrimTrailingNull
                       ?? format.Equals("Ascii", StringComparison.OrdinalIgnoreCase)
                       || format.Equals("HexData", StringComparison.OrdinalIgnoreCase)
                       || format.Equals("Auto", StringComparison.OrdinalIgnoreCase);

        if (format.Equals("UInt32", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = HexToBytes(string.IsNullOrEmpty(hex) ? payload : hex);
            return BytesToUInt32(TakeBytes(bytes, options.ByteLength > 0 ? options.ByteLength : 4));
        }

        if (format.Equals("BytesAscii", StringComparison.OrdinalIgnoreCase)
            || format.Equals("Text", StringComparison.OrdinalIgnoreCase))
        {
            return ApplyLetterCase(DecodeBytes(hex, options.Encoding), options.LetterCase);
        }

        if (format.Equals("Auto", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(hex))
        {
            var text = DecodeBytes(hex, options.Encoding);
            if (IsPrintableAscii(text))
            {
                return ApplyLetterCase(text, options.LetterCase);
            }

            return ApplyLetterCase(trimNull ? TrimTrailing00(hex) : hex, options.LetterCase);
        }

        if (trimNull)
        {
            hex = TrimTrailing00(hex);
        }

        return ApplyLetterCase(hex, options.LetterCase);
    }

    private static string SliceHex(string hex, int skipBytes, int byteLength)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return string.Empty;
        }

        if (skipBytes > 0)
        {
            var skipChars = skipBytes * 2;
            hex = hex.Length > skipChars ? hex[skipChars..] : string.Empty;
        }

        if (byteLength > 0)
        {
            var takeChars = byteLength * 2;
            if (hex.Length > takeChars)
            {
                hex = hex[..takeChars];
            }
        }

        return hex;
    }

    private static string ReverseHexBytes(string hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return string.Empty;
        }

        if (hex.Length % 2 != 0)
        {
            hex = "0" + hex;
        }

        var bytes = HexToBytes(hex);
        Array.Reverse(bytes);
        return Convert.ToHexString(bytes);
    }

    private static string TrimTrailing00(string hex)
    {
        while (hex.Length >= 2 && hex.EndsWith("00", StringComparison.OrdinalIgnoreCase))
        {
            hex = hex[..^2];
        }

        return hex;
    }

    private static string DecodeBytes(string hex, string? encodingName)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return string.Empty;
        }

        var encoding = encodingName != null
                       && encodingName.Equals("UTF8", StringComparison.OrdinalIgnoreCase)
            ? Encoding.UTF8
            : Encoding.ASCII;
        return encoding.GetString(HexToBytes(hex)).Trim('\0', ' ', '\t', '\r', '\n');
    }

    private static string ApplyLetterCase(string text, string? letterCase)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (string.Equals(letterCase, "Lower", StringComparison.OrdinalIgnoreCase))
        {
            return text.ToLowerInvariant();
        }

        if (string.Equals(letterCase, "Keep", StringComparison.OrdinalIgnoreCase))
        {
            return text;
        }

        return text.ToUpperInvariant();
    }

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
