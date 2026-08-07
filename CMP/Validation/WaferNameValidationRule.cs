using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace CMP.Validation;

internal static class WaferNameRules
{
    private static readonly char[] InvalidCharacters = Path.GetInvalidFileNameChars();

    internal const string ErrorMessage = "Wafer name contains characters that are not allowed in a file name.";

    internal static bool IsValid(string? waferName)
    {
        return string.IsNullOrEmpty(waferName) || waferName.IndexOfAny(InvalidCharacters) < 0;
    }
}

public sealed class WaferNameValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return WaferNameRules.IsValid(value?.ToString())
            ? ValidationResult.ValidResult
            : new ValidationResult(false, WaferNameRules.ErrorMessage);
    }
}
