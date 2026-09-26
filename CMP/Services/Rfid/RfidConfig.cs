using Newtonsoft.Json;
using System.IO;
using System.IO.Ports;

namespace CMP.Services.Rfid;

public class RfidConfig
{
    public const string DefaultPath = @"D:\CMP\Configuration\RFID\RfidConfig.json";

    public bool Enabled { get; set; } = true;
    public int BaudRate { get; set; } = 9600;
    public int DataBits { get; set; } = 8;
    public string Parity { get; set; } = "Even";
    public int StopBits { get; set; } = 1;
    public int ReadTimeoutMs { get; set; } = 3000;
    public string PageMask { get; set; } = "00000014";
    /// <summary>与串口助手一致的读命令（默认 010000000014）。非空时优先于 PageMask。</summary>
    public string ReadCommand { get; set; } = "010000000014";
    /// <summary>发送时是否追加 LF。串口助手为 CR+LF。</summary>
    public bool AppendLf { get; set; } = true;
    /// <summary>截取长度，0 表示不截。未写 CarrierIdParse.ByteLength 时使用此项。</summary>
    public int CarrierIdByteLength { get; set; } = 0;
    /// <summary>快捷格式。未写 CarrierIdParse.Format 时使用。Ascii / Hex / HexData / BytesAscii / Raw / UInt32 / Auto。</summary>
    public string CarrierIdFormat { get; set; } = "Ascii";
    /// <summary>字符解析细则。与 CarrierIdFormat 同时存在时，以本对象非空字段为准。</summary>
    public CarrierIdParseOptions CarrierIdParse { get; set; } = new();
    public bool ProbeOnConnect { get; set; } = false;
    public List<RfidReaderBinding> Readers { get; set; } = [];

    public Parity SerialParity =>
        Enum.TryParse<Parity>(Parity, true, out var parity) ? parity : System.IO.Ports.Parity.Even;

    public StopBits SerialStopBits => StopBits switch
    {
        2 => System.IO.Ports.StopBits.Two,
        0 => System.IO.Ports.StopBits.None,
        _ => System.IO.Ports.StopBits.One
    };

    public static RfidConfig Load(string? path = null)
    {
        var file = path ?? DefaultPath;
        if (!File.Exists(file))
        {
            throw new FileNotFoundException($"RFID 配置文件不存在: {file}", file);
        }

        var json = File.ReadAllText(file);
        var config = JsonConvert.DeserializeObject<RfidConfig>(json)
                     ?? throw new InvalidDataException($"RFID 配置无法解析: {file}");

        if (string.IsNullOrWhiteSpace(config.PageMask) || config.PageMask.Length != 8)
        {
            config.PageMask = "00000014";
        }

        config.PageMask = config.PageMask.ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(config.ReadCommand))
        {
            config.ReadCommand = "010000000014";
        }

        config.ReadCommand = config.ReadCommand.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(config.CarrierIdFormat))
        {
            config.CarrierIdFormat = "Ascii";
        }

        config.CarrierIdParse ??= new CarrierIdParseOptions();
        config.Readers ??= [];
        foreach (var reader in config.Readers)
        {
            reader.CarrierIdParse ??= null;
        }

        return config;
    }

    public CarrierIdParseOptions ResolveParseOptions(RfidReaderBinding? binding = null)
    {
        var overlay = binding?.CarrierIdParse;
        var parse = CarrierIdParse ?? new CarrierIdParseOptions();
        var format = FirstText(overlay?.Format, parse.Format, CarrierIdFormat, "Ascii");
        return new CarrierIdParseOptions
        {
            Format = format,
            Source = FirstText(overlay?.Source, parse.Source, CarrierIdParseOptions.DefaultSource(format)),
            SkipBytes = overlay?.SkipBytes ?? parse.SkipBytes,
            ByteLength = FirstPositive(overlay?.ByteLength, parse.ByteLength, CarrierIdByteLength),
            TrimTrailingNull = overlay?.TrimTrailingNull ?? parse.TrimTrailingNull,
            ReverseBytes = overlay?.ReverseBytes ?? parse.ReverseBytes,
            LetterCase = FirstText(overlay?.LetterCase, parse.LetterCase, "Upper"),
            Encoding = FirstText(overlay?.Encoding, parse.Encoding, "ASCII")
        };
    }

    private static string FirstText(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    private static int FirstPositive(params int?[] values)
    {
        foreach (var value in values)
        {
            if (value is > 0)
            {
                return value.Value;
            }
        }

        return 0;
    }
}

/// <summary>
/// RFID CarrierId 字符解析。对回包 0088776655443322110000000000000000：
/// Ascii=008877665544332211；Hex=整帧含尾00；HexData=8877665544332211；
/// BytesAscii=按字节转字符；Raw=串口原样；UInt32=前4字节十进制；Auto=可打印则 BytesAscii 否则 HexData。
/// </summary>
public class CarrierIdParseOptions
{
    public string Format { get; set; } = "Ascii";
    /// <summary>Frame=整帧；Payload=去掉状态码后的数据区；Raw=串口原文。</summary>
    public string Source { get; set; } = "Frame";
    /// <summary>从所选数据源开头跳过的字节数。</summary>
    public int SkipBytes { get; set; }
    /// <summary>截取字节数，0 表示不截。</summary>
    public int ByteLength { get; set; }
    /// <summary>是否去掉尾部 00。不写则 Ascii/HexData/Auto 为 true，其余为 false。</summary>
    public bool? TrimTrailingNull { get; set; }
    public bool ReverseBytes { get; set; }
    /// <summary>Upper / Lower / Keep。</summary>
    public string LetterCase { get; set; } = "Upper";
    /// <summary>BytesAscii / Auto 使用的编码：ASCII 或 UTF8。</summary>
    public string Encoding { get; set; } = "ASCII";

    public static string DefaultSource(string format)
    {
        if (format.Equals("Raw", StringComparison.OrdinalIgnoreCase))
        {
            return "Raw";
        }

        if (format.Equals("HexData", StringComparison.OrdinalIgnoreCase)
            || format.Equals("BytesAscii", StringComparison.OrdinalIgnoreCase)
            || format.Equals("UInt32", StringComparison.OrdinalIgnoreCase))
        {
            return "Payload";
        }

        return "Frame";
    }
}

public class RfidReaderBinding
{
    public string LoadPortCom { get; set; } = string.Empty;
    public int LoadportId { get; set; }
    public string RfidPort { get; set; } = string.Empty;
    /// <summary>该读头覆盖全局 CarrierIdParse，不写则用全局。</summary>
    public CarrierIdParseOptions? CarrierIdParse { get; set; }
}
