namespace CMP.Models.Operation.Brush;

public class Brush
{
    public Chamber Chamber { get; set; } = new();
    public Motor AboveRollerMotor { get; set; } = new();
    public Motor WaferRotateMotor { get; set; } = new();
    public Motor BeneathRollerMotor { get; set; } = new();
}