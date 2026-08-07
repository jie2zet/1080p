using System.Threading;
using CMP.Services.Plc;

namespace CMP.Utils;

/// <summary>
/// 三菱 PLC 位脉冲写入（先 true 再延时再 false），避免在 UI 线程使用 <see cref="Thread.Sleep"/>。
/// </summary>
public static class PlcPulse
{
    public static async Task BoolWritePulseAsync(
        MitsubishiPlcService plc,
        string address,
        int holdMilliseconds,
        CancellationToken cancellationToken = default)
    {
        plc.Write(address, true);
        await Task.Delay(holdMilliseconds, cancellationToken).ConfigureAwait(false);
        plc.Write(address, false);
    }
}
