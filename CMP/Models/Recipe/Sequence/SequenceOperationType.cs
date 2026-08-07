namespace CMP.Models.Recipe.Sequence;

public enum SequenceType
{
    Serial,   // +
    Parallel,  // ×
    None, // /
}

public class SequenceOperationType
{
    public SequenceType Value { get; set; }
    public string? Name { get; set; }
}