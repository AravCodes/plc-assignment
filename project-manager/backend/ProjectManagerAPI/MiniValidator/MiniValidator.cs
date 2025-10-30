using System.ComponentModel.DataAnnotations;

namespace MiniValidator;

public static class MiniValidator
{
    public static bool TryValidate(object? instance, out Dictionary<string, string[]> errors)
    {
        var validationContext = new ValidationContext(instance!);
        var validationResults = new List<ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            instance!, validationContext, validationResults, validateAllProperties: true);

        errors = validationResults
            .SelectMany(r => r.MemberNames.Select(m => (m, r.ErrorMessage ?? "Invalid")))
            .GroupBy(t => t.m)
            .ToDictionary(g => g.Key, g => g.Select(t => t.Item2).ToArray());
        return isValid;
    }
}


