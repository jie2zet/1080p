namespace CMP.Services.Tracking;

public enum ChamberVisitStatus
{
    Skipped,
    Partial,
    Completed
}

public sealed class WaferChamberVisitRecord
{
    public string ChamberName { get; init; } = string.Empty;
    public string RecipeName { get; init; } = string.Empty;
    public bool Planned { get; init; }

    public DateTime? EnterTime { get; init; }
    public DateTime? ExitTime { get; init; }
    public DateTime? ProcessStartTime { get; init; }
    public DateTime? ProcessEndTime { get; init; }

    public TimeSpan? DwellDuration =>
        EnterTime.HasValue && ExitTime.HasValue ? ExitTime.Value - EnterTime.Value : null;

    public TimeSpan? ProcessDuration =>
        ProcessStartTime.HasValue && ProcessEndTime.HasValue
            ? ProcessEndTime.Value - ProcessStartTime.Value
            : null;

    public ChamberVisitStatus Status { get; init; }
}
