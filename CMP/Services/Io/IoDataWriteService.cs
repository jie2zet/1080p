using CMP.Models.Resource;
using KHT = CMP.Models.kvsPlc.KvHostLink;
using KHSTIO = CMP.Models.kvsPlc.IO.KvStruct;
using KHSTRollerIO = CMP.Models.kvsPlc.Roll.KvsStructRollerIOData;
using KHSTPenIO = CMP.Models.kvsPlc.Pen.KvsStructPenIOData;
using KHSTDryIO = CMP.Models.kvsPlc.Dry.KvsStructDryIOData;
using static CMP.Models.kvsPlc.KvHostLink;

namespace CMP.Services.Io;

/// <summary>
/// Writes one boolean IO field without changing the collecting model's cache or ListSV.
/// The next normal PLC collection is the source of truth for the displayed value.
/// </summary>
public sealed class IoDataWriteService
{
    // These are the exact variable names used by CollectData's KHLReadVariable calls.
    // Keep them explicit: an IO write must never invent a PLC handle from a chamber name.
    private const string PolishIoVariable = "structIOData";
    private const string PenIoVariable = "IOData";
    private const string DryIoVariable = "IOData";
    private static readonly string[] RollerIoVariables = [string.Empty, "IOData1", "IOData2", "IOData3"];

    public int Write(CollectDataChamber chamber, string stationPrefix, string fieldName, bool value)
    {
        ArgumentNullException.ThrowIfNull(chamber);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        lock (chamber.IoDataLock)
        {
            return chamber switch
            {
                CMP.Models.Resource.Polish polish => WritePolish(polish, fieldName, value),
                CMP.Models.Resource.Roller roller => WriteRoller(roller, stationPrefix, fieldName, value),
                CMP.Models.Resource.Pen pen => WritePen(pen, stationPrefix, fieldName, value),
                CMP.Models.Resource.Dry dry => WriteDry(dry, fieldName, value),
                _ => -1
            };
        }
    }

    private static int WritePolish(CMP.Models.Resource.Polish chamber, string fieldName, bool value)
    {
        var candidate = Clone(chamber.IoData, KHSTIO.stPolishIOData_ToByte, KHSTIO.ByteTo_stPolishIOData);
        if (!SetBool(candidate, fieldName, value)) return -1;
        var bytes = Serialize(candidate, KHSTIO.stPolishIOData_ToByte);
        return KHT.KHLWriteVariable(chamber.kvLink.sock, PolishIoVariable, bytes, true);
    }

    private static int WriteRoller(CMP.Models.Resource.Roller chamber, string stationPrefix, string fieldName, bool value)
    {
        if (!int.TryParse(stationPrefix.TrimStart('_'), out var station) || station is < 1 or > 3) return -1;
        var candidate = Clone(chamber.GetIoData(station), KHSTRollerIO.stRollerIOData_ToByte, KHSTRollerIO.ByteTo_stRollerIOData);
        if (!SetBool(candidate, fieldName, value)) return -1;
        var bytes = Serialize(candidate, KHSTRollerIO.stRollerIOData_ToByte);
        return KHT.KHLWriteVariable(chamber.kvLink.sock, RollerIoVariables[station], bytes, true);
    }

    private static int WritePen(CMP.Models.Resource.Pen chamber, string stationPrefix, string fieldName, bool value)
    {
        if (stationPrefix != "_1") return -1;
        var candidate = Clone(chamber.IoData, KHSTPenIO.stPenIOData_ToByte, KHSTPenIO.ByteTo_stPenIOData);
        if (!SetBool(candidate, fieldName.TrimStart('_'), value)) return -1;
        var bytes = Serialize(candidate, KHSTPenIO.stPenIOData_ToByte);
        return KHT.KHLWriteVariable(chamber.kvLink.sock, PenIoVariable, bytes, true);
    }

    private static int WriteDry(CMP.Models.Resource.Dry chamber, string fieldName, bool value)
    {
        var candidate = Clone(chamber.IoData, KHSTDryIO.stDryIOData_ToByte, KHSTDryIO.ByteTo_stDryIOData);
        if (!SetBool(candidate, fieldName, value)) return -1;
        var bytes = Serialize(candidate, KHSTDryIO.stDryIOData_ToByte);
        return KHT.KHLWriteVariable(chamber.kvLink.sock, DryIoVariable, bytes, true);
    }

    private delegate void ToByte<T>(ref byte[] bytes, T value, int len = 1, int wordOffset = 0);
    private delegate void FromByte<T>(ref T value, byte[] bytes, int len = 1, int wordOffset = 0);

    private static T Clone<T>(T source, ToByte<T> toByte, FromByte<T> fromByte) where T : new()
    {
        var bytes = Serialize(source, toByte);
        var clone = new T();
        fromByte(ref clone, bytes);
        return clone;
    }

    private static byte[] Serialize<T>(T value, ToByte<T> toByte)
    {
        var bytes = new byte[2048];
        toByte(ref bytes, value);
        return bytes;
    }

    private static bool SetBool<T>(T target, string fieldName, bool value)
    {
        var field = typeof(T).GetField(fieldName)
            ?? typeof(T).GetFields().FirstOrDefault(f =>
                string.Equals(Normalize(f.Name), Normalize(fieldName), StringComparison.Ordinal));
        if (field?.FieldType != typeof(bool)
            || field.Name.StartsWith("Void", StringComparison.Ordinal)
            || field.Name.StartsWith("Member", StringComparison.Ordinal))
        {
            return false;
        }

        field.SetValue(target, value);
        return true;
    }

    private static string Normalize(string value) =>
        string.Concat(value.Where(char.IsLetterOrDigit)).ToUpperInvariant();
}
