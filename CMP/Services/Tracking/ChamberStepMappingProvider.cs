namespace CMP.Services.Tracking;

/// <summary>
/// Tracking 步号（1-based，与 WaferActionDescriptions.csv 的 Step 列一致）到各腔体进出/工艺的映射。
/// </summary>
public static class ChamberStepMappingProvider
{
    public readonly record struct ChamberStepMapping(
        string ChamberName,
        int EnterStartStep,
        int ExitEndStep,
        int ProcessStartStep,
        int ProcessEndStep);

    private static readonly ChamberStepMapping[] DefaultMappings =
    [
        new("PolishB", 45, 54, 45, 53),
        new("PolishA", 64, 74, 65, 73),
        new("RollerA1", 94, 118, 100, 117),
        new("RollerA2", 103, 134, 109, 133),
        new("RollerA3", 136, 156, 141, 155),
        new("PenA1", 143, 170, 148, 169),
        new("PenA2", 172, 184, 177, 183),
        new("DryA", 186, 200, 191, 198),
        new("PolishD", 237, 246, 237, 245),
        new("PolishC", 256, 266, 257, 265),
        new("RollerB1", 286, 310, 292, 309),
        new("RollerB2", 295, 326, 301, 325),
        new("RollerB3", 328, 348, 333, 347),
        new("PenB1", 335, 362, 340, 361),
        new("PenB2", 364, 376, 369, 375),
        new("DryB", 378, 392, 383, 390)
    ];

    public static IReadOnlyList<ChamberStepMapping> GetMappings() => DefaultMappings;
}
