namespace CMP.Models.Resource;

public sealed class WaferSequencePlan
{
    public List<WaferSequenceModel> WaferSequenceModels { get; set; } = [];

    public int[] SequnceValue { get; set; } = new int[16];
}
