using Newtonsoft.Json;
using System.IO;
using System.IO.Ports;

namespace CMP.Services.Rfid;

public class RfidConfig
{
    public const string DefaultPath = @"D:\CMP\Configuration\Configs\RfidConfig.json";

    public bool Enabled { get; set; } = true;
    public int BaudRate { get; set; } = 9600;
    public int DataBits { get; set; } = 8;
    public string Parity { get; set; } = "Even";
    public int StopBits { get; set; } = 1;
    public int ReadTimeoutMs { get; set; } = 3000;
    public string PageMask { get; set; } = "0000000C";
    public int CarrierIdByteLength { get; set; } = 16;
    public bool ProbeOnConnect { get; set; } = true;
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
            config.PageMask = "0000000C";
        }

        config.PageMask = config.PageMask.ToUpperInvariant();
        config.Readers ??= [];
        return config;
    }
}

public class RfidReaderBinding
{
    public string LoadPortCom { get; set; } = string.Empty;
    public int LoadportId { get; set; }
    public string RfidPort { get; set; } = string.Empty;
}
